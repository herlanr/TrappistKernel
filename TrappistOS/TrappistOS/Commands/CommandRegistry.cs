
using System.Collections.Generic;

public class CommandRegistry
{
    private Dictionary<string, AbstractCommand> commands = new Dictionary<string, AbstractCommand>();

    public void Register(AbstractCommand cmd)
    {
        commands[cmd.Name] = cmd;
    }

    public AbstractCommand Get(string name)
    {
        return commands.ContainsKey(name) ? commands[name] : null;
    }

    public IEnumerable<string> GetAllCommandNames()
    {
        return commands.Keys;
    }
}