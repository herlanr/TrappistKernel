using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;

namespace TrappistOS
{
    public class Kernel : Sys.Kernel
    {
        // each program needs one space here
        ProgramClass[] ProgramMemory = new ProgramClass[6];
        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back.");
        }

        protected override void Run()
        {
            Console.Write("Input: ");
            var input = Console.ReadLine();
            Console.Write("Text typed: ");
            Console.WriteLine(input);
        }
    }
}
