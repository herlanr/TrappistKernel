using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;

namespace TrappistOS
{
    public class Kernel : Sys.Kernel
    {
        UserLogin userInfo;
        protected override void BeforeRun()
        {
            userInfo = new UserLogin();
            Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back.");
            userInfo.VisitorLogin();   
        }

        protected override void Run()
        {
            Console.Write($"{userInfo.currentUser.username}:Input: ");
            var input = Console.ReadLine();
            Console.Write("Text typed: ");
            Console.WriteLine(input);
        }
    }
}
