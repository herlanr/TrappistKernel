using TrappistOS;
using System;
using System.IO;
using System.Collections.Generic;


public class TouchCommand : AbstractCommand
{
    private FileSystemManager fsManager;
    private FilePermissions permManager;
    private UserLogin userInfo;

    public TouchCommand(FileSystemManager fsManager, FilePermissions permManager, UserLogin userInfo)
    {
        this.fsManager = fsManager;
        this.permManager = permManager;
        this.userInfo = userInfo;
    }

    public override string Name => "touch";
    public override string Description => "Creates a new file.";
    public override string Usage => "Usage: touch [-o] <file name>\n" +
                                    "Description: Creates a new file.\n" +
                                    "Available Arguments:\n" +
                                    "  -h: help\n" +
                                    "  -o: give yourself complete ownership.\n" +
                                    "      If not set, it will inherit the ownership of the current directory.";
    public override IEnumerable<string> Parameters => new[] { "-h", "-o"};
    public override void Execute(string[] args)
    {
        if (args.Length < 2 || args[1] == "-h")
        {
            Console.WriteLine(Usage);
            return;
        }
        if (!permManager.IsWriter(fsManager.getCurrentDir(), userInfo.GetId()))
        {
            Console.WriteLine("You do not have permission to create files here.");
            return;
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
    }
}

public class MkdirCommand : AbstractCommand
{
    private FileSystemManager fsManager;
    private FilePermissions permManager;
    private UserLogin userInfo;

    public MkdirCommand(FileSystemManager fsManager, FilePermissions permManager, UserLogin userInfo)
    {
        this.fsManager = fsManager;
        this.permManager = permManager;
        this.userInfo = userInfo;
    }

    public override string Name => "mkdir";
    public override string Description => "Creates a new directory.";
    public override string Usage => "Usage: mkdir [-o] <directory name>\n" +
                                    "Description: Creates a new directory.\n" +
                                    "Available Arguments:\n" +
                                    "  -h: help\n" +
                                    "  -o: give yourself complete ownership.\n" +
                                    "      If not set, it will inherit the ownership of the current directory.";
    public override IEnumerable<string> Parameters => new[] { "-h", "-o"};

    public override void Execute(string[] args)
    {
        if (args.Length < 2 || args[1] == "-h")
        {
            Console.WriteLine(Usage);
            return;
        }

        if (!permManager.IsWriter(fsManager.getCurrentDir(), userInfo.GetId()))
        {
            Console.WriteLine("You do not have permission to create directories here.");
            return;
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
            if(!permManager.IsOwner(fsManager.getCurrentDir(), userInfo.GetId()) && !permManager.IsOwner(fsManager.getCurrentDir(), userInfo.visitorid) && permManager.IsWriter(fsManager.getCurrentDir(), userInfo.GetId()))
            {

                permManager.SetReader(path, userInfo.GetId(), true);
                permManager.SetWriter(path, userInfo.GetId(), true);
            }
        }
    }
}

public class LsCommand : AbstractCommand
{
    private FileSystemManager fsManager;

    public LsCommand(FileSystemManager fsManager)
    {
        this.fsManager = fsManager;
    }

    public override string Name => "ls";
    public override string Description => "List all the files in the current directory.";
    public override string Usage => "Usage: ls\n" +
                                    "Description: List all the files in a directory.\n" +
                                    "Available Arguments:\n" +
                                    "  -h: help";
    public override IEnumerable<string> Parameters => new[] { "-h" };
    public override void Execute(string[] args)
    {
        if (args.Length > 1)
        {
            if (args[1] == "-h")
            {
                Console.WriteLine(Usage);
                return;
            }
        }
        fsManager.listFiles();
    }
}

public class MvCommand : AbstractCommand
{
    private FileSystemManager fsManager;
    private FilePermissions permManager;
    private UserLogin userInfo;

    public MvCommand(FileSystemManager fsManager, FilePermissions permManager, UserLogin userInfo)
    {
        this.fsManager = fsManager;
        this.permManager = permManager;
        this.userInfo = userInfo;
    }

    public override string Name => "mv";
    public override string Description => "Move file or directory to a new location.";
    public override string Usage => "Usage: mv <file path> <dest path>\n" +
                                    "Description: Move file between directories.\n" +
                                    "Available Arguments:\n" +
                                    "  -h: help";
    public override IEnumerable<string> Parameters => new[] { "-h" };

