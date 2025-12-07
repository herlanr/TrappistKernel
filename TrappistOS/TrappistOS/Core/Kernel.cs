using Cosmos.System.ScanMaps;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sys = Cosmos.System;

//wait function: Cosmos.HAL.Global.PIT.Wait((uint)10000);

namespace TrappistOS
{
    public class Kernel : Sys.Kernel
    {
        public static bool AbortRequest { get; set; } = false;

        string userpath = @"0:\users.sys";
        string rootdir = @"0:\";

        FileSystemManager fsManager;
        UserLogin userInfo;
        FilePermissions permManager;
        CommandRegistry registry;
        List<string> commandList;

        private static readonly Random _rng = new Random();

        protected override void BeforeRun()
        {
            Console.WriteLine("TrappistOS booting");
            fsManager = new FileSystemManager();
            fsManager.fsInitialize();
            Sys.KeyboardManager.SetKeyLayout(new DE_Standard());
            Console.WriteLine("Filesystem initialized");
            Console.Clear();

            printSystemInfo();

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
                    InitPerms(true);
                    Console.WriteLine("Filepermissions initialized");

                    //regitring commands
                    registry = new CommandRegistry();
                    includeCommandsToRegister();
                    //erstelle eine command liste und gibt die zu CommandHistory() im Run() weiter (f�r AutoComplete)
                    commandList = registry.GetAllCommandNames().ToList();

                    Console.WriteLine("Commands initialized");

                    Console.Clear();

                    if (_rng.Next(10) == 0)   // 0..9 → genau 10 %
                    {
                        Console.WriteLine();
                        Console.WriteLine("Orbit locked.");
                        Console.WriteLine("Habitable zone detected around TRAPPIST-1.");
                        Console.WriteLine("Preparing TrappistOS command interface...");
                        Console.WriteLine();
                    }

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
            var cmd = new CommandHistory(commandList);
            var input = cmd.ReadLine(userInfo.GetName(true), fsManager.getCurrentDir());

            if (string.IsNullOrWhiteSpace(input))
            {
                // Einfach nichts machen und nächste Run()-Iteration abwarten
                Kernel.AbortRequest = false;
                return;
            }

            Kernel.AbortRequest = false;

            string[] args = input.Split(' ');
            args[0] = args[0].ToLower();
            AbstractCommand command = registry.Get(args[0]);

            if (command != null)
            {
                command.Execute(args);
            }
            else
            {
                Console.WriteLine("Command not found.");
            }

        }

        static internal bool WaitForConfirmation()
        {
            char confirmation = ' ';
            do
            {
                var keyInfo = Console.ReadKey(true);

                if ((keyInfo.Modifiers & ConsoleModifiers.Control) != 0 &&
                    keyInfo.Key == ConsoleKey.C)
                {
                    Kernel.AbortRequest = true;
                    return false; // Abbruch = „nein“
                }

                confirmation = keyInfo.KeyChar;
            }
            while (confirmation != 'y' && confirmation != 'n');

            return confirmation == 'y';
        }

        public void InitPerms(bool quiet = false)
        {

            Tuple<string, int>[] allUsers = userInfo.GetAllUsersWithID();
            if (!quiet)
            {
                Console.WriteLine("Creating user specific Directories for the following users:");
                foreach (Tuple<string, int> user in allUsers)
                {
                    Console.Write(user.Item1 + " ");
                }
                Console.WriteLine();
                Console.WriteLine("Initializing root");
            }
            permManager.InitPermissions(rootdir,userInfo.visitorid);
            if(quiet)
            {
                Console.Write("|");
            }
            foreach (Tuple<string, int> user in allUsers)
            {
                string dirpath = fsManager.getFullPath(user.Item1);
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
                    dirpath = fsManager.createDirectory(user.Item1,true);
                    if (!quiet)
                    {
                        Console.WriteLine("Created: " + dirpath);
                    }
                }
                Console.WriteLine("getting all paths");
                string[] allpaths = fsManager.getAllPaths(dirpath);
                foreach (string path in allpaths)
                {
                    //Console.WriteLine("init permissions for " + path);
                    if (permManager.InitPermissions(path, user.Item2,shouldsave: false))
                    {
                        if(!quiet)
                        {
                            Console.WriteLine("Set Rights of " + path + " to " + user.Item1);
                        }
                        
                    }
                    if (quiet)
                    {
                        Console.Write("|");
                    }

                }
            }
            if(!quiet)
            {
                Console.WriteLine();
                Console.WriteLine("intializing Remaining files");
            }
            
            string[] rootpaths = fsManager.getAllPaths(fsManager.getCurrentDir());
            foreach (string path in rootpaths)
            {
                if (permManager.InitPermissions(fsManager.getFullPath(path),shouldsave: false))
                {
                    if(!quiet)
                    {
                        Console.WriteLine("Set Rights of " + fsManager.getFullPath(path) + " to " + userInfo.GetName(permManager.GetOwnerID(fsManager.getFullPath(path)), true));
                    }
                    
                }
                if (quiet)
                {
                    Console.Write("|");
                }
            }
            if(!quiet)
            {
                Console.WriteLine("initialization Successful");
            }
            
            permManager.SavePermissions();
        }

        public void printSystemInfo()
            {
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
            }

        private void includeCommandsToRegister()
        {
            registry.Register(new FreeSpaceCommand(fsManager));
            registry.Register(new TouchCommand(fsManager, permManager, userInfo));
            registry.Register(new MkdirCommand(fsManager, permManager, userInfo));
            registry.Register(new LsCommand(fsManager));
            registry.Register(new MvCommand(fsManager, permManager, userInfo));
            registry.Register(new CatCommand(fsManager, permManager, userInfo));
            registry.Register(new RmFileCommand(fsManager, permManager, userInfo));
            registry.Register(new RmDirCommand(fsManager, permManager, userInfo));
            registry.Register(new RenameCommand(fsManager, permManager, userInfo));
            registry.Register(new CdCommand(fsManager, permManager, userInfo));
            registry.Register(new SetOwnerCommand(fsManager, permManager, userInfo));
            registry.Register(new GivePermissionsCommand(fsManager, permManager, userInfo));
            registry.Register(new TakePermissionsCommand(fsManager, permManager, userInfo));
            registry.Register(new PwdCommand(fsManager));
            registry.Register(new ClearCommand());
            registry.Register(new ShutdownCommand(permManager));
            registry.Register(new RebootCommand(permManager));
            registry.Register(new ForceShutdownCommand());
            registry.Register(new ForceRebootCommand());
            registry.Register(new LoginCommand(userInfo, fsManager, permManager, rootdir));
            registry.Register(new LogoutCommand(userInfo, fsManager, permManager, rootdir));
            registry.Register(new ListUsersCommand(userInfo));
            registry.Register(new DeleteUserCommand(userInfo, fsManager, permManager, rootdir));
            registry.Register(new CreateUserCommand(userInfo, fsManager, permManager, rootdir));
            registry.Register(new InitPermsCommand(userInfo, fsManager, permManager, rootdir));
            registry.Register(new ClearPermsCommand(userInfo, permManager));
            registry.Register(new PermissionsCommand(fsManager, permManager, userInfo));
            registry.Register(new SavePermsCommand(permManager));
            registry.Register(new ChangePwdCommand(userInfo));
            registry.Register(new MivCommand(fsManager, permManager, userInfo));
            registry.Register(new SnakeCommand());
            registry.Register(new HelpCommand(registry));
            registry.Register(new TrappistCommand());
        }
        
    }
}
