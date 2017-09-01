using GED.Common;
using GED.DataModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Web;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace GED
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class SyncScreen : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        bool Localhost = true;

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
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


        public SyncScreen()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;

            try
            {
                Database db = new Database();
                db.CreateTable();
                createFolders();
                checkJsonAndInternet();
            }
            catch
            {

            }

        }

        private async void sendTracking(String TrackingURL)
        {
            Database db = new Database();
            var result = await db.GetRecords();

            List<TrackRecord> Record = new List<TrackRecord>();
            bool recordsExist = false;
            foreach (var item in result)
            {
                recordsExist = true;
                Record.Add(new TrackRecord { Id = item.Id, StartTime = item.StartTime, UniqueId = item.UniqueId, EndTime = item.EndTime });
            }

            if (recordsExist)
            {
                string output = JsonConvert.SerializeObject(Record);
                Uri uri = new Uri(TrackingURL);

                HttpClient aClient = new HttpClient();
                aClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                aClient.DefaultRequestHeaders.Host = uri.Host;

                HttpContent content = new FormUrlEncodedContent(new[] 
            {
                new KeyValuePair<string, string>("Json", output)
            });
                try
                {
                    HttpResponseMessage aResponse = await aClient.PostAsync(uri, content);
                    string responsestring = await aResponse.Content.ReadAsStringAsync();
                    db.DeleteTable();
                }
                catch (HttpRequestException e)
                {
                    // MarshalErrorUI();
                }
            }
        }

        private async void DownloadAndSync(String url, String TrackingURL)
        {
            try
            {
                Uri uri = new Uri(url);
                HttpClient aClient = new HttpClient();
                aClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                aClient.DefaultRequestHeaders.Host = uri.Host;
                string aResponse = await aClient.GetStringAsync(uri);

                jsonData jData = new jsonData();
                jData.jsonitem = aResponse;

                Database db = new Database();
                var result = await db.GetJsonData();

                List<Json> Record = new List<Json>();
                bool recordsExist = false;
                foreach (var item in result)
                {
                    recordsExist = true;
                }

                if (recordsExist)
                {
                    db.UpdateJson(jData);
                }
                else
                {
                    db.InsertJson(jData);
                }
                
                sendTracking(TrackingURL);

                MarshalUI();

            }

            catch (HttpRequestException e)
            {
                MarshalErrorConnectionUI();
            }
        }

        public static bool IsInternet(bool localhost)
        {
            bool internet;
            if (localhost)
            {
                internet = true;
            }
            else
            {
                ConnectionProfile connections = NetworkInformation.GetInternetConnectionProfile();
                internet = connections != null && connections.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
            }

            return internet;
        }

        public class TrackRecord
        {
            public int Id;
            public string UniqueId;
            public string StartTime;
            public string EndTime;
        }
        public class Json
        {
            public int Id;
            public string jsonitem;
        }


        private async void checkJsonAndInternet()
        {
            string fetchJsonurl = "";
            string TrackingURL = "";

            if (Localhost)
            {
                fetchJsonurl = "http://localhost/treeproject/json2.php";
                TrackingURL = "http://localhost/treeproject/tracking_json.php";
            }
            else
            {
                fetchJsonurl = "http://52.74.96.176/GED/json2.php";
                TrackingURL = "http://52.74.96.176/GED/tracking_json.php";
                //fetchJsonurl = "http://192.168.2.180/treeproject/json2.php";
                //TrackingURL = "http://192.168.2.180/treeproject/tracking_json.php";
            }
            try
            {
                if (IsInternet(Localhost))
                {
                    DownloadAndSync(fetchJsonurl, TrackingURL);
                }
                else
                {

                    if (await IsDatabaseExistsAsync("Track.db"))
                    {
                        //sendTracking(TrackingURL);
                        //DownloadHttpclient(fetchJsonurl, jsonFilename);
                        MarshalUI();
                    }
                    else
                    {
                        MarshalErrorUI();
                    }
                }
            }

            catch (IOException e)
            {
                if (Localhost)
                {
                    MarshalErrorLocalUI();
                }
                else
                {
                    MarshalErrorUI();
                }
            }
        }
        public async Task<bool> IsDatabaseExistsAsync(string dbName)
        {
            try
            {
                var dbFile = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFileAsync(dbName);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async void createFolders()
        {
            try
            {
                var appFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                appFolder = await appFolder.CreateFolderAsync("Globalspace", CreationCollisionOption.OpenIfExists);

                await appFolder.CreateFolderAsync("json", CreationCollisionOption.OpenIfExists);
                await appFolder.CreateFolderAsync("Media", CreationCollisionOption.OpenIfExists);


            }
            catch (IOException e)
            {
                return;
            }

        }

        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion



        // When operations happen on a background thread we have to marshal UI updates back to the UI thread.
        private void MarshalUI()
        {
            var ignore = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                //Log(value);
                progressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                nextButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
            });
        }


        private void MarshalErrorUI()
        {
            progressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            NoInternet.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        private void MarshalErrorLocalUI()
        {
            progressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            NoLocalHost.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        private void MarshalErrorConnectionUI()
        {
            progressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            ConnectionErr.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        private void Next_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(GroupedSubVerticalPage));
        }
    }
}
