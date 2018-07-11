using System.Collections.ObjectModel;
using System.ComponentModel;

namespace LUA_Linter
{
    public class MainWindowDataContext
    {
        public static ObservableCollection<luaFileContents> LuaFileInfo { get; set; } = new ObservableCollection<luaFileContents>();

        public static ObservableCollection<LuaFileErrors> ErrorList { get; set; } = new ObservableCollection<LuaFileErrors>();

        public event PropertyChangedEventHandler PropertyChanged;
    }
}