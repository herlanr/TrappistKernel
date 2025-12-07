using System;
using System.Collections.Generic;
using System.Linq;
using TrappistOS;

public class HelpCommand : AbstractCommand
{
    CommandRegistry registry;
    public HelpCommand(CommandRegistry registry){ this.registry = registry; }
    public override string Name => "help";
    public override string Description => "Description: Shows available commands and their usage.";
    public override string Usage => "Usage: help\n";
    public override IEnumerable<string> Parameters => new[] { "-h" };

    public override void Execute(string[] args)
    {
        List<string> commandlist = registry.GetAllCommandNames().ToList();
        int currentPage = 0;
        int maxPage = (int)Math.Ceiling((double)(commandlist.Count/(double)3));
        for (int command = 0; command < commandlist.Count; command++)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(registry.Get(commandlist[command]).Usage);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(registry.Get(commandlist[command]).Description);
            Console.WriteLine();

            if ((command+1) % 3 == 0)
            {
                currentPage++;
                Console.WriteLine("Page " + currentPage.ToString() + " out of " + maxPage.ToString() + " Continue with enter, exit with esc");
                if (!WaitForResponse()) { return; } //wait for enter or escape
                Console.WriteLine();
            }
        }

        currentPage++;
        Console.WriteLine("Page " + currentPage.ToString() + " out of " + maxPage.ToString() + " Continue with enter, exit with esc");
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