    public override void Execute(string[] args)
    {
        if (args.Length < 3 || args[1] == "-h")
        {
            Console.WriteLine(Usage);
            return;

        }
        if (!File.Exists(fsManager.getFullPath(args[1])) && !Directory.Exists(fsManager.getFullPath(args[1])))
        {
            Console.WriteLine("File does not exist");
            return;
        }
        if (!permManager.IsWriter(fsManager.getFullPath(args[1]), userInfo.GetId()) && !userInfo.IsAdmin())
        {
            Console.WriteLine("You do not have permissions to move this file.");
            return;
        }

        string oldpath = fsManager.getFullPath(args[1]);
        string newpath = fsManager.moveFile(args[1], args[2]);

        if (newpath != null)
        {
            permManager.switchPermissionPath(oldpath, newpath, false);
            permManager.SavePermissions();
        }
        else
        {
            Console.WriteLine("Invalid new path");
        }

    }
}

public class CatCommand : AbstractCommand
{
    private FileSystemManager fsManager;
    private FilePermissions permManager;
    private UserLogin userInfo;

    public CatCommand(FileSystemManager fsManager, FilePermissions permManager, UserLogin userInfo)
    {
        this.fsManager = fsManager;
        this.permManager = permManager;
        this.userInfo = userInfo;
    }

    public override string Name => "cat";
    public override string Description => "Opens a text file, reads all the text in the file, and then closes the file.";
    public override string Usage => "Usage: cat <file name>\n" +
                                    "Description: Opens a text file, reads all the text in the file, and then closes the file.\n" +
                                    "Available Arguments:\n" +
                                    "  -h: help";
    public override IEnumerable<string> Parameters => new[] { "-h" };
    public override void Execute(string[] args)
    {
        if (args.Length < 2 || args[1] == "-h")
        {
            Console.WriteLine(Usage);
            return;
        }

        if (!File.Exists(fsManager.getFullPath(args[1])) && !Directory.Exists(fsManager.getFullPath(args[1])))
        {
            Console.WriteLine("File does not exist");
            return;
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
    }
}

public class RmFileCommand : AbstractCommand
{
    private FileSystemManager fsManager;
    private FilePermissions permManager;
    private UserLogin userInfo;

    public RmFileCommand(FileSystemManager fsManager, FilePermissions permManager, UserLogin userInfo)
    {
        this.fsManager = fsManager;
        this.permManager = permManager;
        this.userInfo = userInfo;
    }

    public override string Name => "rmf";
    public override string Description => "Deletes the specified file.";
    public override string Usage => "Usage: rmf <file name>\n" +
                                    "Description: Deletes the specified file.\n" +
                                    "Available Arguments:\n" +
                                    "  -h: help";
    public override IEnumerable<string> Parameters => new[] { "-h" };
    public override void Execute(string[] args)
    {
        if (args.Length < 2 || args[1] == "-h")
        {
            Console.WriteLine(Usage);
            return;
        }
        if (!File.Exists(fsManager.getFullPath(args[1])) && !Directory.Exists(fsManager.getFullPath(args[1])))
        {
            Console.WriteLine("File does not exist");
            return;
        }


        if (!permManager.IsWriter(fsManager.getFullPath(args[1]), userInfo.GetId()) && !userInfo.IsAdmin())
        {
            Console.WriteLine("You do not have permissions to delete this file.");
            return;
        }
        
        permManager.deletePath(fsManager.getFullPath(args[1]));
        fsManager.deleteFile(args[1]);
        permManager.SavePermissions();
    }
}

public class RmDirCommand : AbstractCommand
{
    private FileSystemManager fsManager;
    private FilePermissions permManager;
    private UserLogin userInfo;

    public RmDirCommand(FileSystemManager fsManager, FilePermissions permManager, UserLogin userInfo)
    {
        this.fsManager = fsManager;
        this.permManager = permManager;
        this.userInfo = userInfo;
    }

