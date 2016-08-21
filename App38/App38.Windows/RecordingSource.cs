using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;

namespace App38
{
    public class RecordingSource
    {

        private ObservableCollection<Recording> _recordings;

        public ObservableCollection<Recording> Recordings
        {
            get
            {
                if (this._recordings == null)
                    this._recordings = new ObservableCollection<Recording>();

                return this._recordings;
            }
        }
        public async Task<Recording> GetRecordingAsync(string name)
        {
            foreach(Recording recording in (Collection<Recording>)this.Recordings)
            {
                if (string.Compare(recording.Title, name, StringComparison.Ordinal) == 0)
                    return recording;
            }
            return (Recording)null;

        }

        public async Task GetRecordingsAsync()
        {
            int pos = 0;
            try
            {
                StorageFolder libraryFolder = KnownFolders.MusicLibrary;
                StorageFolder storageFolder = null;
                try
                {
                    storageFolder = await libraryFolder.GetFolderAsync("Audio Recordings");

                }catch(Exception ex)
                {
                    if (storageFolder == null)
                        return;

                }
                foreach(StorageFile storageFile in (IEnumerable<StorageFile>) await storageFolder.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.OrderByName))
                {
                    Recording recording = new Recording();
                    MusicProperties properties = await storageFile.Properties.GetMusicPropertiesAsync();
                    recording.Length = (properties.Duration).ToString();
                    recording.Length = recording.Length.Substring(0, recording.Length.IndexOf("."));
                    IRandomAccessStreamWithContentType fileStream = await storageFile.OpenReadAsync();
                    recording.Stream = (IRandomAccessStream)fileStream;
                    if(properties.Title == string.Empty)
                    {
                        properties.Title = storageFile.DisplayName;
                        await properties.SavePropertiesAsync();

                    }

                    recording.FileType = storageFile.FileType;
                    recording.Title = storageFile.DisplayName;
                    ++pos;
                    recording.Position = pos.ToString();
                    this.Recordings.Add(recording);

                }
            }
            catch
            {

            }
        }


    }
}