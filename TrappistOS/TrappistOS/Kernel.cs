using Cosmos.System.Graphics;
using Cosmos.System.ScanMaps;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Timers;
using System.IO;
using Sys = Cosmos.System;

namespace TrappistOS
{
    public class Kernel : Sys.Kernel
    {
        // each program needs one space here
        ProgramClass[] ProgramMemory = new ProgramClass[6];
        FileSystemManager fsManager;

        UserLogin userInfo;
        ProgramClass[] ProgramMemory = new ProgramClass[6];
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
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write(fsManager.getCurrentDir() + "> ");
            Console.ForegroundColor = ConsoleColor.White;

            Console.Write($"{UserLogin.currentUser.username}:Input: ");
            Console.Write($"{userInfo.currentUser.username}:Input: ");
            Console.Write($"{userInfo.get_name}:Input: ");
            Console.WriteLine(File.ReadAllText("users.json"));
            //Console.Write($"{userInfo.get_name}:Input: ");
            Console.Write($"{userInfo.get_name}:Input: ");
            var input = Console.ReadLine();

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
                default:
                    {
                        Console.WriteLine("Not a valid command");
                        break;
                    }

            }
            userInfo.Run();
            if (input == "shutdown")
            {
                Sys.Power.Shutdown();
            }
            
            Console.Write("Text typed: ");
            Console.WriteLine(input);
        }
    }


}