    public override string Name => "rmd";
    public override string Description => "Deletes the specified directory.";
    public override string Usage => "Usage: rmd <directory name>\n" +
                                    "Description: Deletes the specified directory.\n" +
                                    "Available Arguments:\n" +
                                    "  -h: help";
    public override IEnumerable<string> Parameters => new[] { "-h" };
    public override void Execute(string[] args)
    {
        if (args.Length < 2 || args[1] == "-h")
        {
            Console.WriteLine(Usage);
            return;
        }
        if (!File.Exists(fsManager.getFullPath(args[1])) && !Directory.Exists(fsManager.getFullPath(args[1])))
        {
            Console.WriteLine("File does not exist");
            return;
        }

        if (!permManager.IsWriter(fsManager.getFullPath(args[1]), userInfo.GetId(), false) && !userInfo.IsAdmin())
        {
            Console.WriteLine("You do not have permissions to delete this directory.");
            return;
        }

        permManager.deletePath(fsManager.getFullPath(args[1]));
        fsManager.deleteDir(args[1]);
        permManager.SavePermissions();
    }
}

public class RenameCommand : AbstractCommand
{
    private FileSystemManager fsManager;
    private FilePermissions permManager;
    private UserLogin userInfo;

    public RenameCommand(FileSystemManager fsManager, FilePermissions permManager, UserLogin userInfo)
    {
        this.fsManager = fsManager;
        this.permManager = permManager;
        this.userInfo = userInfo;
    }

    public override string Name => "rename";
    public override string Description => "Renames the selected directory or file.";
    public override string Usage => "Usage: rename <directory or file> <new name>\n" +
                                    "Description: Renames the selected directory or file.\n" +
                                    "Available Arguments:\n" +
                                    "  -h: help";
    public override IEnumerable<string> Parameters => new[] { "-h" };
    public override void Execute(string[] args)
    {
        if (args.Length < 3 || args[1] == "-h")
        {
            Console.WriteLine(Usage);
            return;
        }
        if (!File.Exists(fsManager.getFullPath(args[1])) && !Directory.Exists(fsManager.getFullPath(args[1])))
        {
            Console.WriteLine("File does not exist");
            return;
        }
        if (!permManager.IsWriter(fsManager.getFullPath(args[1]),userInfo.GetId()) && !userInfo.IsAdmin())
        {
            Console.WriteLine("You do not have permissions to rename this file.");
            return;
        }

        string oldpath = fsManager.getFullPath(args[1]);
        string newpath = fsManager.renameFileOrDir(args[1], args[2]);

        if ( newpath != null)
        {
            permManager.switchPermissionPath(oldpath, newpath);
            permManager.SavePermissions();
        }
        else
        {
            Console.WriteLine("Invalid new path");
        }
    }
}

public class CdCommand : AbstractCommand
{
    private FileSystemManager fsManager;
    private FilePermissions permManager;
    private UserLogin userInfo;

    public CdCommand(FileSystemManager fsManager, FilePermissions permManager, UserLogin userInfo)
    {
        this.fsManager = fsManager;
        this.permManager = permManager;
        this.userInfo = userInfo;
    }

    public override string Name => "cd";
    public override string Description => "Changes your current directory to the specified one.";
    public override string Usage => "Usage: cd <directory path>\n" +
                                    "Description: Changes your current directory to the specified one.\n" +
                                    "Available Arguments:\n" +
                                    "  -h: help";

    public override IEnumerable<string> Parameters => new[] { "-h" };

    public override void Execute(string[] args)
    {
        if (args.Length < 2 || args[1] == "-h")
        {
            Console.WriteLine(Usage);
            return;
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
            return;
        }


        if (permManager.IsReader(fsManager.getFullPath(args[1]), userInfo.GetId()) || userInfo.IsAdmin())
        {
            fsManager.changeDirectory(args[1]);
        }
        else
        {
            Console.WriteLine("You do not have permission to view this directory.");
        }
    }
}

public class PwdCommand : AbstractCommand
{
    private FileSystemManager fsManager;

    public PwdCommand(FileSystemManager fsManager)
    {
        this.fsManager = fsManager;
    }

    public override string Name => "pwd";
    public override string Description => "Displays the current working directory.";
    public override string Usage => "Usage: pwd\nDescription: Shows your current directory.";

    public override IEnumerable<string> Parameters => new[] { "-h" };

    public override void Execute(string[] args)
    {
        Console.WriteLine("You are here: " + fsManager.getCurrentDir());
    }
}


