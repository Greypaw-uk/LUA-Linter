using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LUA_Linter
{
    public partial class MainWindow : Window
    {
        MainWindowDataContext context = new MainWindowDataContext();

        private int lineNumber;
        private int nodeNumber = 0;
        public FileInfo LuaFile;
        private string _line;

        OpenFileDialog openFile = new OpenFileDialog
        {
            Filter = "Lua files (*.lua)|*.lua"
        };

        public MainWindow()
        {
            InitializeComponent();

            DataContext = context;
        }


        private void LoadFile(object sender, RoutedEventArgs e)
        {
            Nullable<bool> result = openFile.ShowDialog();

            //if valid file is selected, open file, repeatedly add lines to observable collection  until the end of Lua file
            if (result == true)
            {
                clearDisplayList();
                clearErrorList();

                lineNumber = 0;

                tb_path.Text = openFile.FileName;

                StreamReader fileContents = new StreamReader(openFile.FileName);

                while ((_line = fileContents.ReadLine()) != null)
                {
                    lineNumber++;

                    // display contents of Lua file in listbox along with the line number
                    MainWindowDataContext.LuaFileInfo.Add(new luaFileContents
                    {
                        LineNumber = lineNumber.ToString(),
                        body = _line
                    });
                }
                fileContents.Close();

                checkForErrors();
            }
            else
            {
                MessageBox.Show("Please select a file");
            }
        }


        private void checkForErrors()
        {
            checkSyntaxSpelling();
            checkNodeNumbers();
            checkLineEnding();

            if (MainWindowDataContext.ErrorList.Count == 0)
            {
                MessageBox.Show("File does not contain any currently defined errors.");
            }
        }


        // 	#################################################################
        // 	#																#
        // 	#	CHECK DICTIONARY FOR SPELLING MISTAKES IN SYNTAX			#
        // 	#																#
        // 	#	TODO Check missionflags//									#
        // 	#	TODO Check that ANSWER nodes point to next NODE() number	#
        // 	#																#
        //	#################################################################	

        private void checkSyntaxSpelling()
        {
            var value ="";

            Dictionary<string, string> commonMistakes = new Dictionary<string, string>
            {
                {"say(", "SAY(" },
                {"Say(", "SAY(" },
                {"sya(", "SAY(" },
                {"Sya(", "SAY(" },
                {"node(", "NODE(" },
                {"Node(", "NODE(" },
                {"Answer(", "ANSWER(" },
                {"answer(", "ANSWER(" },
                {"ENDDIALOGUE(", "ENDDIALOG(" },
                {"Enddialog(", "ENDDIALOG(" },
                {"Kill(", "KILL(" },
                {"Attack(", "ATTACK(" },
                {"PLAYER_NAME ", "PLAYER_NAME() " },
                {"PLAYER_NAME,", "PLAYER_NAME()," },
                {"PLAYER_NAME.", "PLAYER_NAME()." }
            };

            lineNumber = 0;

            foreach (var line in MainWindowDataContext.LuaFileInfo)
            {
                value = line.body;
                lineNumber++;

                // if in error dictionary, add to error list
                foreach (var keyValuePair in commonMistakes)
                {
                    if (value.Contains(keyValuePair.Key))
                    {
                        var dicKey = keyValuePair.Key;
                        var dicValue = keyValuePair.Value;

                        MainWindowDataContext.ErrorList.Add(new LuaFileErrors
                        {
                            ErrorLine = lineNumber,
                            ContentError = "Should '" + dicKey + "' be '" + dicValue + "'?" + "\r"
                        });
                    }
                }
            }
        }


        // 	#################################################################
        // 	#																#
        // 	#	CHECKS WHETHER NODENUMBERS APPEAR TO BE CORRECT				#
        // 	#																#
        //	#################################################################	

        private void checkNodeNumbers()
        {
            var _nodeNumber = 0;
            var nodeCounter = 1;        // incrementing counter to compare to NODE() number
            var DialogEnded = false;
            var NodeDetected = false;

            lineNumber = 0;

            foreach (var line in MainWindowDataContext.LuaFileInfo)
            {
                lineNumber++;

                // Check if line is ENDDIALOG - set nodeNumber to equal nodeCounter
                var regexEndDialog = new Regex(@"\s*ENDDIALOG");
                var endDialogMatch = regexEndDialog.Match(line.body);

                // Check if line has the phrase NODE(x) in it - where x is a number
                var regexNode = new Regex(@"\s*NODE\((\d+)\)");
                var nodeMatch = regexNode.Match(line.body);

                if (endDialogMatch.Success) // Line has ENDDIALOG
                {
                    DialogEnded = true;
                }

                else if (nodeMatch.Success) // Line has NODE() in it
                {
                    NodeDetected = true;

                    // strip out NODE(), leaving only the number
                    bool nodesMatch = false;
                    string value = nodeMatch.Groups[1].Value;

                    _nodeNumber = Convert.ToInt32(value);

                    //check that NODE() is higher than zero to avoid ENDDIALOG errors at start of script
                    if (_nodeNumber > 0)
                    {
                        if (DialogEnded)
                        {
                            nodeCounter = _nodeNumber;
                        }

                        //compare NODE number to incrementing counter to check if they are the same.
                        if (_nodeNumber == nodeCounter)
                        {
                            nodesMatch = true;
                        }

                        if (!nodesMatch)
                        {
                            MainWindowDataContext.ErrorList.Add(new LuaFileErrors
                            {
                                ErrorLine = lineNumber,
                                ContentError = "NODE(" + _nodeNumber + ")" + " should be NODE(" + nodeCounter + ")"
                            });
                        }
                        DialogEnded = false;
                    }
                    else
                    {
                        _nodeNumber = nodeCounter;
                    }
                }

                if (NodeDetected)
                {
                    nodeCounter++;

                    NodeDetected = false;
                }
            }
        }


        // 	#################################################################
        // 	#																#
        // 	#	CHECKS LINES WITH SAY ARE FOLLOWED BY AN ANSWER, ATTACK,	#
        //	#	KILL, ENDDIALOG, SETNEXTDIALOGESTATE, OR REMOVE COMMAND		#
        // 	#																#
        //	#################################################################	

        private void checkLineEnding()
        {
            var _nodeNumber = 0;
            var SayTriggered = false;
            var _triggerLine = 0;
            var SayDetectLine = 0;

            foreach (var line in MainWindowDataContext.LuaFileInfo)
            {
                var SayDetected = new Regex(@"\s*(SAY\()+").Match(line.body).Success;
                var AnswerDetected = new Regex(@"\s*(ANSWER\()+").Match(line.body).Success;
                var AttackDetected = new Regex(@"\s*(ATTACK\(\))+").Match(line.body).Success;
                var KillDetected = new Regex(@"\s*(KILL\(\))+").Match(line.body).Success;
                var EndDialogDetected = new Regex(@"\s*(ENDDIALOG\(\))+").Match(line.body).Success;
                var DialogStateDetected = new Regex(@"\s*(SETNEXTDIALOGSTATE\(\))+").Match(line.body).Success;
                var RemoveDetected = new Regex(@"\s*(REMOVE\(\))+").Match(line.body).Success;

                SayDetectLine++;

                if (_nodeNumber > 0)
                {
                    if (SayTriggered && SayDetected)
                    {
                        MainWindowDataContext.ErrorList.Add(new LuaFileErrors
                        {
                            ErrorLine = (_triggerLine + 1),
                            ContentError = "SAY not terminated correctly."
                        });
                    }

                    if (SayDetected)
                    {
                        SayTriggered = true;
                        _triggerLine = SayDetectLine;
                    }

                    if (AnswerDetected | AttackDetected | KillDetected | EndDialogDetected | DialogStateDetected | RemoveDetected)
                    {
                        SayTriggered = false;
                    }
                }
            }
                
        }


        // 	#################################################################
        // 	#																#
        // 	#	RECHECKS ALTERATIONS MADE WITHIN LINTER WITHOUT NEED TO   	#
        //	#	SAVE AND RELOAD THE FILE                                	#
        // 	#																#
        //	#################################################################

        private void ScanFile(object sender, RoutedEventArgs e)
        {
            clearErrorList();
            checkForErrors();
        }


        private void clearDisplayList()
        {
            ObservableCollection<luaFileContents> itemsToRemove = new ObservableCollection<luaFileContents>();

            foreach (luaFileContents item in lv_display.Items)
            {
                itemsToRemove.Add(item);
            }

            foreach (luaFileContents item in itemsToRemove)
            {
                ((ObservableCollection<luaFileContents>)lv_display.ItemsSource).Remove(item);
            }
        }


        private void clearErrorList()
        {
            ObservableCollection<LuaFileErrors> errorsToRemove = new ObservableCollection<LuaFileErrors>();

            foreach (LuaFileErrors item in lb_errorDisplay.Items)
            {
                errorsToRemove.Add(item);
            }

            foreach (LuaFileErrors item in errorsToRemove)
            {
                ((ObservableCollection<LuaFileErrors>)lb_errorDisplay.ItemsSource).Remove(item);
            }
        }


        // 	#################################################################
        // 	#																#
        // 	#	DELETES CURRENT SAVEFILE AND CREATES A NEW SAVE FILE    	#
        // 	#																#
        //	#################################################################

        private void SaveFile(object sender, RoutedEventArgs e)
        {
            var SaveFileLocation = new SaveFileDialog
            {
                Filter = "Lua files (*.lua)|*.lua"
            };

            SaveFileLocation.ShowDialog();

            if (SaveFileLocation.FileName != "")
            {
                var SaveWriter = new StreamWriter(SaveFileLocation.FileName);

                foreach (var item in MainWindowDataContext.LuaFileInfo)
                {
                    SaveWriter.Write(item.body + "\r");
                }

                SaveWriter.Close();

                MessageBox.Show("File saved.");
            }
        }


        // 	#################################################################
        // 	#																#
        // 	#	SCROLLS TO THE LINE IN DISPLAY BOX WHEN DOUBLECLICKING  	#
        //	#	AN ENTRY IN THE ERROR BOX                           		#
        // 	#																#
        //	#################################################################

        private void Errorbox_click(object sender, MouseEventArgs e)
        {
            var control = (TextBox)sender;
            var errorInfo = (LuaFileErrors)control.DataContext;
            var selectedItem = errorInfo.ErrorLine;

            lv_display.SelectedIndex = (selectedItem - 1);
            lv_display.ScrollIntoView(lv_display.SelectedItem);
        }
    }
}