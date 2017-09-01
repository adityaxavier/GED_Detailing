using GED.Common;
using GED.Data;
using GED.DataModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Web;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace GED
{
    public sealed partial class ItemDetailPage : Page
    {
        private List<DownloadOperation> activeDownloads;
        private CancellationTokenSource cts;
        private Database db;
        private String UniqueId;
        private String StartTime;

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private String fileType;

        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        async void onResume(Object sender, Object e)
        {
            StartTime = GetTimeStamp(DateTime.Now);
        }

        private void onSuspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            db = new Database();
            db.CreateTable();
            TrackRecord record = new TrackRecord();
            record.UniqueId = UniqueId;
            record.StartTime = StartTime;
            record.EndTime = GetTimeStamp(DateTime.Now);

            db.InsertRecord(record);
        }

        public ItemDetailPage()
        {
            cts = new CancellationTokenSource();
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;

            Application.Current.Suspending += new Windows.UI.Xaml.SuspendingEventHandler(onSuspending);
            Application.Current.Resuming += new EventHandler<Object>(onResume);
        }

        public void Dispose()
        {
            if (cts != null)
            {
                cts.Dispose();
                cts = null;
            }

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            var file = await SampleDataSource.GetFilesAsync((String)e.NavigationParameter);
            this.DefaultViewModel["Files"] = file;
            String fileName = file.UniqueId;
            String fileUrl = file.MediaPath;
            UniqueId = file.UniqueId;
            fileType = file.MediaType;

            //var appFolder = await KnownFolders.PicturesLibrary.GetFolderAsync("GlobalSpace");
            var appFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            appFolder = await appFolder.GetFolderAsync("Globalspace");
            appFolder = await appFolder.GetFolderAsync("Media");
            string path = appFolder.Path;
            if (await appFolder.TryGetItemAsync(fileName) != null)
            {
                MarshalUI(path + "\\" + fileName);
            }
            else
            {
                StartDownload(BackgroundTransferPriority.High, false, fileUrl, fileName);
            }
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
            await DiscoverActiveDownloadsAsync();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        // Enumerate the downloads that were going on in the background while the app was closed.
        private async Task DiscoverActiveDownloadsAsync()
        {
            activeDownloads = new List<DownloadOperation>();

            IReadOnlyList<DownloadOperation> downloads = null;
            try
            {
                downloads = await BackgroundDownloader.GetCurrentDownloadsAsync();
            }
            catch (Exception ex)
            {
                if (!IsExceptionHandled("Discovery error", ex))
                {
                    throw;
                }
                return;
            }


            if (downloads.Count > 0)
            {
                List<Task> tasks = new List<Task>();
                foreach (DownloadOperation download in downloads)
                {
                    // Attach progress and completion handlers.
                    String filePath = "";
                    tasks.Add(HandleDownloadAsync(download, false, filePath));
                }

                // Don't await HandleDownloadAsync() in the foreach loop since we would attach to the second
                // download only when the first one completed; attach to the third download when the second one
                // completes etc. We want to attach to all downloads immediately.
                // If there are actions that need to be taken once downloads complete, await tasks here, outside
                // the loop.
                await Task.WhenAll(tasks);
            }
        }

        private async void StartDownload(BackgroundTransferPriority priority, bool requestUnconstrainedDownload, string url, string fileName)
        {
            // Validating the URI is required since it was received from an untrusted source (user input).
            // The URI is validated by calling Uri.TryCreate() that will return 'false' for strings that are not valid URIs.
            // Note that when enabling the text box users may provide URIs to machines on the intrAnet that require
            // the "Home or Work Networking" capability.
            Uri source;
            if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out source))
            {
                //NotifyUser("Invalid URI.", NotifyType.ErrorMessage);
                return;
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                //NotifyUser("A local file name is required.", NotifyType.ErrorMessage);
                return;
            }

            StorageFile destinationFile;
            String filePath;

            //var appFolder = await KnownFolders.PicturesLibrary.GetFolderAsync("GlobalSpace");
            
            var appFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            appFolder = await appFolder.GetFolderAsync("Globalspace");
            appFolder = await appFolder.GetFolderAsync("Media");

            try
            {

                destinationFile = await appFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                filePath = destinationFile.Path;
                //destinationFile = await appFolder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);
            }
            catch (FileNotFoundException ex)
            {
                return;
            }

            BackgroundDownloader downloader = new BackgroundDownloader();
            DownloadOperation download = downloader.CreateDownload(source, destinationFile);

            download.Priority = priority;

            if (!requestUnconstrainedDownload)
            {
                // Attach progress and completion handlers.
                await HandleDownloadAsync(download, true, filePath);
                return;
            }

            List<DownloadOperation> requestOperations = new List<DownloadOperation>();
            requestOperations.Add(download);

            UnconstrainedTransferRequestResult result;
            try
            {
                result = await BackgroundDownloader.RequestUnconstrainedDownloadsAsync(requestOperations);
            }
            catch (NotImplementedException)
            {
                return;
            }

            await HandleDownloadAsync(download, true, filePath);
        }

        // Note that this event is invoked on a background thread, so we cannot access the UI directly.
        private void DownloadProgress(DownloadOperation download)
        {
            //MarshalLog(String.Format(CultureInfo.CurrentCulture, "Progress: {0}, Status: {1}", download.Guid,
            //    download.Progress.Status));

            double percent = 100;
            if (download.Progress.TotalBytesToReceive > 0)
            {
                percent = download.Progress.BytesReceived * 100 / download.Progress.TotalBytesToReceive;
            }

            //MarshalLog(String.Format(CultureInfo.CurrentCulture, " - Transfered bytes: {0} of {1}, {2}%",
            //    download.Progress.BytesReceived, download.Progress.TotalBytesToReceive, percent));

            if (download.Progress.HasRestarted)
            {
                //MarshalLog(" - Download restarted");
            }

            if (download.Progress.HasResponseChanged)
            {
                // We've received new response headers from the server.
                //MarshalLog(" - Response updated; Header count: " + download.GetResponseInformation().Headers.Count);

                // If you want to stream the response data this is a good time to start.
                // download.GetResultStreamAt(0);
            }
        }

        private async Task HandleDownloadAsync(DownloadOperation download, bool start, String filePath)
        {
            try
            {
                //LogStatus("Running: " + download.Guid, NotifyType.StatusMessage);

                // Store the download so we can pause/resume.
                activeDownloads.Add(download);

                Progress<DownloadOperation> progressCallback = new Progress<DownloadOperation>(DownloadProgress);
                if (start)
                {
                    // Start the download and attach a progress handler.
                    await download.StartAsync().AsTask(cts.Token, progressCallback);
                }
                else
                {
                    // The download was already running when the application started, re-attach the progress handler.
                    await download.AttachAsync().AsTask(cts.Token, progressCallback);
                }

                ResponseInformation response = download.GetResponseInformation();

                /*LogStatus(String.Format(CultureInfo.CurrentCulture, "Completed: {0}, Status Code: {1}",
                    download.Guid, response.StatusCode), NotifyType.StatusMessage);
                 */
            }
            catch (TaskCanceledException)
            {
                //LogStatus("Canceled: " + download.Guid, NotifyType.StatusMessage);
            }
            catch (Exception ex)
            {
                if (!IsExceptionHandled("Execution error", ex, download))
                {
                    throw;
                }
            }
            finally
            {
                activeDownloads.Remove(download);
                MarshalUI(filePath);
            }
        }

        private bool IsExceptionHandled(string title, Exception ex, DownloadOperation download = null)
        {
            WebErrorStatus error = BackgroundTransferError.GetStatus(ex.HResult);
            if (error == WebErrorStatus.Unknown)
            {
                return false;
            }

            if (download == null)
            {
                /*LogStatus(String.Format(CultureInfo.CurrentCulture, "Error: {0}: {1}", title, error),
                    NotifyType.ErrorMessage);
                 */
            }
            else
            {
                /*LogStatus(String.Format(CultureInfo.CurrentCulture, "Error: {0} - {1}: {2}", download.Guid, title,
                    error), NotifyType.ErrorMessage);
                 */
            }

            return true;
        }

        // When operations happen on a background thread we have to marshal UI updates back to the UI thread.
        private async void MarshalUI(String filePath)
        {
            StorageFile file = await StorageFile.GetFileFromPathAsync(filePath);
            IRandomAccessStream fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
            StartTime = GetTimeStamp(DateTime.Now);

            if (fileType == "Image") { 
                BitmapImage bitmapImage = new BitmapImage();
                 await bitmapImage.SetSourceAsync(fileStream);
                ImagePage.Source = bitmapImage;
            }
            else
            {
                if (fileType == "Video")
                {
                    VideoPage.SetSource(fileStream, file.ContentType);
                }
            }

            var ignore = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                progressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                if (fileType == "Image")
                {
                    ImagePage.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }
                else
                {
                    if (fileType == "Video")
                    {
                        VideoPage.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    }
                }
            });
        }

        private void backButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            db = new Database();
            db.CreateTable();
            TrackRecord record = new TrackRecord();
            record.UniqueId = UniqueId;
            record.StartTime = StartTime;
            record.EndTime = GetTimeStamp(DateTime.Now);

            db.InsertRecord(record);
        }

        public static String GetTimeStamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmss");
        }
    }
}
