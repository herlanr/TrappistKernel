using System;
using System.IO;
using Console = System.Console;
using TrappistOS;

namespace MIV
{
    internal class MIV
    {
        internal class CursorPos
        {
            public int row;
            public int column;
            public CursorPos(int line, int col) 
            {
                row = line;
                column = col;
            }
        }

        public static void printMIVStartScreen()
        {
            Console.Clear();
            Console.WriteLine("~");
            Console.WriteLine("~");
            Console.WriteLine("~");
            Console.WriteLine("~");
            Console.WriteLine("~");
            Console.WriteLine("~");
            Console.WriteLine("~");
            Console.WriteLine("~");
            Console.WriteLine("~");
            Console.WriteLine("~");
            Console.WriteLine("~");
            Console.WriteLine("~");
            Console.WriteLine("~");
            Console.WriteLine("~");
            Console.WriteLine("~");
            Console.WriteLine("~                     type :help<Enter>          for information");
            Console.WriteLine("~                     type :q<Enter>             to exit");
            Console.WriteLine("~                     type :wq<Enter>            to save file and exit");
            Console.WriteLine("~                     type :i<Enter>             to enter writing mode");
            Console.WriteLine("~                     press escape               to exit writing mode");
            Console.WriteLine("~                                                                    ");
            Console.WriteLine("~                     press any key to return to the text editor     ");
            Console.WriteLine("~");
            Console.WriteLine("~");
            Console.WriteLine("~");
            Console.Write("~");
        }


        private static void printMIVScreen(char[] chars, int pos, String infoBar, Boolean editMode, CursorPos cursor)
        {
            try
            {
                int countNewLine = 0;
                int countChars = 0;
                Console.Clear();

                for (int i = 0; i < chars.Length; i++)
                {
                    if (countNewLine > 22 && countNewLine >= cursor.row)
                    {
                        break;
                    }
                    if (chars[i] == '\n')
                    {
                        Console.WriteLine("");
                        countNewLine++;
                        countChars = 0;
                    }
                    else
                    {
                        Console.Write(chars[i]);
                        countChars++;
                        if ((countChars - 1) % 80 == 79)
                        {
                            countNewLine++;
                        }
                    }
                }

                for (int i = 0; i < 23 - countNewLine; i++)
                {
                    Console.WriteLine("");
                    Console.Write("~");
                }

                //PRINT INSTRUCTION
                Console.WriteLine();
                for (int i = 0; i < infoBar.Length; i++)
                {
                    Console.Write(infoBar[i]);
                }
                Console.Write($"pos: {pos} x: {cursor.column} y: {cursor.row} ");

                if (editMode)
                {
                    Console.Write(countNewLine + 1 + "," + countChars);
                }
                Console.SetCursorPosition(cursor.column, cursor.row);
            }
            catch (Exception e)
            {
                throw new SystemException($"Error while Drawing:{e.Message}");
            }
        }

        public static int NewLines(char[] text)
        {
            int countChars = 0;
            int countNewLine = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n')
                {
                    countChars = 0;
                    countNewLine++;
                }
                else
                {
                    if (countChars % 80 == 79)
                    {
                        countChars = 0;
                        countNewLine++;
                    }
                    countChars++;
                }
            }
            return countNewLine;

        }

        public static bool IsNewLine(char x)
        {
            return x == '\n';
        }

