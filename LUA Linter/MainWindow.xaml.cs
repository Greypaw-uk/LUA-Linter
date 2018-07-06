using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace LUA_Linter
{

    public partial class MainWindow : Window
    {
        MainWindowDataContext context = new MainWindowDataContext();

        private int lineNumber;
        public FileInfo LuaFile;
        private string line;
        private string error;


        public MainWindow()
        {
            InitializeComponent();

            DataContext = context;
        }


        private void LoadFile(object sender, RoutedEventArgs e)
        {
            ClearList();
            lineNumber = 0;

            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Lua files (*.lua)|*.lua";

            Nullable<bool> result = openFile.ShowDialog();

            Dictionary<string, string> commonMistakes = new Dictionary<string, string>
            {
                {"say(", "SAY(" },
                {"Say(", "SAY(" },
                {"Answer(", "ANSWER(" },
                {"answer(", "ANSWER(" }
            };

            //if valid file is selected, open file, repeatedly add line to datacontext  until the end
            if (result == true)
            {
                StreamReader fileContents = new StreamReader(openFile.FileName);
                
                while ((line = fileContents.ReadLine()) != null)
                {
                    lineNumber++; // adds line numbers to listbox items

                    // if in error dictionary, add to error list
                    foreach (var keyValuePair in commonMistakes)
                    {
                        if (line.Contains(keyValuePair.Key))
                        {
                            var dicKey = keyValuePair.Key;
                            var dicValue = keyValuePair.Value;
                            MainWindowDataContext.errorList.Add(new luaFileErrors
                            {
                                contentError = lineNumber + " " + line  + "\r" + "Should '"  + dicKey + "' be '" + dicValue + "'?" + "\r"
                            });
                        }
                    }
                    
                    // display contents of Lua file in listbox
                    MainWindowDataContext.luaFileInfo.Add(new luaFileContents
                    {
                        fileName = openFile.FileName,
                        body = lineNumber + "   " + line
                    });
                }
            }
            else
            {
                MessageBox.Show("Please select a file");
            }
        }

 
        private void ClearList()
        {
            ObservableCollection<luaFileContents> itemsToRemove = new ObservableCollection<luaFileContents>();

            foreach (luaFileContents item in lb_display.Items)
            {
                itemsToRemove.Add(item);
            }

            foreach (luaFileContents item in itemsToRemove)
            {
                ((ObservableCollection<luaFileContents>)lb_display.ItemsSource).Remove(item);
            }
        }
    }
}
