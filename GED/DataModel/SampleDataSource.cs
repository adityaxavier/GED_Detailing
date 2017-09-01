using GED.DataModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The data model defined by this file serves as a representative example of a strongly-typed
// model.  The property names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs. If using this model, you might improve app 
// responsiveness by initiating the data loading task in the code behind for App.xaml when the app 
// is first launched.

namespace GED.Data
{

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class DataVertical
    {
        public DataVertical(String uniqueId, String title, String subtitle, String imagePath, String content,String description)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Subtitle = subtitle;
            this.Description = description;
            this.ImagePath = imagePath;
            this.Content = content;
            this.SubVertical = new ObservableCollection<DataSubVertical>();
        }

        public string UniqueId { get; private set; }
        public string Title { get; private set; }
        public string Subtitle { get; private set; }
        public string Description { get; private set; }
        public string ImagePath { get; private set; }
        public string Content { get; private set; }
        public ObservableCollection<DataSubVertical> SubVertical { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class DataSubVertical
    {
        
        public DataSubVertical(String uniqueId, String title, String subtitle, String imagePath, string content ,String description)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Subtitle = subtitle;
            this.Description = description;
            this.ImagePath = imagePath;
            this.Content = content;
            this.Topic = new ObservableCollection<DataTopic>();
        }

        public string UniqueId { get; private set; }
        public string Title { get; private set; }
        public string Subtitle { get; private set; }
        public string Description { get; private set; }
        public string ImagePath { get; private set; }
        public string Content { get; private set; }
        public ObservableCollection<DataTopic> Topic { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }
    }

    // Generic topic data model
    public class DataTopic
    {
        public DataTopic(String uniqueId, String title, String subtitle, String imagePath, String description, String content)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Subtitle = subtitle;
            this.Description = description;
            this.ImagePath = imagePath;
            this.Content = content;
            this.SubTopic = new ObservableCollection<DataSubTopic>();
        }

