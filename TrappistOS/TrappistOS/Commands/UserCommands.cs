using TrappistOS;
using System;
using System.IO;
using System.Collections.Generic;
public class LoginCommand : AbstractCommand
{
    private UserLogin userInfo;
    private FileSystemManager fsManager;
    private FilePermissions permManager;
    private string rootDir;

    public LoginCommand(UserLogin userInfo, FileSystemManager fsManager, FilePermissions permManager, string rootDir)
    {
        this.userInfo = userInfo;
        this.fsManager = fsManager;
        this.permManager = permManager;
        this.rootDir = rootDir;
    }

    public override string Name => "login";
    public override string Description => "Logs into your account.";
    public override string Usage => "Usage: login\nDescription: Login to your account.";
    public override IEnumerable<string> Parameters => new[] { "-h" };
    public override void Execute(string[] args)
    {
        if (args.Length == 1)
        {
            userInfo.Login();
            if (!permManager.IsReader(fsManager.getCurrentDir(),userInfo.GetId()))
            {
                fsManager.changeDirectory(rootDir);
            }
        }
        else
        {
            Console.WriteLine(Usage);
        }
    }
}

public class LogoutCommand : AbstractCommand
{
    private UserLogin userInfo;
    private FileSystemManager fsManager;
    private FilePermissions permManager;
    private string rootDir;

    public LogoutCommand(UserLogin userInfo, FileSystemManager fsManager, FilePermissions permManager, string rootDir)
    {
        this.userInfo = userInfo;
        this.fsManager = fsManager;
        this.permManager = permManager;
        this.rootDir = rootDir;
    }

    public override string Name => "logout";
    public override string Description => "Logs out of your account.";
    public override string Usage => "Usage: logout\nDescription: Logout from your account.";
    public override IEnumerable<string> Parameters => new[] { "-h" };
    public override void Execute(string[] args)
    {
        if (args.Length == 1)
        {
            userInfo.Logout();
            if (!permManager.IsReader(fsManager.getCurrentDir(),userInfo.GetId()))
            {
                fsManager.changeDirectory(rootDir);
            }
        }
        else
        {
            Console.WriteLine(Usage);
        }
    }
}

public class ListUsersCommand : AbstractCommand
{
    private UserLogin userInfo;

    public ListUsersCommand(UserLogin userInfo)
    {
        this.userInfo = userInfo;
    }

    public override string Name => "listusers"; 
    public override string Description => "Lists all registered users.";
    public override string Usage => "Usage: lusrs / listusers\nDescription: Displays all users in the system.";
    public override IEnumerable<string> Parameters => new[] { "-h" };
    public override void Execute(string[] args)
    {
        string[] userList = userInfo.GetAllUsers();
        foreach (string user in userList)
        {
            Console.WriteLine(user);
        }
    }
}

public class DeleteUserCommand : AbstractCommand
{
    private UserLogin userInfo;
    private FileSystemManager fsManager;
    private FilePermissions permManager;
    private string rootDir;

    public DeleteUserCommand(UserLogin userInfo, FileSystemManager fsManager, FilePermissions permManager, string rootDir)
    {
        this.userInfo = userInfo;
        this.fsManager = fsManager;
        this.permManager = permManager;
        this.rootDir = rootDir;
    }

    public override string Name => "delusr";
    public override string Description => "Deletes a user account (admin only).";
    public override string Usage => "Usage: delusr [username]\nDescription: Deletes a user account (only available to admins).";
    public override IEnumerable<string> Parameters => new[] { "-h" };
    public override void Execute(string[] args)
    {
         if (args.Length == 2 && args[1] != "-h")
        {
            if (userInfo.IsAdmin())
            {
                if (args[1] == "Visitor" || args[1] == "Admin")
                {
                    Console.WriteLine($"Cannot delete {args[1]}");
                    return;
                }
                if (args[1] == userInfo.GetName(true))
                {
                    Console.WriteLine("Cannot delete User you are logged in with");
                    return;
                }
                if (userInfo.GetId(args[1]) == 0)
                {
                    Console.WriteLine("This user doesn't exist.");
                    return;
                }
                Console.WriteLine($"Are you sure you want to delete {args[1]}? (y)es/(n)o");

                if (!Kernel.WaitForConfirmation())
                {
                    Console.WriteLine("Deletion aborted");
                    return;
                }

                Console.WriteLine("Deletion in progress, please wait...");
                string[] paths = fsManager.getAllPaths(rootDir);
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
            Console.WriteLine(Usage);

        }
    }
}

public class CreateUserCommand : AbstractCommand
{
    private UserLogin userInfo;
    private FileSystemManager fsManager;
    private FilePermissions permManager;
    private string rootDir;

    public CreateUserCommand(UserLogin userInfo, FileSystemManager fsManager, FilePermissions permManager, string rootDir)
    {
        this.userInfo = userInfo;
        this.fsManager = fsManager;
        this.permManager = permManager;
        this.rootDir = rootDir;
    }

    public override string Name => "createuser";
    public override string Description => "Creates a new user account.";
    public override string Usage => "Usage: createuser [-a]\n" +
                                    "Description: Creates a new user account.\n" +
                                    "Available Arguments:\n" +
                                    "  -h: help\n" +
                                    "  -a: create Admin (Only Admins can create Admins)";
    public override IEnumerable<string> Parameters => new[] { "-h", "-a"};
    public override void Execute(string[] args)
    {
        if (args.Length == 1)
        {  

            int newUser = userInfo.CreateUser(false); 
            string newdir = fsManager.createDirectory(rootDir + userInfo.GetName(newUser,true));

            Console.WriteLine($"init perms for {newdir}");
            permManager.InitPermissions(newdir, newUser);
            Console.WriteLine("Login as this User? \n(y)es/(n)o");

            if (!Kernel.WaitForConfirmation())
            {
                Console.WriteLine("Auto-login aborted");

                if (Kernel.AbortRequest)
                {
                    Kernel.AbortRequest = false;
                }
            }
            else
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
                    string newdir = fsManager.createDirectory(rootDir + userInfo.GetName(newUser,true));
                    Console.WriteLine($"init perms for {newdir}");
                    permManager.InitPermissions(newdir, newUser);
                }
                else
                {
                    Console.WriteLine("Only Admins can create Admins");
                }
            else
            {
                Console.WriteLine(Usage);
            }
        }
        
    }
}

public class ChangePwdCommand : AbstractCommand
{
    private UserLogin userInfo;

    public ChangePwdCommand(UserLogin userInfo)
    {
        this.userInfo = userInfo;
    }

    public override string Name => "changepwd";
    public override string Description => "Changes the password of the current user.";
    public override string Usage => "Usage: changepwd\nDescription: Change your account password.";
    public override IEnumerable<string> Parameters => new[] { "-h" };
    public override void Execute(string[] args)
    {
        if(args.Length != 1)
        {
            Console.WriteLine(Usage);
            return;
        }
        if (!(userInfo.GetName(true) == "Visitor"))
        { 
            userInfo.Changepassword(); 
        }
        else
        {
            Console.WriteLine("Can't change Password of Visitor");
        }
    }
}





