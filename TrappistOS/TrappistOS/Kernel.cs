using Cosmos.System.ScanMaps;
using MIV;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using Sys = Cosmos.System;

namespace TrappistOS
{
    public class Kernel : Sys.Kernel
    {
        private static Sys.FileSystem.CosmosVFS FS;
        public static string file;

        protected override void BeforeRun() {
            FS = new Sys.FileSystem.CosmosVFS(); Sys.FileSystem.VFS.VFSManager.RegisterVFS(FS); FS.Initialize(true);
            Console.Clear();
            Console.WriteLine("TrappistOS booted successfully. Welcome.");
            Sys.KeyboardManager.SetKeyLayout(new DE_Standard());
        }

        protected override void Run() {
            Console.Write("Input: ");
            var input = Console.ReadLine();

            if (input == "miv") {
                MIV.MIV.StartMIV();
            }
        }
    }
}