using System.ComponentModel;
using System.Windows.Data;

namespace LUA_Linter
{
    public class luaFileContents : INotifyPropertyChanged
    {
        private string _body;

        public string body
        {
            get { return _body; }
            set
            {
                _body = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("body"));
            }
        }

        private string _fileName;
        public string fileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("fileName"));
            }
        }

        private string _lineNumber;
        public string lineNumber
        {
            get { return _lineNumber; }
            set
            {
                _lineNumber = value; 
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("lineNumber"));
            }
        }

        private string _contentError;

        public string contentError
        {
            get { return _contentError; }
            set
            {
                _contentError = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("contentError"));
            }
        }

        public ListCollectionView selectedListEntry { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}