        public static String miv(String start)
        {
            Boolean editMode = false;
            int pos = 0;
            List<char> chars = new List<char>();
            String infoBar = String.Empty;
            infoBar += ":";

            CursorPos cursor = new CursorPos(0, 0);

            if (start == null)
            {
                printMIVStartScreen();
            }
            else
            {
                chars = start.ToList();
                
                if (chars.Last() != '\n')
                {
                    chars.Add('\n');
                }

                cursor.row = 24;
                cursor.column = infoBar.Length;
                printMIVScreen(chars.ToArray(), pos, infoBar, editMode, cursor);
            }
            pos = chars.Count-1;
            ConsoleKeyInfo keyInfo;

            //do
            //{
            //    keyInfo = Console.ReadKey(true);

            //    //if (isForbiddenKey(keyInfo.Key)) continue;

            //    if (!editMode)
            //    {
            //        printMIVScreen(chars, pos, infoBar, editMode);
            //        do
            //        {
            //            if (keyInfo.Key == ConsoleKey.Backspace)
            //            {
            //                if (infoBar.Length > 1)
            //                {
            //                    infoBar = stringCopy(infoBar);
            //                    printMIVScreen(chars, pos, infoBar, editMode);
            //                }
            //                // if infoBar is just ":" do nothing
            //                continue;
            //            }

            //            //keyInfo = Console.ReadKey(true);
            //            if (keyInfo.Key == ConsoleKey.Enter)
            //            {
            //                if (infoBar == ":wq")
            //                {
            //                    String returnString = String.Empty;
            //                    for (int i = 0; i < pos; i++)
            //                    {
            //                        returnString += chars[i];
            //                    }
            //                    return returnString;
            //                }
            //                else if (infoBar == ":q")
            //                {
            //                    return null;

            //                }
            //                else if (infoBar == ":help")
            //                {
            //                    printMIVStartScreen();
            //                    break;
            //                }
            //                else if (infoBar == ":i")
            //                {
            //                    editMode = true;
            //                    infoBar = "-- INSERT --";
            //                    printMIVScreen(chars, pos, infoBar, editMode);
            //                    break;
            //                }
            //                else
            //                {
            //                    infoBar = "ERROR: No such command. (ex. \":help\")";
            //                    printMIVScreen(chars, pos, infoBar, editMode);
            //                    break;
            //                }
            //            }

            //            //else if (Console.ReadKey(true).Key == ConsoleKey.Backspace)
            //            //{
            //            //    if (infoBar.Length == 1 && infoBar.StartsWith(':') == true)
            //            //    {
            //            //        continue;       
            //            //    }
            //            //    else
            //            //    {
            //            //        infoBar = stringCopy(infoBar);
            //            //        printMIVScreen(chars, pos, infoBar, editMode);
            //            //    }
            //            //}

            //            // Append allowed command characters (ignore other keys)
            //            char c = keyInfo.KeyChar;
            //            if (c == 'q' || c == ':' || c == 'w' || c == 'h' || c == 'e' || c == 'l' || c == 'p' || c == 'i')
            //            {
            //                infoBar += c;
            //                printMIVScreen(chars, pos, infoBar, editMode);
            //            }
            //            // otherwise ignore the keypress
            //            else
            //            {
            //                continue;
            //            }
            //            printMIVScreen(chars, pos, infoBar, editMode);



            //        } while (true);
            //        //while (keyInfo.Key != ConsoleKey.Escape);
            //    }

            while (true)
            {
                // Read exactly once per loop iteration
                keyInfo = Console.ReadKey(true);
                if (isForbiddenKey(keyInfo.Key)) continue;

                if (editMode)
                {
                    try
                    {
                        // Edit mode: behave like insert mode
                        switch (keyInfo.Key)
                        {
                            case ConsoleKey.Escape:
                                {
                                    editMode = false;
                                    infoBar = ":";
                                    printMIVScreen(chars.ToArray(), pos, infoBar, editMode, cursor);
                                    break;
                                }
                            case ConsoleKey.Enter:
                                {
                                    chars.Insert(pos, '\n');
                                    pos++;
                                    cursor.column = 0;
                                    cursor.row++;
                                    printMIVScreen(chars.ToArray(), pos, infoBar, editMode, cursor);
                                    break;
                                }
                            case ConsoleKey.Backspace:
                                {
                                    if (pos == 0)
                                    {
                                        Console.WriteLine("doing nothing");
                                        printMIVScreen(chars.ToArray(), pos, infoBar, editMode, cursor);
                                    }
                                    if (chars[pos - 1] == '\n')
                                    {
                                        Console.WriteLine("removing: newline");
                                        if (cursor.row != 0)
                                        {
                                            cursor.row--;
                                        }

                                        pos--;
                                        Console.WriteLine($"removing at {pos}");
                                        chars.RemoveAt(pos);
                                        if (cursor.column != 0)
                                        {
                                            cursor.column = chars.LastIndexOf('\n', pos, 0);
                                        }

                                    }
                                    else
                                    {
                                        Console.WriteLine("removing normal ");
                                        if (cursor.column != 0)
                                        {
                                            cursor.column--;
                                        }
                                        pos--;
                                        Console.WriteLine($"removing at {pos}");
                                        chars.RemoveAt(pos);

                                    }
                                    printMIVScreen(chars.ToArray(), pos, infoBar, editMode, cursor);
                                    break;
                                }
                            case ConsoleKey.RightArrow:
                                {  //wenn wir auf newline stehen und nicht am ende, an den anfange der nächsten Zeile
                                    if (pos + 1 < chars.Count)
                                    {
                                        if (chars[pos] != '\n' && (pos - chars.LastIndexOf('\n', pos - 1, 0)) %80 < 79)
                                        {
                                            cursor.column++;
                                        }
                                        else
                                        {
                                            cursor.column = 0;
                                            cursor.row++;
                                        }
                                        pos++;
                                    }
                                    printMIVScreen(chars.ToArray(), pos, infoBar, editMode, cursor);
                                    break;
                                }
                            case ConsoleKey.LeftArrow:
                                { //wenn vor uns ein newline ist, sind wir am anfang der Zeile und müssen ans ende der letzten Zeile
                                    if (pos - 1 > 0)
                                    {
                                        if (chars[pos - 1] != '\n' && cursor.column!=0 )
                                        {
                                            cursor.column--;
                                        }
                                        else
                                        {
                                            if(chars.LastIndexOf('\n', pos , 0) == pos-1)
                                            {
                                                int previous = chars.LastIndexOf('\n', pos - 1, 0);
                                                if (previous != -1)
                                                {
                                                    previous = 0;
                                                }
                                                cursor.column = pos - previous; //length of this line
                                            }
                                            else
                                            {
                                                cursor.column = 79;
                                            }
                                            cursor.row--;
                                        }
                                        pos--;
                                    }
                                    printMIVScreen(chars.ToArray(), pos, infoBar, editMode, cursor);
                                    break;
                                }

                            case ConsoleKey.UpArrow:
                                {
                                    if (pos > chars.IndexOf('\n') && chars.IndexOf('\n') != -1 || chars.Count >= 80) //check we aren't in top row
                                    {
                                        cursor.row--; //go up
                                        
                                        if (pos - chars.LastIndexOf('\n',pos-1, 0) > 79)
                                        {
                                            pos -= 79;
                                        }
                                        else
                                        {
                                            int currCol = cursor.column; //get current column
                                            int startPrevLine = chars.LastIndexOf('\n', chars.LastIndexOf('\n', pos, 0) - 1, 0) + 1; //get start of previous line, by getting th newline before the previous newline
                                            if (chars.LastIndexOf('\n', pos, 0) - startPrevLine < currCol) //check if line is actually long enough
                                            {
                                                currCol = chars.LastIndexOf('\n',pos, 0) - startPrevLine; //got to end of line if not
                                            }
                                            cursor.column = currCol;
                                            pos = startPrevLine + currCol - 1;
                                        }
                                    }
                                    printMIVScreen(chars.ToArray(), pos, infoBar, editMode, cursor);
                                    break;
                                }

                            case ConsoleKey.DownArrow:
                                {
                                    if (chars.IndexOf('\n', pos) < chars.LastIndexOf('\n') || chars.IndexOf('\n', pos) - (pos - cursor.column) > 79) //check we aren't in last line
                                    {
                                        cursor.row++;
                                        if (chars.IndexOf('\n',pos)- (pos - cursor.column) > 79)
                                        {
                                            int beginNextLine = chars.LastIndexOf('\n', pos, 0) + 79;
                                            int endNextLine = chars.IndexOf('\n', beginNextLine);
                                            if (endNextLine < cursor.column)
                                            {
                                                cursor.column = endNextLine - chars.LastIndexOf('\n', endNextLine, 0) + 1;
                                            }
                                            pos = beginNextLine + cursor.column;
                                        }
                                        else
                                        {
                                            int currCol = cursor.column;
                                            int startNextLine = chars.IndexOf('\n', pos) + 1; //get nextline beginning by character after end of this line
                                            if (chars.IndexOf('\n', chars.IndexOf('\n', pos) + 1) - startNextLine < currCol) //check next line is long enough
                                            {
                                                currCol = chars.LastIndexOf('\n', pos, 0) - startNextLine; //if not, set to beginning of this line
                                            }
                                            cursor.column = currCol;
                                            pos = startNextLine + currCol - 1;
                                        }
                                        
                                    }
                                    printMIVScreen(chars.ToArray(), pos, infoBar, editMode, cursor);
                                    break;
                                }
                            default: // Regular character insertion
                                {
                                    chars.Insert(pos, keyInfo.KeyChar);
                                    pos++;
                                    cursor.column++;
                                    if((cursor.column)%80 == 79)
                                    {
                                        cursor.column = 0;
                                        cursor.row++;
                                    }
                                    printMIVScreen(chars.ToArray(), pos, infoBar, editMode, cursor);
                                    break;
                                }
                        }

                        continue;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine();
                        Console.WriteLine(ex.Message + "\nError Occured in edit Mode\nExiting MIV...");
                        Cosmos.HAL.Global.PIT.Wait((uint)5000);
                        return null;
                    }

                }


                //        else if (keyInfo.Key == ConsoleKey.Escape)
                //    {
                //        editMode = false;
                //        infoBar = String.Empty;
                //        infoBar += ":";
                //        printMIVScreen(chars, pos, infoBar, editMode);
                //        continue;
                //    }

                //    //else if (keyInfo.Key == ConsoleKey.I && !editMode)
                //    //{
                //    //    editMode = true;
                //    //    infoBar = "-- INSERT --";
                //    //    printMIVScreen(chars, pos, infoBar, editMode);
                //    //    continue;
                //    //}

                //    else if (keyInfo.Key == ConsoleKey.Enter && editMode && pos >= 0)
                //    {
                //        chars[pos++] = '\n';
                //        printMIVScreen(chars, pos, infoBar, editMode);
                //        continue;
                //    }
                //    else if (keyInfo.Key == ConsoleKey.Backspace && editMode && pos >= 0)
                //    {
                //        if (pos > 0) pos--;

                //        chars[pos] = '\0';

                //        printMIVScreen(chars, pos, infoBar, editMode);
                //        continue;
                //    }

                //    if (editMode && pos >= 0)
                //    {
                //        chars[pos++] = keyInfo.KeyChar;
                //        printMIVScreen(chars, pos, infoBar, editMode);
                //    }

                //} while (true);

                else
                {
                    if (keyInfo.Key == ConsoleKey.Spacebar)
                    {
                        continue; // Ignore Spacebar in command mode
                    }
                    // Command mode: single key read is used for decisions
                    //if (keyInfo.Key == ConsoleKey.Escape)
                    //{
                    //    editMode = false;
                    //    infoBar = ":";
                    //    printMIVScreen(chars, pos, infoBar, editMode);
                    //    continue;
                    //}
                    cursor.row = 24;
                    cursor.column = infoBar.Length;
                    if (keyInfo.Key == ConsoleKey.Backspace)
                    {
                        if (infoBar.Length > 1)
                        {
                            infoBar = infoBar.Remove(infoBar.Length - 1);
                            printMIVScreen(chars.ToArray(), pos, infoBar, editMode,cursor);
                        }
                        // if infoBar is just ":" do nothing
                        continue;
                    }

                    if (keyInfo.Key == ConsoleKey.Enter)
                    {
                        try
                        { 
                            if (infoBar == ":wq")
                            {
                                String returnString = String.Empty;
                                for (int i = 0; i < pos; i++)
                                {
                                    returnString += chars[i];
                                }
                                return returnString;
                            }
                            else if (infoBar == ":q")
                            {
                                return null;
                            }
                            else if (infoBar == ":help")
                            {
                                printMIVStartScreen();
                                Console.ReadKey(true);
                                infoBar = ":";
                                printMIVScreen(chars.ToArray(), pos, infoBar, editMode,cursor);
                                continue;
                            }
                            else if (infoBar == ":i")
                            {
                                editMode = true;
                                infoBar = "-- INSERT --";
                                //find last char, find second to last newline if existent, put cursor on difference
                                int lastNewLine = chars.LastIndexOf('\n');
                                if (lastNewLine-1 > 0)
                                {
                                    int previousNewLine = chars.LastIndexOf('\n', lastNewLine - 1, 0);
                                    if (previousNewLine != -1)
                                    {
                                        cursor.column = chars.Count - chars.LastIndexOf('\n', lastNewLine - 1, 0)-1;
                                    }
                                    else
                                    {
                                        cursor.column = chars.Count-1;
                                    }
                                    if (cursor.column < 0)
                                    {
                                        cursor.column = 0;
                                    }
                                }
                                else
                                {
                                    cursor.column = chars.Count - 1;
                                }
                                cursor.row = chars.Count(IsNewLine) - 1;
                            

                                printMIVScreen(chars.ToArray(), pos, infoBar, editMode, cursor);
                                continue;
                            }
                            else
                            {
                                infoBar = "ERROR: No such command. Press any key to continue. (Command ex. \":help\")";
                                printMIVScreen(chars.ToArray(), pos, infoBar, editMode,cursor);
                                Console.ReadKey(true);

                                infoBar = ":";
                                printMIVScreen(chars.ToArray(), pos, infoBar, editMode, cursor);
                                continue;
                            }
                        } catch (Exception e) {Console.WriteLine(e.Message); Cosmos.HAL.Global.PIT.Wait((uint)5000); }
                    }
                    

                    // Append allowed command characters (ignore other keys)
                    char c = keyInfo.KeyChar;
                    if (c == 'q' || c == ':' || c == 'w' || c == 'h' || c == 'e' || c == 'l' || c == 'p' || c == 'i')
                    {
                        infoBar += c;
                        printMIVScreen(chars.ToArray(), pos, infoBar, editMode, cursor);
                    }
                    // otherwise ignore the keypress
                }
            }
        }

