using Cosmos.System.Graphics;
using Cosmos.System.ScanMaps;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Sys = Cosmos.System;

namespace TrappistOS
{
    public class Kernel : Sys.Kernel
    {
        // each program needs one space here
        ProgramClass[] ProgramMemory = new ProgramClass[6];
        FileSystemManager fsManager;

        protected override void BeforeRun()
        {
            fsManager = new FileSystemManager();
            fsManager.fsInitialize();
            Sys.KeyboardManager.SetKeyLayout(new DE_Standard());
            Console.WriteLine("TrappistOS booted up!");
        }

        protected override void Run()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write(fsManager.getCurrentDir() + "> ");
            Console.ForegroundColor = ConsoleColor.White;

            var input = Console.ReadLine();

            string[] args = input.Split(' ');

            switch (args[0])
            {
                case "freespace":
                    {
                        fsManager.showFreeSpace();
                        break;
                    }
                case "touch":
                    {
                        if (args.Length > 1)
                        {
                            fsManager.createFile(args[1]);
                        }
                        else
                        {
                            Console.WriteLine("Use: touch <file_name>");
                        }
                        break;
                    }
                case "mkdir":
                    {
                        if (args.Length > 1)
                        {
                            fsManager.createDirectory(args[1]);
                        }
                        else
                        {
                            Console.WriteLine("Use: mkdir <dir_name>");
                        }
                        break;
                    }

                case "ls":
                    {
                        fsManager.listFiles();
                        break;
                    }

                case "mv":
                    {
                        fsManager.moveFile(args[1], args[2]);
                        break;
                    }

                case "cat":
                    {
                        fsManager.readFromFile(args[1]);
                        break;
                    }

                case "rm":
                    {
                        fsManager.deleteFileOrDir(args[1]);
                        break;
                    }
                case "cd":
                    {
                        fsManager.changeDirectory(args[1]);
                        break;
                    }
                case "clear":
                    {
                        Console.Clear();
                        break;
                    }
                default:
                    {
                        Console.WriteLine("Not a valid command");
                        break;
                    }

            }
        }
    }


}
