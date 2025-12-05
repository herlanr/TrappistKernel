using TrappistOS;
using System;
using Sys = Cosmos.System;
using System.Collections.Generic;

public class FreeSpaceCommand : AbstractCommand
{
    private FileSystemManager fsManager;

    public FreeSpaceCommand(FileSystemManager fsManager)
    {
        this.fsManager = fsManager;
    }

    public override string Name => "freespace";
    public override string Description => "Get available free space.";
    public override string Usage => "Usage: freespace [-h]";
    public override IEnumerable<string> Parameters => new[] { "-h" };

    public override void Execute(string[] args)
    {
        if (args.Length > 1 && args[1] == "-h")
        {
            Console.WriteLine(Usage);
            Console.WriteLine("Description: " + Description);
            return;
        }

        fsManager.showFreeSpace();
    }
}

public class ClearCommand : AbstractCommand
{
    public override string Name => "clear";
    public override string Description => "Clears the console screen.";
    public override string Usage => "Usage: clear\nDescription: Clears the terminal screen.";
    public override IEnumerable<string> Parameters => new[] { "-h" };

    public override void Execute(string[] args)
    {
        Console.Clear();
    }
}

public class ShutdownCommand : AbstractCommand
{
    private FilePermissions permManager;

    public ShutdownCommand(FilePermissions permManager)
    {
        this.permManager = permManager;
    }

    public override string Name => "shutdown";
    public override string Description => "Shuts down the system.";
    public override string Usage => "Usage: shutdown\nDescription: Saves permissions and powers off the system.";
    public override IEnumerable<string> Parameters => new[] { "-h" };

    public override void Execute(string[] args)
    {
        permManager.SavePermissions();
        Sys.Power.Shutdown();
    }
}

public class RebootCommand : AbstractCommand
{
    private FilePermissions permManager;

    public RebootCommand(FilePermissions permManager)
    {
        this.permManager = permManager;
    }

    public override string Name => "reboot";
    public override string Description => "Reboots the system.";
    public override string Usage => "Usage: reboot\nDescription: Saves permissions and restarts the system.";
    public override IEnumerable<string> Parameters => new[] { "-h" };
    public override void Execute(string[] args)
    {
        permManager.SavePermissions();
        Sys.Power.Reboot();
    }
}

public class ForceShutdownCommand : AbstractCommand
{
    public override string Name => "force-shutdown";
    public override string Description => "Forcefully shuts down the system without saving changes.";
    public override string Usage => "Usage: force-shutdown\nDescription: Immediately powers off the system without saving changes.";
    public override IEnumerable<string> Parameters => new[] { "-h" };
    public override void Execute(string[] args)
    {
        Console.WriteLine("Are you sure you want to forcefully shutdown? Not all changes will be saved.\n(y)es/(n)o");
        if (WaitForConfirmation())
        {
            Sys.Power.Shutdown();
        }
    }

    private bool WaitForConfirmation()
    {
        string input = Console.ReadLine()?.Trim().ToLower();
        return input == "y" || input == "yes";
    }
}

public class ForceRebootCommand : AbstractCommand
{
    public override string Name => "force-reboot";
    public override string Description => "Forcefully reboots the system without saving changes.";
    public override string Usage => "Usage: force-reboot\nDescription: Immediately restarts the system without saving changes.";
    public override IEnumerable<string> Parameters => new[] { "-h" };

    public override void Execute(string[] args)
    {
        Console.WriteLine("Are you sure you want to forcefully reboot? Not all changes will be saved.\n(y)es/(n)o");
        if (WaitForConfirmation())
        {
            Sys.Power.Reboot();
        }
    }

    private bool WaitForConfirmation()
    {
        string input = Console.ReadLine()?.Trim().ToLower();
        return input == "y" || input == "yes";
    }
}

public class MivCommand : AbstractCommand
{
    private FileSystemManager fsManager;
    private FilePermissions permManager;
    private UserLogin userInfo;

