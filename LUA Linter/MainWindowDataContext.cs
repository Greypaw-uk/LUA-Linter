using System.Collections.ObjectModel;
using System.ComponentModel;

namespace LUA_Linter
{
    public class MainWindowDataContext
    {
        public static ObservableCollection<luaFileContents> luaFileInfo { get; set; } = new ObservableCollection<luaFileContents>();

        public event PropertyChangedEventHandler PropertyChanged;
    }
}