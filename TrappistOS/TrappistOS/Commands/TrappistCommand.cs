using System;
using System.Collections.Generic;
using TrappistOS;

public class TrappistCommand : AbstractCommand
{
    public override string Name => "trappist";

    public override string Description => "???";

    public override string Usage => "???";
    public override IEnumerable<string> Parameters => Array.Empty<string>();

    public override void Execute(string[] args)
    {
        Console.WriteLine();
        Console.WriteLine("TRAPPIST-1 Star System");
        Console.WriteLine("----------------------");
        Console.WriteLine("Type: Red dwarf star");
        Console.WriteLine("Distance: ~39 light years from Earth");
        Console.WriteLine();
        Console.WriteLine("Known planets in orbit:");
        Console.WriteLine("  b  c  d  e  f  g  h");
        Console.WriteLine();
        Console.WriteLine("Somewhere out there, another shell might be running TrappistOS.");
        Console.WriteLine();
    }
}