    public MivCommand(FileSystemManager fsManager, FilePermissions permManager, UserLogin userInfo)
    {
        this.fsManager = fsManager;
        this.permManager = permManager;
        this.userInfo = userInfo;
    }

    public override string Name => "miv";
    public override string Description => "Opens the MIV editor to edit the specified file.";
    public override string Usage =>
        "Usage: miv <file name>\n" +
        "Description: Edit the specified file using MIV editor.\n" +
        "Available Arguments:\n  -h: help";
    public override IEnumerable<string> Parameters => new[] { "-h" };

    public override void Execute(string[] args)
    {
        if (args.Length < 2 || args[1] == "-h")
        {
            Console.WriteLine(Usage);
            return;
        }

        if (args.Length == 2)
        {
            string filePath = fsManager.getFullPath(args[1]);

            if (!permManager.IsWriter(filePath, userInfo.GetId()) && !userInfo.IsAdmin())
            {
                Console.WriteLine("You do not have permission to edit this file");
                return;
            }

            if (MIV.MIV.PrintMivCommands())
            {
                MIV.MIV.StartMIV(filePath);
            }
        }
    }
}
public class SnakeCommand : AbstractCommand
{
    public override string Name => "snake";
    public override string Description => "Starts the Snake game.";
    public override string Usage =>
        "Usage: snake\n" +
        "Description: Play the Snake game.";
    public override IEnumerable<string> Parameters => new[] { "-h" };

    public override void Execute(string[] args)
    {
        Snake snake = new Snake();
        snake.configSnake();

        ConsoleKey x;
        ConsoleKeyInfo keyInfo;

        Console.Clear();
        Console.SetCursorPosition(0, 0);

        while (true)
        {
            while (snake.gameover())
            {
                Console.SetCursorPosition(0, 0);
                snake.changeArray();
                snake.printGame();

                bool endGame = false;

                keyInfo = Console.ReadKey(true);

                if ((keyInfo.Modifiers & ConsoleModifiers.Control) != 0 &&
                    keyInfo.Key == ConsoleKey.C)
                {
                    Kernel.AbortRequest = true;
                    Console.Clear();
                    Console.SetCursorPosition(0, 0);
                    return;
                }

                switch (keyInfo.Key)
                {
                    case ConsoleKey.R:
                        snake.configSnake();
                        break;

                    case ConsoleKey.Escape:
                        endGame = true;
                        break;
                }

                if (endGame)
                {
                    Console.Clear();
                    Console.SetCursorPosition(0, 0);
                    return;
                }
            }

            while (!Console.KeyAvailable && !snake.gameover())
            {
                snake.updateDirections();
                snake.updatePosotion();
                snake.checkIfTouchFood();

                Console.SetCursorPosition(0, 0);
                snake.changeArray();
                snake.printGame();

                snake.delay(10000000);
            }

            keyInfo = Console.ReadKey(true);

            if ((keyInfo.Modifiers & ConsoleModifiers.Control) != 0 &&
                keyInfo.Key == ConsoleKey.C)
            {
                Kernel.AbortRequest = true;
                Console.Clear();
                Console.SetCursorPosition(0, 0);
                return;
            }

            x = keyInfo.Key;

            if (x == ConsoleKey.LeftArrow)
            {
                if (snake.snake[0][1] != 3)
                    snake.commands.Add(new int[2] { 1, 0 });
            }
            else if (x == ConsoleKey.UpArrow)
            {
                if (snake.snake[0][1] != 2)
                    snake.commands.Add(new int[2] { 4, 0 });
            }
            else if (x == ConsoleKey.RightArrow)
            {
                if (snake.snake[0][1] != 1)
                    snake.commands.Add(new int[2] { 3, 0 });
            }
            else if (x == ConsoleKey.DownArrow)
            {
                if (snake.snake[0][1] != 4)
                    snake.commands.Add(new int[2] { 2, 0 });
            }
            else if (x == ConsoleKey.Escape)
            {
                Console.Clear();
                Console.SetCursorPosition(0, 0);
                return;
            }
            else if (x == ConsoleKey.R)
            {
                snake.configSnake();
            }
        }
    }
}
