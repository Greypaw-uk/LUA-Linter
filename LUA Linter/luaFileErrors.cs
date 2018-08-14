using System.ComponentModel;
using System.Windows.Data;

namespace LUA_Linter
{
    public class LuaFileErrors : INotifyPropertyChanged
    {
        private string _contentError;
        public string ContentError
        {
            get { return _contentError; }
            set
            {
                _contentError = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ContentError"));
            }
        }

        private int _errorLine;

        public int ErrorLine
        {
            get { return _errorLine; }
            set
            {
                _errorLine = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ErrorLine"));
            }
        }

        public ListCollectionView SelectedListEntry { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}