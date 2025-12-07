using TrappistOS;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class SetOwnerCommand : AbstractCommand
{
    private FileSystemManager fsManager;
    private FilePermissions permManager;
    private UserLogin userInfo;

    public SetOwnerCommand(FileSystemManager fsManager, FilePermissions permManager, UserLogin userInfo)
    {
        this.fsManager = fsManager;
        this.permManager = permManager;
        this.userInfo = userInfo;
    }

    public override string Name => "setowner";
    public override string Description =>   "Description: Changes the owner of the specified file/directory to the specified account.\n" +
                                            "Can only be done by the current file owner.\n" +
                                            "Available Arguments:\n" +
                                            "  -h: help";
    public override string Usage => "Usage: setowner <path> <new owner>";
                                    
    public override IEnumerable<string> Parameters => new[] { "-h" };
    public override void Execute(string[] args)
    {
        if (args.Length < 3 || args[1] == "-h")
        {
            Console.WriteLine(Usage);
            Console.WriteLine(Description);
            return;
        }
        if (args[2] == "system")
        {
            Console.WriteLine("You cannot take away ownership from the system.");
            return;
        }
        if (userInfo.GetId(args[2]) == 0)
        {
            Console.WriteLine("This User does not exist. Please check if you respected the case.");
            return;
        }
        if (!File.Exists(fsManager.getFullPath(args[1])) && !Directory.Exists(fsManager.getFullPath(args[1])))
        {
            Console.WriteLine("File does not exist");
            return;
        }
        if (!permManager.IsOwner(fsManager.getFullPath(args[1]), userInfo.GetId()) && !userInfo.IsAdmin() && !userInfo.IsVisitor()
            || permManager.IsOwner(fsManager.getFullPath(args[1]), 0))
        {
            Console.WriteLine("You do not have the right to change ownership.");
            return;
        }
        Console.WriteLine($"Do you want to give Ownership of {args[1]} to {args[2]}?\nThis action is not reversable.\n(y)es/(n)o");
        
        if (WaitForConfirmation())
        {
            permManager.SetOwner(fsManager.getFullPath(args[1]), userInfo.GetId(args[2]));
            permManager.SetReader(fsManager.getFullPath(args[1]), userInfo.GetId(args[2]),true);
            permManager.SetWriter(fsManager.getFullPath(args[1]), userInfo.GetId(args[2]),true);
            Console.WriteLine("Successfully changed Owner to " + args[2]);
        }
        else
        {
            Console.WriteLine("Change aborted.");
        }

        permManager.SavePermissions();

    }

    private bool WaitForConfirmation()
    {
        string input = Console.ReadLine()?.Trim().ToLower();
        return input == "y" || input == "yes";
    }
}

public class GivePermissionsCommand : AbstractCommand
{
    private FileSystemManager fsManager;
    private FilePermissions permManager;
    private UserLogin userInfo;

    public GivePermissionsCommand(FileSystemManager fsManager, FilePermissions permManager, UserLogin userInfo)
    {
        this.fsManager = fsManager;
        this.permManager = permManager;
        this.userInfo = userInfo;
    }

    public override string Name => "gperm";
    public override string Description =>   "Description: Grants read and/or write permissions to a user for the specified file/directory.\n" +
                                            "Available Arguments:\n" +
                                            "  -h: help\n" +
                                            "  -r: grant read permission\n" +
                                            "  -w: grant write permission";
    public override string Usage => "Usage: gperm / givepermissions <path> <user> [-r] [-w]";
    public override IEnumerable<string> Parameters => new[] { "-h", "-r", "w"};

