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

        List<string> errorList = new List<string>();

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

            //if valid file is selected, open file, repeatedly add line to datacontext  until the end
            if (result == true)
            {
                StreamReader fileContents = new StreamReader(openFile.FileName);
                
                while ((line = fileContents.ReadLine()) != null)
                {
                    CheckForErrors(); // check if line exists in error dictionary

                    lineNumber++; // used for displaying line number for easy user reference

                    MainWindowDataContext.luaFileInfo.Add(new luaFileContents
                    {
                        fileName = openFile.FileName,
                        body = lineNumber + "   " + line,
                        contentError = error
                    });
                }
            }
            else
            {
                MessageBox.Show("Please select a file");
            }
        }


        private void CheckForErrors()
        {
            Dictionary<string, string> commonMistakes = new Dictionary<string, string>
            {
                {"say(", "SAY(" },
                {"Say(", "SAY(" },
                {"Answer(", "ANSWER(" },
                {"answer(", "ANSWER(" }
            };
            
            // Check contents of luaFileInfo against the above dictionary.
            foreach (var item in MainWindowDataContext.luaFileInfo)
            {
                foreach (var dk in commonMistakes)
                {
                    // If in the dictionary and !listed, add to list and add to data context error list
                    if (item.body.Contains(dk.Key) && !errorList.Contains(item.body))
                    {
                        errorList.Add(item.body);
                        error = item.body;
                    }
                }
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
