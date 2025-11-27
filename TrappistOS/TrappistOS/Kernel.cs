using Cosmos.System.ScanMaps;
using System;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using Sys = Cosmos.System;

//wait function: Cosmos.HAL.Global.PIT.Wait((uint)10000);

namespace TrappistOS
{
    public class Kernel : Sys.Kernel
    {
        string userpath = @"0:\users.sys";
        string rootdir = @"0:\";

        FileSystemManager fsManager;

        UserLogin userInfo;
        FilePermissions permManager;
        protected override void BeforeRun()
        {
            Console.WriteLine("TrappistOS booting");
            fsManager = new FileSystemManager();
            fsManager.fsInitialize();
            Sys.KeyboardManager.SetKeyLayout(new DE_Standard());
            Console.WriteLine("Filesystem initialized");
            Console.Clear();
            // Print system info
            Console.WriteLine(" _____                     _     _   _____ _____ \r\n|_   _|                   (_)   | | |  _  /  ___|\r\n  | |_ __ __ _ _ __  _ __  _ ___| |_| | | \\ `--. \r\n  | | '__/ _` | '_ \\| '_ \\| / __| __| | | |`--. \\\r\n  | | | | (_| | |_) | |_) | \\__ \\ |_\\ \\_/ /\\__/ /\r\n  \\_/_|  \\__,_| .__/| .__/|_|___/\\__|\\___/\\____/ \r\n              | |   | |                          \r\n              |_|   |_|                          ");

            Console.WriteLine("Version: 1.0");
            Console.WriteLine("Kernel: Cosmos");
            Console.WriteLine("Current Directory: " + fsManager.getCurrentDir());
            Console.WriteLine("Memory: " + Cosmos.Core.CPU.GetAmountOfRAM() + " MB");
            Console.WriteLine("Autors: Erik Wenk");
            Console.WriteLine("        Herlan Rodrigo Ribeiro Rocha");
            Console.WriteLine("        Nurettin Fatih Semiz");  
            Console.WriteLine("        Timo Andreas Paetsch");  
            Console.WriteLine("=====================================");
            Console.WriteLine();

            Console.WriteLine("Please press ENTER to go to the terminal...");
            Console.WriteLine("Or press ESC to exit...");

            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine("\nStarting terminal...");
                    userInfo = new UserLogin();
                    userInfo.BeforeRun(userpath);
                    Console.WriteLine("Usermanagement intialized");

                    permManager = new FilePermissions();
                    permManager.PermInit(userInfo, new[] { userpath });
                    Console.WriteLine("Filepermissions initialized");
                    Console.Clear();
                    Console.WriteLine("TrappistOS booted up!");
                    break;
                }
                else if (keyInfo.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine("\nExiting system...");
                    Sys.Power.Shutdown();
                    return;
                }
                else
                {
                    Console.WriteLine("\nInvalid key. Press ENTER or ESC.");
                }
            }



        }

        protected override void Run()
        {

            var cmd = new CommandHistory();
            var input = cmd.ReadLine(userInfo.GetName(), fsManager.getCurrentDir());
            

            string[] args = input.Split(' ');
            args[0] = args[0].ToLower();
            switch (args[0])
            {
                case "freespace": //in help
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
                case "touch": //in help
                    {
                        if (args.Length < 2 || args[1] == "-h")
                        {
                            Console.WriteLine("Usage: touch [-o] <file name> ");
                            Console.WriteLine("Description: Creates a new file");
                            Console.WriteLine("Avaiable Arguments: \n-h: help \n-o: give yourself complete ownership.\n    If not set, it will inherent the ownership of the Current directory.");
                            break;
                        }
                        if(!permManager.IsWriter(fsManager.getCurrentDir(),userInfo.GetId()))
                        {
                            Console.WriteLine("You do not have permission to create files here.");
                            break;
                        }
                        if (args.Length > 2 && args[2] == "-o")
                        {
                            string path = fsManager.createFile(args[1]);
                            permManager.InitPermissions(path, userInfo.GetId());
                        }
                        else
                        {
                            string path = fsManager.createFile(args[1]);
                            permManager.InitPermissions(path);
                        }
                        break;
                    }
                case "mkdir": //in help
                    {
                        if (args.Length < 2 || args[1] == "-h")
                        {
                            Console.WriteLine("Usage: mkdir [-o] <directory name> ");
                            Console.WriteLine("Description: Creates a new directory");
                            Console.WriteLine("Avaiable Arguments: \n-h: help \n-o: give yourself complete ownership.\n    If not set, it will inherent the ownership of the Current directory.");
                            break;
                        }

                        if (!permManager.IsWriter(fsManager.getCurrentDir(), userInfo.GetId()))
                        {
                            Console.WriteLine("You do not have permission to create directories here.");
                            break;
                        }
                        if (args.Length > 2 && args[2] == "-o")
                        {
                            string path = fsManager.createDirectory(args[1]);
                            permManager.InitPermissions(path, userInfo.GetId());
                        }
                        else
                        {
                            string path = fsManager.createDirectory(args[1]);
                            permManager.InitPermissions(path);
                        }
                            
                        break;
                    }

                case "ls": //in help
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

                case "mv": //in help
                    {
                        if (args.Length < 3 || args[1] == "-h")
                        {
                                Console.WriteLine("Usage: mv <file path> <dest path>");
                                Console.WriteLine("Description: Move file between directories");
                                Console.WriteLine("Avaiable Arguments: \n-h: help");
                                break;

                        }
                        if (!File.Exists(fsManager.getFullPath(args[1])) && !Directory.Exists(fsManager.getFullPath(args[1])))
                        {
                            Console.WriteLine("File does not exist");
                            break;
                        }
                        if (!permManager.IsWriter(fsManager.getFullPath(args[1]), userInfo.GetId()) && !userInfo.IsAdmin())
                        {
                            Console.WriteLine("You do not have permissions to move this file.");
                            break;
                        }
                        string oldpath = fsManager.getFullPath(args[1]);
                        string newpath = fsManager.moveFile(args[1], args[2]);
                        if (newpath != null)
                        {
                            permManager.switchPermissionPath(oldpath, newpath);
                        }
                        break;
                    }

                case "cat": //in help
                    {
                        if (args.Length < 2 || args[1] == "-h")
                        {
                            Console.WriteLine("Usage: cat <file name>");
                            Console.WriteLine("Description: Opens a text file, reads all the text in the file,\nand then closes the file.");
                            Console.WriteLine("Avaiable Arguments: \n-h: help");
                            break;
                        }
                        if (!File.Exists(fsManager.getFullPath(args[1])) && !Directory.Exists(fsManager.getFullPath(args[1])))
                        {
                            Console.WriteLine("File does not exist");
                            break;
                        }
                        string path = fsManager.getFullPath(args[1]);
                        if (path == null)
                        {
                            Console.WriteLine("This file does not exist.");                        
                        }
                        if (permManager.IsReader(fsManager.getFullPath(args[1]), userInfo.GetId()) || userInfo.IsAdmin())
                        {
                            fsManager.readFromFile(args[1]);
                        } else
                        {
                            Console.WriteLine("You do not have permission to view this file.");
                        }
                        break;
                    }
                case "rmf":
                case "rmfile":
                    {
                        if (args.Length < 2 || args[1] == "-h")
                        {
                            Console.WriteLine("Usage: rmf / rmfile <file name>");
                            Console.WriteLine("Description: Deletes the specified file");
                            Console.WriteLine("Avaiable Arguments: \n-h: help");
                            break;
                        }
                        if (!File.Exists(fsManager.getFullPath(args[1])) && !Directory.Exists(fsManager.getFullPath(args[1])))
                        {
                            Console.WriteLine("File does not exist");
                            break;
                        }


                        if (!permManager.IsWriter(fsManager.getFullPath(args[1]), userInfo.GetId()) && !userInfo.IsAdmin())
                        {
                            Console.WriteLine("You do not have permissions to delete this file.");
                            break;
                        }
                        permManager.deletePath(fsManager.getFullPath(args[1]));
                        fsManager.deleteFile(args[1]);
                        permManager.SavePermissions();
                        break;
                    }
                case "rmd":
                case "rmdir":
                    {
                        if (args.Length < 2 || args[1] == "-h")
                        {
                            Console.WriteLine("Usage: rmd / rmdir <directory name>");
                            Console.WriteLine("Description: Deletes the specified dir");
                            Console.WriteLine("Avaiable Arguments: \n-h: help");
                            break;
                        }
                        if (!File.Exists(fsManager.getFullPath(args[1])) && !Directory.Exists(fsManager.getFullPath(args[1])))
                        {
                            Console.WriteLine("File does not exist");
                            break;
                        }

                        if (!permManager.IsWriter(fsManager.getFullPath(args[1]), userInfo.GetId()) && !userInfo.IsAdmin())
                        {
                            Console.WriteLine("You do not have permissions to delete this directory.");
                            break;
                        }
                        permManager.deletePath(fsManager.getFullPath(args[1]));
                        fsManager.deleteDir(args[1]);
                        permManager.SavePermissions();
                        break;
                    }

                case "rename":
                    {
                        if (args.Length < 3 || args[1] == "-h")
                        {
                            Console.WriteLine("Usage: rename <directory or file> <new name>");
                            Console.WriteLine("Description: It Renames the selected directory or file.");
                            Console.WriteLine("Avaiable Arguments: \n-h: help");
                            break;
                        }
                        if (!File.Exists(fsManager.getFullPath(args[1])) && !Directory.Exists(fsManager.getFullPath(args[1])))
                        {
                            Console.WriteLine("File does not exist");
                            break;
                        }
                        if (!permManager.IsWriter(fsManager.getFullPath(args[1]),userInfo.GetId()) && !userInfo.IsAdmin())
                        {
                            Console.WriteLine("You do not have permissions to rename this file.");
                            break;
                        }
                        string oldpath = fsManager.getFullPath(args[1]);
                        string newpath = fsManager.renameFileOrDir(args[1], args[2]);
                        if ( newpath != null)
                        {
                            permManager.switchPermissionPath(oldpath, newpath);
                        }
                        break;
                    }

                case "cd": //in help
                    {
                        if (args.Length < 2 || args[1] == "-h")
                        {
                            Console.WriteLine("Usage: cd <directory path>");
                            Console.WriteLine("Description: Changes your current directory to the specified one.");
                            Console.WriteLine("Avaiable Arguments: \n-h: help");
                            break;
                        }
                        string path = fsManager.getFullPath(args[1]);
                        if (!File.Exists(path) && !Directory.Exists(path))
                        {
                            if (path != null)
                            {
                                Console.WriteLine($"File/Directory {path} does not exist");
                            }
                            else
                            {
                                Console.WriteLine("File/Directory does not exist");
                            }
                            break;
                        }


                        if (permManager.IsReader(fsManager.getFullPath(args[1]), userInfo.GetId()) || userInfo.IsAdmin())
                        {
                            fsManager.changeDirectory(args[1]);
                        }
                        else
                        {
                            Console.WriteLine("You do not have permission to view this directory.");
                        }
                        break;
                    }
                case "setowner":
                    {
                        if (args.Length < 3 || args[1] == "-h")
                        {
                            Console.WriteLine("Usage: setOwner <path> <new owner>");
                            Console.WriteLine("Description: Changes the owner of the specified file/directory to the specified account. \nCan only be done by the current file owner.");
                            Console.WriteLine("Avaiable Arguments: \n-h: help");
                            break;
                        }
                        if (args[2] == "system")
                        {
                            Console.WriteLine("You cannot take away ownership from the system.");
                            break;
                        }
                        if (userInfo.GetId(args[2]) == 0)
                        {
                            Console.WriteLine("This User does not exist. Please check if you respected the case.");
                            break;
                        }
                        if (!File.Exists(fsManager.getFullPath(args[1])) && !Directory.Exists(fsManager.getFullPath(args[1])))
                        {
                            Console.WriteLine("File does not exist");
                            break;
                        }
                        if (!permManager.IsOwner(fsManager.getFullPath(args[1]), userInfo.GetId()) && !userInfo.IsAdmin() && !userInfo.IsVisitor()
                            || permManager.IsOwner(fsManager.getFullPath(args[1]), 0))
                        {
                            Console.WriteLine("You do not have the right to change ownership.");
                            break;
                        }
                        Console.WriteLine($"Do you want to give Ownership of {args[1]} to {args[2]}?\nThis action is not reversable.\n(y)es/(n)o");
                        
                        if (WaitForConfirmation())
                        {
                            permManager.SetOwner(fsManager.getFullPath(args[1]), userInfo.GetId(args[2]));
                            Console.WriteLine("Successfully changed Owner to" + args[2]);
                        }
                        else
                        {
                            Console.WriteLine("Change aborted.");
                        }
                        break;
                    }
                case "givepermissions":
                case "gperm":
                    {
                        if (args.Length < 4 || args.Contains("-h"))
                        {
                            Console.WriteLine("Usage: gperm / givepermissions <path> <user owner> [-r] [-w]");
                            Console.WriteLine("Description: Changes the owner of the specified file/directory to the specified account. \nCan only be done by the current file owner.");
                            Console.WriteLine("Avaiable Arguments: \n-h: help");
                            break;
                        }
                        if (args[2] == "system")
                        {
                            Console.WriteLine("You cannot give permissions to the system.");
                            break;
                        }
                        if (userInfo.GetId(args[2]) == 0)
                        {
                            Console.WriteLine("This User does not exist. Please check if you respected the case.");
                            break;
                        }
                        if (!File.Exists(fsManager.getFullPath(args[1])) && !Directory.Exists(fsManager.getFullPath(args[1])))
                        {
                            Console.WriteLine("File does not exist");
                            break;
                        }
                        if ((!permManager.IsOwner(fsManager.getFullPath(args[1]), userInfo.GetId()) && !userInfo.IsAdmin() && !userInfo.IsVisitor()) 
                            || permManager.IsOwner(fsManager.getFullPath(args[1]),0))
                        {
                            Console.WriteLine("You do not have the right to change the Permissions of this file.");
                            break;
                        }
                        if (args.Contains("-r")||args.Contains("-w"))
                        {
                            if(permManager.SetReader(fsManager.getFullPath(args[1]),userInfo.GetId(args[2])))
                            {
                                Console.WriteLine("Successfully added reading rights");
                            }
                            else
                            {
                                Console.WriteLine("Error adding reading rights");
                            }
                        }

                        if (args.Contains("-w"))
                        {
                            if(permManager.SetWriter(fsManager.getFullPath(args[1]), userInfo.GetId(args[2])))
                            {
                                Console.WriteLine("Successfully added writing rights");
                            }
                            else
                            {
                                Console.WriteLine("Error adding writing rights");
                            }
                        }
                        break;
                    }
                case "takepermissions":
                case "tperm":
                    {
                        if (args.Length < 4 || args.Contains("-h"))
                        {
                            Console.WriteLine("Usage: tperm / takepermissions <path> <user owner> [-r] [-w]");
                            Console.WriteLine("Description: Changes the owner of the specified file/directory to the specified account. \nCan only be done by the current file owner.");
                            Console.WriteLine("Avaiable Arguments: \n-h: help");
                            break;
                        }
                        if (args[2] == "system")
                        {
                            Console.WriteLine("You cannot take away permissions from the system.");
                            break;
                        }
                        if(userInfo.GetId(args[2]) == 0)
                        {
                            Console.WriteLine("This User does not exist. Please check if you respected the case.");
                            break;
                        }
                        if (!File.Exists(fsManager.getFullPath(args[1])) && !Directory.Exists(fsManager.getFullPath(args[1])))
                        {
                            Console.WriteLine("File does not exist");
                            break;
                        }
                        if (!permManager.IsOwner(fsManager.getFullPath(args[1]), userInfo.GetId()) && !userInfo.IsAdmin() && !userInfo.IsVisitor())
                        {
                            Console.WriteLine("You do not have the right to change the Permissions of this file.");
                            break;
                        }
                        if (args.Contains("-w") || args.Contains("-r"))
                        {
                            if(permManager.RemoveWriter(fsManager.getFullPath(args[1]), userInfo.GetId(args[2])))
                            {
                                Console.WriteLine("Successfully removed writing rights");
                            }
                            else
                            {
                                Console.WriteLine("Error removing writing rights");
                            }
                        }
                        if (args.Contains("-r"))
                        {
                           if ( permManager.RemoveReader(fsManager.getFullPath(args[1]), userInfo.GetId(args[2]),args[2], fsManager))
                           {
                                Console.WriteLine("Successfully removed reading rights");
                           }
                           else
                           {
                                Console.WriteLine("Couldn't remove reading rights");
                           }
                        }
                        break;
                    }
                case "pwd": //in help
                    {
                        Console.WriteLine("You are here: " + fsManager.getCurrentDir());
                        break;
                    }
                case "clear": //in help
                    {
                        Console.Clear();
                        break;
                    }
                case "shutdown": //in help
                    {
                        permManager.SavePermissions();
                        Sys.Power.Shutdown();
                        break;
                    }
                case "reboot": //in help
                    {
                        permManager.SavePermissions();
                        Sys.Power.Reboot();
                        break;
                    }
                case "force-shutdown": //in help
                    {
                        Console.WriteLine($"Are you sure you want to forcecfully shutdown? Not all changes will be saved.\n(y)es/(n)o");
                        
                        if (WaitForConfirmation())
                        {
                            Sys.Power.Shutdown();
                        }
                        break;
                    }
                case "force-reboot": //in help
                    {
                        Console.WriteLine($"Are you sure you want to forcecfully reboot? Not all changes will be saved.\n(y)es/(n)o");

                        if (WaitForConfirmation())
                        {
                            Sys.Power.Reboot();
                        }
                        break;
                    }
                case "login": //in help
                    {        
                        if (args.Length == 1)
                        {
                            userInfo.Login();
                            if (!permManager.IsReader(fsManager.getCurrentDir(),userInfo.GetId()))
                            {
                                fsManager.changeDirectory(rootdir);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Usage: Login");
                            Console.WriteLine("Description: Login to Account");
                            Console.WriteLine("Available Arguments:\n -h: help");
                        }
                        break;
                    }
                case "logout": //in help
                    {
                        if (args.Length == 1)
                        {
                            userInfo.Logout();
                            if (!permManager.IsReader(fsManager.getCurrentDir(),userInfo.GetId()))
                            {
                                fsManager.changeDirectory(rootdir);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Usage: Logout");
                            Console.WriteLine("Description: Logout of your Account");
                            Console.WriteLine("Available Arguments:\n -h: help");
                        }
                        break;
                    }
                case "listusers":
                case "lusrs":
                    {
                        string[] userlist = userInfo.GetAllUsers();
                        foreach (string user in userlist)
                        {
                            Console.WriteLine($"{user}");
                        }
                        break;
                    }
                case "deleteuser":
                case "delusr": //in help
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
                                if (args[1] == userInfo.GetName())
                                {
                                    Console.WriteLine("Cannot delete User you are logged in with");
                                    break;
                                }
                                if (userInfo.GetId(args[1]) == 0)
                                {
                                    Console.WriteLine("This user doesn't exist.");
                                    break;
                                }
                                Console.WriteLine($"Are you sure you want to delete {args[1]}? (y)es/(n)o");

                                if (!Kernel.WaitForConfirmation())
                                {
                                    Console.WriteLine("Deletion aborted");
                                    break;
                                }
                                Console.WriteLine("Deletion in progress, please wait...");
                                string[] paths = fsManager.getAllPaths(rootdir);
                                int deluserID = userInfo.GetId(args[1]);
                                foreach (string path in paths)
                                {
                                    Console.Write("|");
                                    //Console.WriteLine($"checking {path} with {deluserID} which is the id from {args[1]}");
                                    if (!Directory.Exists(path) && !File.Exists(path))
                                    {
                                        continue;
                                    }
                                    if (permManager.IsOwner(path, deluserID))
                                    {
                                        permManager.InitPermissions(path,userInfo.visitorid,true);
                                    }
                                    else
                                    { 
                                        if(permManager.IsReader(path, deluserID, false))
                                        {
                                            permManager.RemoveReader(path, deluserID, args[1], fsManager,true);
                                        }
                                        
                                        if (permManager.IsWriter(path, deluserID, false))
                                        {
                                            permManager.RemoveWriter(path, deluserID, true);
                                        }
                                    }
                                }
                                Console.WriteLine();
                                permManager.SavePermissions();
                                userInfo.DeleteUser(args[1]);
                                
                            } else
                            {
                                Console.WriteLine("You need to be an admin to do this");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Usage: deleteUser / delUsr [username]");
                            Console.WriteLine("Description: delete a User (only available to admins)");
                            Console.WriteLine("Available Arguments: \n -h: help");

                        }
                        break;
                    }
                case "createuser":
                case "mkusr": //in help
                    {
                        if (args.Length == 1)
                        {  
                            int newUser = userInfo.CreateUser(false); 
                            string newdir = fsManager.createDirectory(rootdir + userInfo.GetName(newUser));
                            Console.WriteLine("init perms now");
                            permManager.InitPermissions(newdir, newUser);
                            Console.WriteLine("Login as this User? \n(y)es/(n)o");

                            if (WaitForConfirmation())
                            {
                                userInfo.AutoLogin(newUser);
                            }
                        }
                        else if (args.Length >= 2)
                        {
                            if (args[1] == "-a")
                                if (userInfo.IsAdmin())
                                {
                                    int newUser = userInfo.CreateUser(true);
                                    string newdir = fsManager.createDirectory(rootdir + userInfo.GetName(newUser));
                                    Console.WriteLine("init perms second");
                                    permManager.InitPermissions(newdir, newUser);
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
                case "initperms": //in help
                    {
                        if (args.Length != 1)
                        { 
                            Console.WriteLine("Usage: initperms");
                            Console.WriteLine("Description: initialize Filepermissions. Only available to Admins.");
                            Console.WriteLine("Available Arguments:\n -h: help");
                            break;
                        }
                        if (!userInfo.IsAdmin())
                        {
                            Console.WriteLine("Only Admins can run this command");
                            break;
                        }
                        if (fsManager.getCurrentDir() != rootdir)
                        {
                            Console.WriteLine("Command can only get run in home directory");
                            break; 
                        }
                        InitPerms();
                        break;
                    }
                case "clearperms": //in help
                    { 
                        if (args.Length != 1)
                        {
                            Console.WriteLine("Usage: clearperms");
                            Console.WriteLine("Description: clear all Filepermissions.");
                            Console.WriteLine("Available Arguments:\n -h: help");
                            break;
                        }
                        if (!userInfo.IsAdmin())
                        {
                            Console.WriteLine("Only Admins can run this command");
                            break;
                        }
                        permManager.EmptyPerms();
                        Console.WriteLine("Permissions cleared successfully");
                        break;
                    }
                case "perm":
                case "permissions": //in help
                    {
                        if (args.Length < 2 || args[1] == "-h")
                        {
                            Console.WriteLine("Usage: perm/permissions <file name>");
                            Console.WriteLine("Description: Shows the owner, readers and writers of a file or Directory.");
                            Console.WriteLine("Avaiable Arguments: \n-h: help");
                            break;
                        }
                        string path = fsManager.getFullPath(args[1]);
                        if (!File.Exists(path) && !Directory.Exists(path) )
                        {
                            Console.WriteLine("File/Directory does not exist");
                            break;
                        }

                        int ownerID = permManager.GetOwnerID(path);
                        Console.WriteLine("owner: " + userInfo.GetName(ownerID));
                        
                        int[] readersIDs = permManager.GetReaderIDs(path);
                        Console.Write("readers: ");
                        if (readersIDs.Length == 0)
                        {
                            Console.WriteLine("nobody");
                            break;
                        }
                        for (int i = 0; i < readersIDs.Length - 1; i++)
                        {
                            Console.Write(userInfo.GetName(readersIDs[i]) + ", ");
                        }
                        Console.Write(userInfo.GetName(readersIDs[readersIDs.Length - 1]));
                        Console.WriteLine();
                        
                        int[] writerIDs = permManager.GetWriterIDs(path);
                        Console.Write("writers: ");
                        if (writerIDs.Length == 0)
                        {
                            Console.WriteLine("nobody");
                            break;
                        }
                        for (int i = 0; i < writerIDs.Length - 1; i++)
                        {
                            Console.Write(userInfo.GetName(writerIDs[i]) + ", ");
                        }
                        Console.Write(userInfo.GetName(writerIDs[writerIDs.Length - 1]));
                        Console.WriteLine();
                        break;
                    }
                case "saveperms": //in help
                    {
                        if (args.Length == 1)
                        {
                            permManager.SavePermissions();
                        }
                        else
                        {
                            Console.WriteLine("Usage: saveperms");
                            Console.WriteLine("Description: Save Permissions");
                            Console.WriteLine("Available Arguments:\n -h: help");
                        }
                        break;
                    } 
                case "changepwd": //in help
                    {
                        if(args.Length != 1)
                        {
                            Console.WriteLine("Usage: changepwd");
                            Console.WriteLine("Description: Change password");
                            Console.WriteLine("Available Arguments:\n -h: help");
                            break;
                        }
                        if (!(userInfo.GetName() == "Visitor"))
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
                        if (args.Length < 2 || args[1] == "-h")
                        {
                            Console.WriteLine("Usage: miv <file name>");
                            Console.WriteLine("Description: Edit the specified file.");
                            Console.WriteLine("Avaiable Arguments: \n-h: help");
                            break;
                        }
                        if (args.Length == 2)
                        {
                            string filePath = fsManager.getFullPath(args[1]);
                            if(!permManager.IsWriter(filePath,userInfo.GetId()) && !userInfo.IsAdmin())
                            {
                                Console.WriteLine("You do not have permission to edit this file");
                                break;
                            }
                            if (MIV.MIV.PrintMivCommands() == true)
                            {
                                MIV.MIV.StartMIV(filePath);
                            }
                        }
                        break;
                    }
                case "help":
                    {
                        HelpOutput();
                        break;
                    }
                default:
                    {
                        Console.WriteLine("Not a valid command use \"help\" for list of commands");
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

        static internal bool WaitForConfirmation()
        //waits for enter (true) or escape (false);
        {
            char confirmation = ' ';
            do
            { confirmation = Console.ReadKey(true).KeyChar; }
            while (confirmation != 'y' && confirmation != 'n');
            if (confirmation == 'y')
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static internal bool WaitForResponse()
        //waits for enter (true) or escape (false);
        {
            while (true)
            {
                var key = System.Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                    return true;
                if (key.Key == ConsoleKey.Escape)
                { return false; }
            }
        }

        internal void HelpOutput()
        {
            int pagecount = 10;
            int currentPage = 0;
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Usage: freespace");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Description: Get available free space.");
            Console.WriteLine("Avaiable Arguments: \n-h: help");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Usage: touch [-o] <directory name> ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Description: Creates a new file");
            Console.WriteLine("Avaiable Arguments: \n-h: help \n-o: give yourself complete ownership.\n    If not set, it will inherent the ownership of the Current directory.");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Usage: mkdir [-o] <directory name> ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Description: Creates a new directory");
            Console.WriteLine("Avaiable Arguments: \n-h: help \n-o: give yourself complete ownership.\n    If not set, it will inherent the ownership of the Current directory.");
            Console.WriteLine();

            currentPage++;
            Console.WriteLine("Page " + currentPage.ToString() + " out of " + pagecount.ToString() + " Continue with enter, exit with esc");
            if (!WaitForResponse()) { return; } //wait for enter or escape
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Usage: ls");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Description: List all the files in a directory");
            Console.WriteLine("Avaiable Arguments: \n-h: help");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Usage: mv <file path> <dest path>");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Description: Move file between directories");
            Console.WriteLine("Avaiable Arguments: \n-h: help");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Usage: cat <file name>");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Description: Opens a text file, reads all the text in the file,\nand then closes the file.");
            Console.WriteLine("Avaiable Arguments: \n-h: help");
            Console.WriteLine();

            currentPage++;
            Console.WriteLine("Page " + currentPage.ToString() + " out of " + pagecount.ToString() + " Continue with enter, exit with esc");
            if (!WaitForResponse()) { return; } //wait for enter or escape
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Usage: rmf / rmfile <file name>");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Description: Deletes the specified file");
            Console.WriteLine("Avaiable Arguments: \n-h: help");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Usage: rmd / rmdir <directory name>");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Description: Deletes the specified dir");
            Console.WriteLine("Avaiable Arguments: \n-h: help");

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Usage: rename <directory or file> <new name>");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Description: It Renames the selected directory or file.");
            Console.WriteLine("Avaiable Arguments: \n-h: help");
            Console.WriteLine();

            currentPage++;
            Console.WriteLine("Page " + currentPage.ToString() + " out of " + pagecount.ToString() + " Continue with enter, exit with esc");
            if (!WaitForResponse()) { return; } //wait for enter or escape
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Usage: cd <directory path>");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Description: Changes your current directory to the specified one.");
            Console.WriteLine("Avaiable Arguments: \n-h: help");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Usage: pwd");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Description: Get current working directory");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Usage: clear");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Description: clear command log");
            Console.WriteLine();

            currentPage++;
            Console.WriteLine("Page " + currentPage.ToString() + " out of " + pagecount.ToString() + " Continue with enter, exit with esc");
            if (!WaitForResponse()) { return; } //wait for enter or escape
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Usage: shutdown");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Description: Shut the Computer down");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Usage: reboot");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Description: Reboot the Computer");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Usage: force-shutdown");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Description: Force a shutdown without proper exit protocols.\nWarning: not all changes made will be saved!");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Usage: force-reboot");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Description: Force a reboot without proper exit protocols.\nWarning: not all changes made will be saved!");
            Console.WriteLine();

            currentPage++;
            Console.WriteLine("Page " + currentPage.ToString() + " out of " + pagecount.ToString() + " Continue with enter, exit with esc");
            if (!WaitForResponse()) { return; } //wait for enter or escape
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Usage: Login");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Description: Login to Account");
            Console.WriteLine("Available Arguments:\n -h: help");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Usage: Logout");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Description: Logout of your Account");
            Console.WriteLine("Available Arguments:\n -h: help");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Usage: createUser / mkUsr");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Description: Create a new User");
            Console.WriteLine("Available Arguments: \n -h: help \n -a: create Admin (Only Admins can crate Admins)");
            Console.WriteLine();

            currentPage++;
            Console.WriteLine("Page " + currentPage.ToString() + " out of " + pagecount.ToString() + " Continue with enter, exit with esc");
            if (!WaitForResponse()) { return; } //wait for enter or escape
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Usage: deleteUser [username] / delUsr [username]");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Description: delete a User (only available to admins)");
            Console.WriteLine("Available Arguments: \n -h: help");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Usage: initperms");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Description: initialize Filepermissions.  Only available to Admins.");
            Console.WriteLine("Available Arguments:\n -h: help");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Usage: perm/permissions <file name>");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Description: Shows the owner, readers and writers of a file or Directory.");
            Console.WriteLine("Avaiable Arguments: \n-h: help");
            Console.WriteLine();

            currentPage++;
            Console.WriteLine("Page " + currentPage.ToString() + " out of " + pagecount.ToString() + " Continue with enter, exit with esc");
            if (!WaitForResponse()) { return; } //wait for enter or escape
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Usage: saveperms");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Description: Save File Permissions");
            Console.WriteLine("Available Arguments:\n -h: help");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Usage: clearperms");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Description: clear all Filepermissions.");
            Console.WriteLine("Available Arguments:\n -h: help");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Usage: changepwd");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Description: Change password");
            Console.WriteLine("Available Arguments:\n -h: help");
            Console.WriteLine();

            currentPage++;
            Console.WriteLine("Page " + currentPage.ToString() + " out of " + pagecount.ToString() + " Continue with enter, exit with esc");
            if (!WaitForResponse()) { return; } //wait for enter or escape
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Usage: setOwner <path> <new owner>");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Description: Changes the owner of the specified file/directory to the specified account. \nCan only be done by the current file owner.");
            Console.WriteLine("Avaiable Arguments: \n-h: help");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Usage: gperm / givepermissions <path> <user owner> [-r] [-w]");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Description: Changes the owner of the specified file/directory to the specified account. \nCan only be done by the current file owner.");
            Console.WriteLine("Avaiable Arguments: \n-h: help");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Usage: tperm / takepermissions <path> <user owner> [-r] [-w]");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Description: Changes the owner of the specified file/directory to the specified account. \nCan only be done by the current file owner.");
            Console.WriteLine("Avaiable Arguments: \n-h: help");
            Console.WriteLine();

            currentPage++;
            Console.WriteLine("Page " + currentPage.ToString() + " out of " + pagecount.ToString() + " Continue with enter, exit with esc");
            if (!WaitForResponse()) { return; } //wait for enter or escape
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Usage: miv <file name>");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Description: Edit the specified file.");
            Console.WriteLine("Avaiable Arguments: \n-h: help");
            Console.WriteLine();

            currentPage++;
            Console.WriteLine("Page " + currentPage.ToString() + " out of " + pagecount.ToString() + " Continue with enter, exit with esc");
            if (!WaitForResponse()) { return; } //wait for enter or escape
            Console.WriteLine();
        }

        public void InitPerms()
        {

            string[] allUsers = userInfo.GetAllUsers();
            Console.WriteLine("Creating user specific Directories for the following users:");
            foreach (string user in allUsers)
            {
                Console.Write(user + " ");
            }
            Console.WriteLine();
            Console.WriteLine("Initializing root");
            permManager.InitPermissions(rootdir,userInfo.visitorid);
            foreach (string user in allUsers)
            {
                string dirpath = fsManager.getFullPath(user);
                if (dirpath == null)
                {
                    //Console.WriteLine("inavild path creation");
                    continue;
                }
                if (File.Exists(dirpath))
                {
                    //Console.WriteLine("Error: File with Username " + user + " already exists.");
                    continue;
                }
                if (!Directory.Exists(dirpath)) 
                {
                    dirpath = fsManager.createDirectory(user);
                    Console.WriteLine("Created: " + dirpath);
                }
                //Console.WriteLine("getting all paths");
                string[] allpaths = fsManager.getAllPaths(dirpath);
                foreach (string path in allpaths)
                {
                    //Console.WriteLine("init permissions for " + path);
                    if (permManager.InitPermissions(path, userInfo.GetId(user)))
                    {
                        Console.WriteLine("Set Rights of " + path + " to " + user);
                    }

                }
            }
            Console.WriteLine();
            Console.WriteLine("intializing Remaining files");
            string[] rootpaths = fsManager.getAllPaths(fsManager.getCurrentDir());
            foreach (string path in rootpaths)
            {
                if (permManager.InitPermissions(fsManager.getFullPath(path)))
                {
                    Console.WriteLine("Set Rights of " + fsManager.getFullPath(path) + " to " + userInfo.GetName(permManager.GetOwnerID(fsManager.getFullPath(path))));
                }
            }
            Console.WriteLine("initialization Successful");
            permManager.SavePermissions();
        }

    }


}