        public static bool isForbiddenKey(ConsoleKey key)
        {
            ConsoleKey[] forbiddenKeys = { ConsoleKey.Print, ConsoleKey.PrintScreen, ConsoleKey.Pause, ConsoleKey.Home, ConsoleKey.PageUp, ConsoleKey.PageDown, ConsoleKey.End, ConsoleKey.Delete, ConsoleKey.Insert, ConsoleKey.NumPad0, ConsoleKey.NumPad1, ConsoleKey.NumPad2, ConsoleKey.NumPad3, ConsoleKey.NumPad4, ConsoleKey.NumPad5, ConsoleKey.NumPad6, ConsoleKey.NumPad7, ConsoleKey.NumPad8, ConsoleKey.NumPad9, ConsoleKey.Insert, ConsoleKey.F1, ConsoleKey.F2, ConsoleKey.F3, ConsoleKey.F4, ConsoleKey.F5, ConsoleKey.F6, ConsoleKey.F7, ConsoleKey.F8, ConsoleKey.F9, ConsoleKey.F10, ConsoleKey.F11, ConsoleKey.F12, ConsoleKey.Add, ConsoleKey.Divide, ConsoleKey.Multiply, ConsoleKey.Subtract, ConsoleKey.LeftWindows, ConsoleKey.RightWindows };
            for (int i = 0; i < forbiddenKeys.Length; i++)
            {
                if (key == forbiddenKeys[i]) return true;
            }
            return false;
        }

