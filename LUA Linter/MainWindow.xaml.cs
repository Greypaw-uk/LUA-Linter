using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
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

            //if valid file is selected, open file, repeatedly add lines to observable collection  until the end of Lua file
            if (result == true)
            {
                ClearDisplayList();
                ClearErrorList();
                lineNumber = 0;

                StreamReader fileContents = new StreamReader(openFile.FileName);
                
                while ((_line = fileContents.ReadLine()) != null)
                {
                    lineNumber++;

                    // display contents of Lua file in listbox along with the line number
                    MainWindowDataContext.LuaFileInfo.Add(new luaFileContents
                    {
                        body = lineNumber + "   " + _line
                    });
                }

                CheckForErrors();
            }
            else
            {
                MessageBox.Show("Please select a file");
            }
        }


        private void CheckForErrors()
        {
// ----- CHECK DICTIONARY FOR SPELLING MISTAKES -----

            Dictionary<string, string> commonMistakes = new Dictionary<string, string>
            {
                {"say(", "SAY(" },
                {"Say(", "SAY(" },
                {"sya(", "SAY(" },
                {"Sya(", "SAY(" },
                {"node(", "NODE(" },
                {"Node(", "NODE(" },
                {"Answer(", "ANSWER(" },
                {"answer(", "ANSWER(" }
            };

            foreach (var line in MainWindowDataContext.LuaFileInfo)
            {
                var value = line.body;

                // if in error dictionary, add to error list
                foreach (var keyValuePair in commonMistakes)
                {
                    if (value.Contains(keyValuePair.Key))
                    {
                        var dicKey = keyValuePair.Key;
                        var dicValue = keyValuePair.Value;

                        MainWindowDataContext.ErrorList.Add(new LuaFileErrors
                        {
                            ContentError = value + "\r" + "Should '" + dicKey + "' be '" + dicValue + "'?" + "\r"
                        });
                    }
                }
            }

// ----- CHECK NODE ORDER -----

            int nodeCounter = 0;
            int nodeNumber =0;

            foreach (var line in MainWindowDataContext.LuaFileInfo)
                {
                // Check if line has the phrase NODE(x) in it - where x is a number
                Regex regex = new Regex(@"\s*NODE\((\d+)\)");
                Match match = regex.Match(line.body);

                if(match.Success)
                {
                    // strip out NODE(), leaving only the number
                    bool nodesMatch = false;
                    string value = match.Groups[1].Value;
                    
                    nodeNumber = Convert.ToInt32(value);

                    //compare NODE number to incrementing counter to check if they are the same.
                    if (nodeCounter == nodeNumber)
                    {
                        nodesMatch = true;
                    }

//TODO Add function to check for ENDDIALOG(), KILL(), ATTACK() and round up node numbers to nearest 10.

                    if (!nodesMatch)
                    {
                        //Console.WriteLine("i and j do not match");

                        MainWindowDataContext.ErrorList.Add(new LuaFileErrors
                        {
                            ContentError = "Potential NODE numbering error at line " + line.body
                        });
                    }

                    nodeCounter++;
                }
            }

// ----- DISPLAY ALL CLEAR MESSAGE IF NO ERRORS IDENTIFIED -----

            if (MainWindowDataContext.ErrorList.Count == 0)
            {
                MessageBox.Show("File does not contain any currently defined errors.");
            }
        }


        private void ScanFile(object sender, RoutedEventArgs e)
        {
            ClearErrorList();
            CheckForErrors();
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


        private void ClearErrorList()
        {
            ObservableCollection<LuaFileErrors> errorsToRemove = new ObservableCollection<LuaFileErrors>();

            foreach (LuaFileErrors item in lb_errorDisplay.Items)
            {
                //Console.WriteLine("Removing" + item.ContentError + "from error list...");
                errorsToRemove.Add(item);
            }

            foreach (LuaFileErrors item in errorsToRemove)
            {
                //Console.WriteLine("Removed " + item.ContentError + "from error list...");
                ((ObservableCollection<LuaFileErrors>)lb_errorDisplay.ItemsSource).Remove(item);
            }
        }
    }
}
