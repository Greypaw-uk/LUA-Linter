using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace LUA_Linter
{

    public partial class MainWindow : Window
    {
        MainWindowDataContext context = new MainWindowDataContext();

        private int lineNumber;
        public FileInfo LuaFile;
        private string _line;


        public MainWindow()
        {
            InitializeComponent();

            DataContext = context;
        }


        private void LoadFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog
            {
                Filter = "Lua files (*.lua)|*.lua"
            };

            Nullable<bool> result = openFile.ShowDialog();


            //if valid file is selected, open file, repeatedly add line to datacontext  until the end
            if (result == true)
            {
                ClearDisplayList();
                clearErrorList();
                lineNumber = 0;

                StreamReader fileContents = new StreamReader(openFile.FileName);
                
                while ((_line = fileContents.ReadLine()) != null)
                {
                    lineNumber++; // adds line numbers to listbox items

                    // display contents of Lua file in listbox
                    MainWindowDataContext.luaFileInfo.Add(new luaFileContents
                    {
                        body = lineNumber + "   " + _line
                    });
                }

                checkForErrors();
            }
            else
            {
                MessageBox.Show("Please select a file");
            }
        }


        private void checkForErrors()
        {
            Dictionary<string, string> commonMistakes = new Dictionary<string, string>
            {
                {"say(", "SAY(" },
                {"Say(", "SAY(" },
                {"Answer(", "ANSWER(" },
                {"answer(", "ANSWER(" }
            };

            foreach (var line in MainWindowDataContext.luaFileInfo)
            {
                var value = line.body;

                // if in error dictionary, add to error list
                foreach (var keyValuePair in commonMistakes)
                {
                    if (value.Contains(keyValuePair.Key))
                    {
                        var dicKey = keyValuePair.Key;
                        var dicValue = keyValuePair.Value;

                        MainWindowDataContext.errorList.Add(new luaFileErrors
                        {
                            contentError = value + "\r" + "Should '" + dicKey + "' be '" + dicValue + "'?" + "\r"
                        });
                    }
                }
            }
        }


        private void scanFile(object sender, RoutedEventArgs e)
        {
            clearErrorList();
            checkForErrors();
        }


        private void ClearDisplayList()
        {
            ObservableCollection<luaFileContents> itemsToRemove = new ObservableCollection<luaFileContents>();

            foreach (luaFileContents item in lb_display.Items)
            {
                //Console.WriteLine("Removing" + item.body + "from display list...");
                itemsToRemove.Add(item);
            }

            foreach (luaFileContents item in itemsToRemove)
            {
                //Console.WriteLine("Removed " + item.body + "from display list...");
                ((ObservableCollection<luaFileContents>)lb_display.ItemsSource).Remove(item);
            }
        }

        private void clearErrorList()
        {
            ObservableCollection<luaFileErrors> errorsToRemove = new ObservableCollection<luaFileErrors>();

            foreach (luaFileErrors item in lb_errorDisplay.Items)
            {
                //Console.WriteLine("Removing" + item.contentError + "from error list...");
                errorsToRemove.Add(item);
            }

            foreach (luaFileErrors item in errorsToRemove)
            {
                //Console.WriteLine("Removed " + item.contentError + "from error list...");
                ((ObservableCollection<luaFileErrors>) lb_errorDisplay.ItemsSource).Remove(item);
            }
        }
    }
}