        public static bool PrintMivCommands(string input)
        {
            //Console.WriteLine("Do you want to see the MIV Commands before opening the file? (yes/y/no/n)");
            //Console.WriteLine("Type \"exit\" to quit.");
            //string input = Console.ReadLine().ToLower().Trim();

            if (input == "yes" || input == "y")
            {
                Console.WriteLine();
                Console.WriteLine("MIV Commands:");
                Console.WriteLine(":wq - Save file and exit");
                Console.WriteLine(":q - Exit without saving");
                Console.WriteLine(":help - Show this help message");
                Console.WriteLine(":i - Enter writing mode");
                Console.WriteLine("Escape - Exit writing mode");
                Console.WriteLine();
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return true;
            }
            else if (input == "no" || input == "n")
            {
                return true;
            }
            else if(input == "exit")
            {
                Console.WriteLine("Exiting MIV...");
                Console.WriteLine();
                return false;
            }
            else
            {
                Console.WriteLine("Invalid input, please try again (yes/y/no/n) ");
                input = Console.ReadLine().ToLower().Trim();
                return PrintMivCommands(input);
            }
        }

        public static void StartMIV(string file, FileSystemManager fsManager)
        {
            try
            {
                if (!File.Exists(file))
                {
                    string input = String.Empty;
                    Console.WriteLine("File couldn't found. Do you want to create the file: " + file + "? (yes/y/no/n)");
                    input = Console.ReadLine().ToLower().Trim();

                    if (input == "yes" || input == "y")
                    {
                        if (!fsManager.createFile(file))
                        {
                            Console.WriteLine("Exiting MIV...");
                            return;
                        }

                    }
                    else if (input == "no" || input == "n")
                    {
                        Console.WriteLine("Exiting MIV...");
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Invalid input. Exiting MIV...");
                        return;
                    }
                }
                else
                {
                    Console.WriteLine("File found!");
                }

                if (File.Exists(file))
                {
                    Console.WriteLine("Do you want to see the MIV Commands before opening the file? (yes/y/no/n)");
                    Console.WriteLine("Type \"exit\" to quit.");
                    string input = Console.ReadLine().ToLower().Trim();
                    PrintMivCommands(input);
                }
                Console.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            String text = String.Empty;
            text = miv(File.ReadAllText(file));

            // Wenn Strg+C gedrückt wurde -> nichts speichern, nur Meldung
            if (Kernel.AbortRequest)
            {
                Kernel.AbortRequest = false;
                Console.Clear();
                Console.WriteLine("MIV aborted. No changes were saved to " + file);
                return;
            }

            if (text != null && File.Exists(file))
            {
                File.WriteAllText(file, text);
                Console.Clear();
                Console.WriteLine("Content has been saved to " + file);
            }
            else
            {
                Console.Clear();
                Console.WriteLine("No changes were made on " + file);
                
            }
        }
    }
}