    public override void Execute(string[] args)
    {
        if (args.Length < 4 || args.Contains("-h"))
        {
            Console.WriteLine(Usage);
            Console.WriteLine(Description);
            return;
        }
        if (args[2] == "system")
        {
            Console.WriteLine("You cannot give permissions to the system.");
            return;
        }
        if (userInfo.GetId(args[2]) == 0)
        {
            Console.WriteLine("This User does not exist. Please check if you respected the case.");
            return;
        }
        if (!File.Exists(fsManager.getFullPath(args[1])) && !Directory.Exists(fsManager.getFullPath(args[1])))
        {
            Console.WriteLine("File does not exist");
            return;
        }
        if ((!permManager.IsOwner(fsManager.getFullPath(args[1]), userInfo.GetId()) && !userInfo.IsAdmin() && !userInfo.IsVisitor()) 
            || permManager.IsOwner(fsManager.getFullPath(args[1]),0))
        {
            Console.WriteLine("You do not have the right to change the Permissions of this file.");
            return;
        }
        if (args.Contains("-r")||args.Contains("-w"))
        {
            if(permManager.SetReader(fsManager.getFullPath(args[1]),userInfo.GetId(args[2]), (!args.Contains("-r") && args.Contains("-w"))))
            {
                if (args.Contains("-r") && !args.Contains("-w"))
                {
                    Console.WriteLine("Successfully added reading rights");
                }
                
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

        permManager.SavePermissions();

    }
}

public class TakePermissionsCommand : AbstractCommand
{
    private FileSystemManager fsManager;
    private FilePermissions permManager;
    private UserLogin userInfo;

    public TakePermissionsCommand(FileSystemManager fsManager, FilePermissions permManager, UserLogin userInfo)
    {
        this.fsManager = fsManager;
        this.permManager = permManager;
        this.userInfo = userInfo;
    }

    public override string Name => "tperm"; 
    public override string Description =>   "Description: Removes read and/or write permissions from a user for the specified file/directory.\n" +
                                            "Available Arguments:\n" +
                                            "  -h: help\n" +
                                            "  -r: remove read permission\n" +
                                            "  -w: remove write permission";
    public override string Usage => "Usage: tperm / takepermissions <path> <user> [-r] [-w]";
    public override IEnumerable<string> Parameters => new[] { "-h", "-r", "w"};
    public override void Execute(string[] args)
    {
        if (args.Length < 4 || args.Contains("-h"))
        {
            Console.WriteLine(Usage);
            Console.WriteLine(Description);
            return;
        }
        if (args[2] == "system")
        {
            Console.WriteLine("You cannot take away permissions from the system.");
            return;
        }
        if(userInfo.GetId(args[2]) == 0)
        {
            Console.WriteLine("This User does not exist. Please check if you respected the case.");
            return;
        }
        if (!File.Exists(fsManager.getFullPath(args[1])) && !Directory.Exists(fsManager.getFullPath(args[1])))
        {
            Console.WriteLine("File does not exist");
            return;
        }
        if (!permManager.IsOwner(fsManager.getFullPath(args[1]), userInfo.GetId()) && !userInfo.IsAdmin() && !userInfo.IsVisitor())
        {
            Console.WriteLine("You do not have the right to change the Permissions of this file.");
            return;
        }
        if (args.Contains("-w") || args.Contains("-r"))
        {
            if(permManager.RemoveWriter(fsManager.getFullPath(args[1]), userInfo.GetId(args[2]), (args.Contains("-r") && !args.Contains("-w"))))
            {
                if (!args.Contains("-r") && args.Contains("-w"))
                {
                    Console.WriteLine("Successfully removed writing rights");
                }
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

        permManager.SavePermissions();
    }
}

public class InitPermsCommand : AbstractCommand
{
    private UserLogin userInfo;
    private FileSystemManager fsManager;
    private FilePermissions permManager;
    private string rootDir;

    public InitPermsCommand(UserLogin userInfo, FileSystemManager fsManager, FilePermissions permManager, string rootDir)
    {
        this.userInfo = userInfo;
        this.fsManager = fsManager;
        this.permManager = permManager;
        this.rootDir = rootDir;
    }

    public override string Name => "initperms";
    public override string Description => "Description: Initializes file permissions. Only available to Admins.";
    public override string Usage => "Usage: initperms\n";
    public override IEnumerable<string> Parameters => new[] { "-h" };
    public override void Execute(string[] args)
    {
        if (args.Length != 1)
            { 
                Console.WriteLine(Usage);
                Console.WriteLine(Description);
            return;
            }
            if (!userInfo.IsAdmin())
            {
                Console.WriteLine("Only Admins can run this command");
                return;
            }
            if (fsManager.getCurrentDir() != rootDir)
            {
                Console.WriteLine("Command can only get run in home directory");
                return; 
            }
            InitPerms();
    }

    public void InitPerms(bool quiet = false)
        {

            string[] allUsers = userInfo.GetAllUsers();
            if (!quiet)
            {
                Console.WriteLine("Creating user specific Directories for the following users:");
                foreach (string user in allUsers)
                {
                    Console.Write(user + " ");
                }
                Console.WriteLine();
                Console.WriteLine("Initializing root");
            }
            permManager.InitPermissions(rootDir,userInfo.visitorid);
            if(quiet)
            {
                Console.Write("|");
            }
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
                    dirpath = fsManager.createDirectory(user,true);
                    if (!quiet)
                    {
                        Console.WriteLine("Created: " + dirpath);
                    }
                }
                //Console.WriteLine("getting all paths");
                string[] allpaths = fsManager.getAllPaths(dirpath);
                foreach (string path in allpaths)
                {
                    //Console.WriteLine("init permissions for " + path);
                    if (permManager.InitPermissions(path, userInfo.GetId(user),shouldsave: false))
                    {
                        if(!quiet)
                        {
                            Console.WriteLine("Set Rights of " + path + " to " + user);
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
}

public class ClearPermsCommand : AbstractCommand
{
    private UserLogin userInfo;
    private FilePermissions permManager;

    public ClearPermsCommand(UserLogin userInfo, FilePermissions permManager)
    {
        this.userInfo = userInfo;
        this.permManager = permManager;
    }

    public override string Name => "clearperms";
    public override string Description => "Clears all file permissions. Only available to Admins.";
    public override string Usage => "Usage: clearperms";
    public override IEnumerable<string> Parameters => new[] { "-h" };
    public override void Execute(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine(Usage);
            Console.WriteLine(Description);
            return;
        }
        if (!userInfo.IsAdmin())
        {
            Console.WriteLine("Only Admins can run this command");
            return;
        }
        permManager.EmptyPerms();
        Console.WriteLine("Permissions cleared successfully");
    }
}

public class PermissionsCommand : AbstractCommand
{
    private FileSystemManager fsManager;
    private FilePermissions permManager;
    private UserLogin userInfo;

    public PermissionsCommand(FileSystemManager fsManager, FilePermissions permManager, UserLogin userInfo)
    {
        this.fsManager = fsManager;
        this.permManager = permManager;
        this.userInfo = userInfo;
    }

    public override string Name => "perm";
    public override string Description => "Description: Displays the owner, readers, and writers of a file or directory.\n" +
                                          "Available Arguments:\n" +
                                          "  -h: help";
    public override string Usage => "Usage: perm / permissions <file name>";
                                    
    public override IEnumerable<string> Parameters => new[] { "-h" };
    public override void Execute(string[] args)
    {
        if (args.Length < 2 || args[1] == "-h")
        {
            Console.WriteLine(Usage);
            Console.WriteLine(Description);
            return;
        }

        string path = fsManager.getFullPath(args[1]);

        if (!File.Exists(path) && !Directory.Exists(path) )
        {
            Console.WriteLine("File/Directory does not exist");
            return;
        }

        int ownerID = permManager.GetOwnerID(path);
        Console.WriteLine("owner: " + userInfo.GetName(ownerID,true));

        int[] readersIDs = permManager.GetReaderIDs(path);
        Console.Write("readers: ");

        if (readersIDs.Length == 0)
        {
            Console.WriteLine("nobody");
            return;
        }

        for (int i = 0; i < readersIDs.Length - 1; i++)
        {
            Console.Write(userInfo.GetName(readersIDs[i], true) + ", ");
        }

        Console.Write(userInfo.GetName(readersIDs[readersIDs.Length - 1], true));
        Console.WriteLine();

        int[] writerIDs = permManager.GetWriterIDs(path);
        Console.Write("writers: ");

        if (writerIDs.Length == 0)
        {
            Console.WriteLine("nobody");
            return;
        }

        for (int i = 0; i < writerIDs.Length - 1; i++)
        {
            Console.Write(userInfo.GetName(writerIDs[i],true) + ", ");
        }

        Console.Write(userInfo.GetName(writerIDs[writerIDs.Length - 1], true));
        Console.WriteLine();
    }
}

public class SavePermsCommand : AbstractCommand
{
    private FilePermissions permManager;

    public SavePermsCommand(FilePermissions permManager)
    {
        this.permManager = permManager;
    }

    public override string Name => "saveperms";
    public override string Description => "Description: Saves all file permissions.";
    public override string Usage => "Usage: saveperms";
    public override IEnumerable<string> Parameters => new[] { "-h" };
    public override void Execute(string[] args)
    {
        if (args.Length == 1)
        {
            permManager.SavePermissions();
        }
        else
        {
            Console.WriteLine(Usage);
            Console.WriteLine(Description);
        }
    }
}






