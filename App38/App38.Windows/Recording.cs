using System.ComponentModel;
using Windows.Storage.Streams;

namespace App38
{
    public  class Recording : INotifyPropertyChanged
    {

        private string _title;
        private string _length;
        private IRandomAccessStream _stream;
        private string _position;
        private string _fileType;

        public string Title
        {
            get
            {
                return this._title;
            }
            set
            {
                this._title = value;
                this.OnPropertyChanged("Title");
            }
        }
        public string Length
        {
            get
            {
                return this._length;
            }
            set
            {
                this._length = value;
                this.OnPropertyChanged("Length");
            }
        }

        public IRandomAccessStream Stream
        {
            get
            {
                return this._stream;
            }
            set
            {
                this._stream = value;
                this.OnPropertyChanged("Stream");
            }
        }

        public string Position
        {
            get
            {
                return this._position;
            }
            set
            {
                this._position = value;
                this.OnPropertyChanged("Position");
            }
        }

        public string FileType
        {
            get
            {
                return this._fileType;
            }
            set
            {
                this._fileType = value;
                this.OnPropertyChanged("FileType");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler changedEventHandler = this.PropertyChanged;
            if (changedEventHandler == null)
                return;
            changedEventHandler((object)this, new PropertyChangedEventArgs(propertyName));
        }




    }
}