        public string UniqueId { get; private set; }
        public string Title { get; private set; }
        public string Subtitle { get; private set; }
        public string Description { get; private set; }
        public string ImagePath { get; private set; }
        public string Content { get; private set; }
        public ObservableCollection<DataSubTopic> SubTopic { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }
    }

    // Generic Subtopic data model
    public class DataSubTopic
    {
        public DataSubTopic(String uniqueId, String title, String subtitle, String imagePath, String description, String content)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Subtitle = subtitle;
            this.Description = description;
            this.ImagePath = imagePath;
            this.Content = content;
            this.Media = new ObservableCollection<DataMedia>();
        }

        public string UniqueId { get; private set; }
        public string Title { get; private set; }
        public string Subtitle { get; private set; }
        public string Description { get; private set; }
        public string ImagePath { get; private set; }
        public string Content { get; private set; }
        public ObservableCollection<DataMedia> Media { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }
    }

    // Generic Media
    public class DataMedia
    {
        public DataMedia(String uniqueId, String title, String subtitle, String imagePath, String description, String content)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Subtitle = subtitle;
            this.Description = description;
            this.ImagePath = imagePath;
            this.Content = content;
            this.Files = new ObservableCollection<DataFiles>();
        }

        public string UniqueId { get; private set; }
        public string Title { get; private set; }
        public string Subtitle { get; private set; }
        public string Description { get; private set; }
        public string ImagePath { get; private set; }
        public string Content { get; private set; }
        public ObservableCollection<DataFiles> Files { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }
    }

    public class DataFiles
    {
        public DataFiles(String uniqueId, String title, String subtitle, String imagePath, String description, String content, String mediaPath, String mediaType)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Subtitle = subtitle;
            this.ImagePath = imagePath;
            this.Description = description;
            this.Content = content;
            this.MediaPath = mediaPath;
            this.MediaType = mediaType;
        }

        public string UniqueId { get; private set; }
        public string Title { get; private set; }
        public string Subtitle { get; private set; }
        public string ImagePath { get; private set; }
        public string Description { get; private set; }
        public string Content { get; private set; }
        public string MediaPath { get; private set; }
        public string MediaType { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }
    }



    /// <summary>
    /// Creates a collection of groups and items with content read from a static json file.
    /// 
    /// SampleDataSource initializes with data read from a static json file included in the 
    /// project.  This provides sample data at both design-time and run-time.
    /// </summary>
    public sealed class SampleDataSource
    {
       
        private static SampleDataSource _sampleDataSource = new SampleDataSource();

        private ObservableCollection<DataVertical> _vertical = new ObservableCollection<DataVertical>();
        public ObservableCollection<DataVertical> Vertical
        {
            get { return this._vertical; }
        }

        public static async Task<IEnumerable<DataVertical>> GetVerticalAsync()
        {
            await _sampleDataSource.GetSampleDataAsync();

            return _sampleDataSource.Vertical;
        }

        public static async Task<DataVertical> GetVerticalAsync(string uniqueId)
        {
            await _sampleDataSource.GetSampleDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.Vertical.Where((Vertical) => Vertical.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        private ObservableCollection<DataSubVertical> _subvertical = new ObservableCollection<DataSubVertical>();
        public ObservableCollection<DataSubVertical> SubVertical
        {
            get { return this._subvertical; }
        }

        public static async Task<IEnumerable<DataSubVertical>> GetSubVerticalAsync()
        {
            await _sampleDataSource.GetSampleDataAsync();

            return _sampleDataSource.SubVertical;
        }

        public static async Task<DataSubVertical> GetSubVerticalAsync(string uniqueId)
        {
            await _sampleDataSource.GetSampleDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.Vertical.SelectMany(Vertical => Vertical.SubVertical).Where((SubVertical) => SubVertical.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        private ObservableCollection<DataTopic> _topic = new ObservableCollection<DataTopic>();
        public ObservableCollection<DataTopic> Topic
        {
            get { return this._topic; }
        }

        public static async Task<IEnumerable<DataTopic>> GetTopicAsync()
        {
            await _sampleDataSource.GetSampleDataAsync();

            return _sampleDataSource.Topic;
        }

        public static async Task<DataTopic> GetTopicAsync(string uniqueId)
        {
            await _sampleDataSource.GetSampleDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.SubVertical.SelectMany(SubVertical => SubVertical.Topic).Where((Topic) => Topic.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        private ObservableCollection<DataSubTopic> _subtopic = new ObservableCollection<DataSubTopic>();
        public ObservableCollection<DataSubTopic> SubTopic
        {
            get { return this._subtopic; }
        }

        public static async Task<IEnumerable<DataSubTopic>> GetSubTopicAsync()
        {
            await _sampleDataSource.GetSampleDataAsync();

            return _sampleDataSource.SubTopic;
        }

        public static async Task<DataSubTopic> GetSubTopicAsync(string uniqueId)
        {
            await _sampleDataSource.GetSampleDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.Topic.SelectMany(Topic => Topic.SubTopic).Where((SubTopic) => SubTopic.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        private ObservableCollection<DataMedia> _media = new ObservableCollection<DataMedia>();
        public ObservableCollection<DataMedia> Media
        {
            get { return this._media; }
        }

        public static async Task<IEnumerable<DataMedia>> GetMediaAsync()
        {
            await _sampleDataSource.GetSampleDataAsync();

            return _sampleDataSource.Media;
        }

        public static async Task<DataMedia> GetMediaAsync(string uniqueId)
        {
            await _sampleDataSource.GetSampleDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.SubTopic.SelectMany(SubTopic => SubTopic.Media).Where((Media) => Media.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static async Task<DataFiles> GetFilesAsync(string uniqueId)
        {
            await _sampleDataSource.GetSampleDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.Media.SelectMany(Media => Media.Files).Where((Files) => Files.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        private async Task GetSampleDataAsync()
        {
            if (this._vertical.Count != 0)
                return;

          

            Database db = new Database();
            var jsonText = await db.GetJson();
            string jText = jsonText.jsonitem;

            JsonObject jsonObject = JsonObject.Parse(jText);
            JsonArray jsonArray = jsonObject["Vertical"].GetArray();

            foreach (JsonValue VerticalValue in jsonArray)
            {
                JsonObject VerticalObject = VerticalValue.GetObject();
                DataVertical Vertical = new DataVertical(VerticalObject["UniqueId"].GetString(),
                                                            VerticalObject["Title"].GetString(),
                                                            VerticalObject["Subtitle"].GetString(),
                                                            VerticalObject["ImagePath"].GetString(),
                                                            VerticalObject["Description"].GetString(),
                                                            VerticalObject["Content"].GetString());

                foreach (JsonValue SubVerticalValue in VerticalObject["SubVertical"].GetArray())
                {
                    JsonObject SubVerticalObject = SubVerticalValue.GetObject();
                    
                    DataSubVertical SubVertical = new DataSubVertical(SubVerticalObject["UniqueId"].GetString(),
                                                       SubVerticalObject["Title"].GetString(),
                                                       SubVerticalObject["Subtitle"].GetString(),
                                                       SubVerticalObject["ImagePath"].GetString(),
                                                       SubVerticalObject["Description"].GetString(),
                                                       SubVerticalObject["Content"].GetString());

                    Vertical.SubVertical.Add(SubVertical);

                    foreach (JsonValue TopicValue in SubVerticalObject["Topic"].GetArray())
                    {
                        JsonObject TopicObject = TopicValue.GetObject();
                        DataTopic Topic = new DataTopic(TopicObject["UniqueId"].GetString(),
                                                           TopicObject["Title"].GetString(),
                                                           TopicObject["Subtitle"].GetString(),
                                                           TopicObject["ImagePath"].GetString(),
                                                           TopicObject["Description"].GetString(),
                                                           TopicObject["Content"].GetString());
                        SubVertical.Topic.Add(Topic);


                        foreach (JsonValue SubTopicValue in TopicObject["SubTopic"].GetArray())
                        {
                            JsonObject SubTopicObject = SubTopicValue.GetObject();
                            DataSubTopic SubTopic = new DataSubTopic(SubTopicObject["UniqueId"].GetString(),
                                                               SubTopicObject["Title"].GetString(),
                                                               SubTopicObject["Subtitle"].GetString(),
                                                               SubTopicObject["ImagePath"].GetString(),
                                                               SubTopicObject["Description"].GetString(),
                                                               SubTopicObject["Content"].GetString());
                            Topic.SubTopic.Add(SubTopic);

                            foreach (JsonValue MediaValue in SubTopicObject["Media"].GetArray())
                            {
                                JsonObject MediaObject = MediaValue.GetObject();
                                DataMedia Media = new DataMedia(MediaObject["UniqueId"].GetString(),
                                                                   MediaObject["Title"].GetString(),
                                                                   MediaObject["Subtitle"].GetString(),
                                                                   MediaObject["ImagePath"].GetString(),
                                                                   MediaObject["Description"].GetString(),
                                                                   MediaObject["Content"].GetString());
                                SubTopic.Media.Add(Media);

                                foreach (JsonValue FilesValue in MediaObject["Files"].GetArray())
                                {
                                    JsonObject FilesObject = FilesValue.GetObject();
                                    Media.Files.Add(new DataFiles(FilesObject["UniqueId"].GetString(),
                                                                       FilesObject["Title"].GetString(),
                                                                       FilesObject["Subtitle"].GetString(),
                                                                       FilesObject["ImagePath"].GetString(),
                                                                       FilesObject["Description"].GetString(),
                                                                       FilesObject["Content"].GetString(),
                                                                       FilesObject["MediaPath"].GetString(),
                                                                       FilesObject["MediaType"].GetString()));

                                }
                                this.Media.Add(Media);

                            }
                            this.SubTopic.Add(SubTopic);

                        }
                        this.Topic.Add(Topic);

                    }
                    this.SubVertical.Add(SubVertical);

                }
                this.Vertical.Add(Vertical);
            }
        }
    }
}