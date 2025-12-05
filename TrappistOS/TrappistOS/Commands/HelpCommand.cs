using System;
using System.Collections.Generic;
using TrappistOS;

public class HelpCommand : AbstractCommand
{
    public HelpCommand(){}

    public override string Name => "help";
    public override string Description => "Displays help information for all commands.";
    public override string Usage => "Usage: help\nDescription: Shows available commands and their usage.";
    public override IEnumerable<string> Parameters => new[] { "-h" };
    public override void Execute(string[] args)
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

    static internal bool WaitForResponse()
    {
        while (true)
        {
            var key = System.Console.ReadKey(true);

            // Strg+C = Abort
            if ((key.Modifiers & ConsoleModifiers.Control) != 0 &&
                key.Key == ConsoleKey.C)
            {
                Kernel.AbortRequest = true;
                return false;
            }

            if (key.Key == ConsoleKey.Enter)
                return true;
            if (key.Key == ConsoleKey.Escape)
                return false;
        }
    }
}
