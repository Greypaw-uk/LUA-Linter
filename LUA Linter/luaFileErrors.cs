using System.ComponentModel;
using System.Windows.Data;

namespace LUA_Linter
{
    public class luaFileErrors : INotifyPropertyChanged
    {
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