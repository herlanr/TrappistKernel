using Cosmos.System.Graphics;
using Cosmos.System.ScanMaps;
using Cosmos.System.ScanMaps;
using MIV;
using System;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Timers;
using Sys = Cosmos.System;

namespace TrappistOS
{
    public class Kernel : Sys.Kernel
    {
        // each program needs one space here
        ProgramClass[] ProgramMemory = new ProgramClass[6];
        FileSystemManager fsManager;

        UserLogin userInfo;
        protected override void BeforeRun()
        {
            fsManager = new FileSystemManager();
            fsManager.fsInitialize();
            Sys.KeyboardManager.SetKeyLayout(new DE_Standard());
            Console.Clear();
            Console.WriteLine("TrappistOS booted up!");
            userInfo = new UserLogin();
            ProgramMemory[0] = userInfo;
            userInfo.BeforeRun();
            Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back.");
            
        }

        protected override void Run()
        {

            var cmd = new CommandHistory();
            var input = cmd.ReadLine(userInfo.get_name(), fsManager.getCurrentDir());

            string[] args = input.Split(' ');

            switch (args[0])
            {
                case "freespace":
                    {
                        if(args.Length > 1)
                        {
                            if (args[1] == "-h")
                            {
                                Console.WriteLine("Usage: freespace");
                                Console.WriteLine("Description: Get available free space.");
                                Console.WriteLine("Avaiable Arguments: \n-h: help");
                                break;
                            }
                        }

                        fsManager.showFreeSpace();
                        break;
                    }
                case "touch":
                    {
                        if (args.Length < 2 || args[1] == "-h")
                        {
                            Console.WriteLine("Usage: touch <file name> ");
                            Console.WriteLine("Description: Creates a new file");
                            Console.WriteLine("Avaiable Arguments: \n-h: help");
                            break;
                        }
                        
                        fsManager.createFile(args[1]);
                        break;
                    }
                case "mkdir":
                    {
                        if (args.Length < 2 || args[1] == "-h")
                        {
                            Console.WriteLine("Usage: mkdir <directory name> ");
                            Console.WriteLine("Description: Creates a new directory");
                            Console.WriteLine("Avaiable Arguments: \n-h: help");
                            break;
                        }
                        fsManager.createDirectory(args[1]);
                        break;
                    }

                case "ls":
                    {
                        if (args.Length > 1)
                        {
                            if (args[1] == "-h")
                            {
                                Console.WriteLine("Usage: ls");
                                Console.WriteLine("Description: List all the files in a directory");
                                Console.WriteLine("Avaiable Arguments: \n-h: help");
                                break;
                            }
                        }
                        fsManager.listFiles();
                        break;
                    }

                case "mv":
                    {
                        if (args.Length < 3 || args[1] == "-h")
                        {
                                Console.WriteLine("Usage: mv <file path> <dest path>");
                                Console.WriteLine("Description: List all the files in a directory");
                                Console.WriteLine("Avaiable Arguments: \n-h: help");
                                break;

                        }
                        fsManager.moveFile(args[1], args[2]);
                        break;
                    }

                case "cat":
                    {
                        if (args.Length < 2 || args[1] == "-h")
                        {
                            Console.WriteLine("Usage: cat <file name>");
                            Console.WriteLine("Description: Opens a text file, reads all the text in the file, and then closes the file.");
                            Console.WriteLine("Avaiable Arguments: \n-h: help");
                            break;
                        }

                        fsManager.readFromFile(args[1]);
                        break;
                    }

                case "rm":
                    {
                        if (args.Length < 2 || args[1] == "-h")
                        {
                            Console.WriteLine("Usage: rm <file name OR directory name>");
                            Console.WriteLine("Description: Deletes the specified file or dir");
                            Console.WriteLine("Avaiable Arguments: \n-h: help");
                            break;
                        }

                        fsManager.deleteFileOrDir(args[1]);
                        break;
                    }
                case "cd":
                    {
                        if (args.Length < 2 || args[1] == "-h")
                        {
                            Console.WriteLine("Usage: cd <directory path>");
                            Console.WriteLine("Description: Changes your current directory to the specified one.");
                            Console.WriteLine("Avaiable Arguments: \n-h: help");
                            break;
                        }

                        fsManager.changeDirectory(args[1]);
                        break;
                    }

                case "pwd":
                    {
                        Console.WriteLine("You are here: " + fsManager.getCurrentDir());
                        break;
                    }
                case "clear":
                    {
                        Console.Clear();
                        break;
                    }
                case "shutdown":
                    {
                        Sys.Power.Shutdown();
                        break;
                    }
                case "reboot":
                    {
                        Sys.Power.Reboot();
                        break;
                    }
                case "login":
                    {        
                        if (args.Length == 1)
                        {
                            userInfo.Login();
                        }
                        else
                        {
                            Console.WriteLine("Usage: Login");
                            Console.WriteLine("Description: Login to Account");
                            Console.WriteLine("Available Arguments:\n -h: help");
                        }
                        break;
                    }
                case "logout":
                    {
                        if (args.Length == 1)
                        {
                            userInfo.Logout();
                        }
                        else
                        {
                            Console.WriteLine("Usage: Logout");
                            Console.WriteLine("Description: Logout of your Account");
                            Console.WriteLine("Available Arguments:\n -h: help");
                        }
                        break;
                    }
                case "deleteuser":
                case "delusr":
                    {
                        if (args.Length == 2 && args[1] != "-h")
                        {
                            if (userInfo.IsAdmin())
                            {
                                if (args[1] == "Visitor" || args[1] == "Admin")
                                {
                                    Console.WriteLine($"Cannot delete {args[1]}");
                                    break;
                                }
                                if (args[1] == userInfo.get_name())
                                {
                                    Console.WriteLine("Cannot delete User you are logged in with");
                                }
                                userInfo.DeleteUser(args[1]);
                                
                            } else
                            {
                                Console.WriteLine("You need to be an admin to do this");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Usage: deleteUser [username] / delUsr [username]");
                            Console.WriteLine("Description: delete a User (only available to admins)");
                            Console.WriteLine("Available Arguments: \n -h: help");

                        }
                        break;
                    }
                case "createuser":
                case "mkusr":
                    {
                        if (args.Length == 1)
                        {  
                            userInfo.CreateUser(false); 
                        }
                        else if (args.Length >= 2)
                        {
                            if (args[1] == "-a")
                                if (userInfo.IsAdmin())
                                {
                                    userInfo.CreateUser(true);
                                }
                                else
                                {
                                    Console.WriteLine("Only Admins can create Admins");
                                }
                            else
                            {
                                Console.WriteLine("Usage: createUser / mkUsr");
                                Console.WriteLine("Description: Create a new User");
                                Console.WriteLine("Available Arguments: \n -h: help \n -a: create Admin (Only Admins can crate Admins)");
                            }
                        }
                        break;
                    }/*
                case "increaseAdminRange":
                case "incAdmRange":
                    {
                        if (args.Length == 1 || args[1] == "-h")
                        {
                            Console.WriteLine("Usage: increaseAdminRange [count] / incAdmRange [count]");
                            Console.WriteLine("Descrition: increase the Amount off possible Admins by this amount (reducing is not possible");
                            Console.WriteLine("Available Arguments: \n -h help");
                        }
                        break;
                    }*/
                case "changepwd":
                    {
                        if (!(userInfo.get_name() == "Visitor"))
                        { 
                            userInfo.Changepassword(); 
                        }
                        else
                        {
                            Console.WriteLine("Can't change Password of Visitor");
                        }
                        break;
                    }
                case "":
                    { break; }
                case "miv":
                    {
                        if (args.Length == 2)
                        {
                            string filePath = Path.Combine(fsManager.getCurrentDir(), args[1]);
                            if (MIV.MIV.PrintMivCommands() == true)
                            {
                                MIV.MIV.StartMIV(filePath);
                            }
                            else break;
                        }
                        else {
                            Console.WriteLine("miv + path");
                        } 
                        break;
                    }
                default:
                    {
                        Console.WriteLine("Not a valid command");
                        break;
                    }

            }
            
        }
        protected static void myHandler(object sender, ConsoleCancelEventArgs args)
        {
            Console.WriteLine("\nThe read operation has been interrupted.");

            Console.WriteLine($"  Key pressed: {args.SpecialKey}");

            Console.WriteLine($"  Cancel property: {args.Cancel}");

            // Set the Cancel property to true to prevent the process from terminating.
            Console.WriteLine("Setting the Cancel property to true...");
            args.Cancel = true;

            // Announce the new value of the Cancel property.
            Console.WriteLine($"  Cancel property: {args.Cancel}");
            Console.WriteLine("The read operation will resume...\n");
        }

    }


}
