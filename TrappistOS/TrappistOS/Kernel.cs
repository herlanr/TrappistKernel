using Cosmos.System.ScanMaps;
using System;
using System.IO;
using Sys = Cosmos.System;

//wait function: Cosmos.HAL.Global.PIT.Wait((uint)10000);

namespace TrappistOS
{
    public class Kernel : Sys.Kernel
    {
        string userpath = @"0:\users.sys";

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

            userInfo = new UserLogin();
            userInfo.BeforeRun(userpath);
            Console.WriteLine("Usermanagement intialized");

            permManager = new FilePermissions();
            permManager.PermInit(userInfo,new[] { userpath } );
            Console.WriteLine("Filepermissions initialized");
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
                    ProgramMemory[0] = userInfo;
                    userInfo.BeforeRun();
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
                        
                        string path = fsManager.createFile(args[1]);
                        permManager.InitPermissions(path, userInfo.GetId());
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
                        string path = fsManager.createDirectory(args[1]);
                        permManager.InitPermissions(path, userInfo.GetId());
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
                                Console.WriteLine("Description: Move file between directories");
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
                            Console.WriteLine("Description: Opens a text file, reads all the text in the file,\nand then closes the file.");
                            Console.WriteLine("Avaiable Arguments: \n-h: help");
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

                case "rmfile":
                    {
                        if (args.Length < 2 || args[1] == "-h")
                        {
                            Console.WriteLine("Usage: rm <file name>");
                            Console.WriteLine("Description: Deletes the specified file");
                            Console.WriteLine("Avaiable Arguments: \n-h: help");
                            break;
                        }

                        fsManager.deleteFile(args[1]);
                        break;
                    }
                case "rmdir":
                    {
                        if (args.Length < 2 || args[1] == "-h")
                        {
                            Console.WriteLine("Usage: rm <directory name>");
                            Console.WriteLine("Description: Deletes the specified dir");
                            Console.WriteLine("Avaiable Arguments: \n-h: help");
                            break;
                        }

                        fsManager.deleteDir(args[1]);
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

                        fsManager.renameFileOrDir(args[1], args[2]);
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
                        permManager.SavePermissions();
                        Sys.Power.Shutdown();
                        break;
                    }
                case "reboot":
                    {
                        permManager.SavePermissions();
                        Sys.Power.Reboot();
                        break;
                    }
                case "force-shutdown":
                    {
                        Console.WriteLine($"Are you sure you want to forcecfully shutdown? Not all changes will be saved.\n(y)es/(n)o");
                        char confimation = ' ';
                        do
                        { confimation = Console.ReadKey(true).KeyChar; }
                        while (confimation != 'y' && confimation != 'n');
                        if (confimation == 'y')
                        {
                            Sys.Power.Shutdown();
                        }
                        break;
                    }
                case "force-reboot":
                    {
                        Console.WriteLine($"Are you sure you want to forcecfully reboot? Not all changes will be saved.\n(y)es/(n)o");
                        char confimation = ' ';
                        do
                        { confimation = Console.ReadKey(true).KeyChar; }
                        while (confimation != 'y' && confimation != 'n');
                        if (confimation == 'y')
                        {
                            Sys.Power.Reboot();
                        }
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
                                if (args[1] == userInfo.GetName())
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
                case "initperms":
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
                        if (fsManager.getCurrentDir() != @"0:\")
                        {
                            Console.WriteLine("Command can only get run in home directory");
                            break; 
                        }
                        string[] allUsers = userInfo.GetAllUsers();
                        Console.WriteLine("intializing Rights with Visitor");
                        string[] rootpaths = fsManager.getAllPaths(fsManager.getCurrentDir());
                        foreach (string path in rootpaths)
                        {
                            permManager.InitPermissions(path, userInfo.GetUserID("Visitor"));
                        }
                        Console.WriteLine("initialization successful.\nCreating user specific Directories for the following users:");
                        foreach (string user in allUsers)
                        {
                            Console.Write(user + " ");
                        }
                        Console.WriteLine();
                        foreach (string user in allUsers)
                        {
                            string dirpath = fsManager.getFullPath(user);
                            if (dirpath == null)
                            {
                                dirpath = fsManager.createDirectory(user);
                                Console.WriteLine("Created: "+ dirpath);
                            }
                            if(File.Exists(dirpath))
                            {
                                Console.WriteLine("Error: File with Username " + user + " already exists.");
                                continue;
                            }
                            string[] allpaths = fsManager.getAllPaths(dirpath);
                            foreach(string path in allpaths)
                            {
                                if (permManager.InitPermissions(path, userInfo.GetUserID(user)))
                                {
                                    Console.WriteLine("Set Rights of " + path + " to " + user);
                                }
                                else
                                {
                                    Console.WriteLine(path + " already has defined rights.");
                                }
                                
                            }
                        }
                        Console.WriteLine("initialization Successful");
                        //Console.SetOut(_oldOut);
                        //Console.SetError(_oldError);
                        break;
                    }
                case "clearperms":
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
                case "permissions":
                    {
                        if (args.Length < 2 || args[1] == "-h")
                        {
                            Console.WriteLine("Usage: perm/permissions <file name>");
                            Console.WriteLine("Description: Shows the owner, readers and writers of a file or Directory.");
                            Console.WriteLine("Avaiable Arguments: \n-h: help");
                            break;
                        }
                        string path = fsManager.getFullPath(args[1]);
                        if (path is null)
                        {
                            Console.WriteLine("File/Directory does not exist");
                            break;
                        }

                        int ownerID = permManager.GetOwnerID(path);
                        Console.WriteLine("owner: " + userInfo.GetUserName(ownerID));
                        
                        int[] readersIDs = permManager.GetReaderIDs(path);
                        Console.Write("readers: ");
                        if (readersIDs.Length == 0)
                        {
                            Console.WriteLine("nobody");
                            break;
                        }
                        for (int i = 0; i < readersIDs.Length - 1; i++)
                        {
                            Console.Write(userInfo.GetUserName(readersIDs[i]) + ", ");
                        }
                        Console.Write(userInfo.GetUserName(readersIDs[readersIDs.Length - 1]));
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
                            Console.Write(userInfo.GetUserName(writerIDs[i]) + ", ");
                        }
                        Console.Write(userInfo.GetUserName(writerIDs[writerIDs.Length - 1]));
                        Console.WriteLine();
                        break;
                    }
                case "saveperms":
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
                case "changepwd":
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

        internal bool WaitForResponse()
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
            int pagecount = 7;
            int currentPage = 0;
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Usage: freespace");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Description: Get available free space.");
            Console.WriteLine("Avaiable Arguments: \n-h: help");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Usage: mkdir <directory name> ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Description: Creates a new directory");
            Console.WriteLine("Avaiable Arguments: \n-h: help");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Usage: mkdir <directory name> ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Description: Creates a new directory");
            Console.WriteLine("Avaiable Arguments: \n-h: help");
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
            Console.WriteLine("Usage: rm <file name OR directory name>");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Description: Deletes the specified file or dir");
            Console.WriteLine("Avaiable Arguments: \n-h: help");
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
        }

    }


}
