using System;
using System.Collections.Generic;
using System.Text;

namespace TrappistOS
{
    public abstract class ProgramClass
    {
        public string PID { get; private set; }

        public string Identifier { get; set; }
        public int MemoryStartIndex { get; private set; }

        public abstract void Run();
    }
}
