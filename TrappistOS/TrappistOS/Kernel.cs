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

            Console.Write(fsManager.getCurrentDir() + "> ");
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
                            Console.WriteLine("Use: createfile <file_name>");
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
                            Console.WriteLine("Use: createdirectory <file_name>");
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
