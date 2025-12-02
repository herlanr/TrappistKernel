
using System.Collections.Generic;

public abstract class AbstractCommand
{
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract string Usage { get; }
    public abstract IEnumerable<string> Parameters { get; }
    public abstract void Execute(string[] args);
}
