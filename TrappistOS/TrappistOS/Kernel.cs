using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;

namespace TrappistOS
{
    public class Kernel : Sys.Kernel
    {
        UserLogin userInfo;
        ProgramClass[] ProgramMemory = new ProgramClass[6];
        protected override void BeforeRun()
        {
            userInfo = new UserLogin();
            ProgramMemory[0] = userInfo;
            Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back.");
            userInfo.BeforeRun();
        }

        protected override void Run()
        {
            Console.Write($"{userInfo.get_name}:Input: ");
            var input = Console.ReadLine();
            userInfo.Run();
            Console.Write("Text typed: ");
            Console.WriteLine(input);
        }
    }
}
