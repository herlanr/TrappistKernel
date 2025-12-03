
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Cosmos.HAL.Drivers.PCI.Video.VMWareSVGAII;

namespace TrappistOS
{
    public class CommandHistory
    {
        private List<string> _commandList;

        public CommandHistory(List<string> commandList)
        {
            _commandList = commandList;
        }


        private static readonly List<string> _history = new List<string>();
        private int _historyIndex = -1;

        private readonly StringBuilder _buffer = new StringBuilder();
        private int _cursor = 0;

        private string _prompt = "";
        private int _prevBufferLen = 0;

        private List<string> _matches = new List<string>();
        private string _lastSearchTerm = ""; // The original text user typed ("he")
        private int _tabIndex = 0;

        public int MaxHistory { get; set; } = 200;

        public string ReadLine(string userName, string currentDir)
        {
            _prompt = $"{userName}>{currentDir}> ";
            _buffer.Clear();
            _cursor = 0;
            _prevBufferLen = 0;
            _historyIndex = _history.Count;

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write(_prompt);
            Console.ResetColor();

            while (true)
            {
                var key = Console.ReadKey(true);

                if ((key.Modifiers & ConsoleModifiers.Control) != 0)
                {
                    if (key.Key == ConsoleKey.C)
                    {
                        Console.WriteLine();
                        _buffer.Clear();
                        _cursor = 0;

                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.Write(_prompt);
                        Console.ResetColor();
                    }
                }

                switch (key.Key)
                {
                    case ConsoleKey.Enter:
                        Console.WriteLine();
                        var line = _buffer.ToString();
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            _history.Add(line);
                            if (_history.Count > MaxHistory)
                                _history.RemoveAt(0);
                        }
                        _historyIndex = _history.Count;
                        _prevBufferLen = 0;
                        return line;

                    case ConsoleKey.Backspace:
                        if (_cursor > 0)
                        {
                            _buffer.Remove(_cursor - 1, 1);
                            _cursor--;
                            Redraw();
                        }
                        break;

                    case ConsoleKey.Delete:
                        if (_cursor < _buffer.Length)
                        {
                            _buffer.Remove(_cursor, 1);
                            Redraw();
                        }
                        break;

                    case ConsoleKey.LeftArrow:
                        if (_cursor > 0)
                        {
                            _cursor--;
                            Redraw();
                        }
                        break;

                    case ConsoleKey.RightArrow:
                        if (_cursor < _buffer.Length)
                        {
                            _cursor++;
                            Redraw();
                        }
                        break;

                    case ConsoleKey.UpArrow:
                        if (_history.Count > 0)
                        {
                            if (_historyIndex > 0)
                                _historyIndex--;
                            else
                                _historyIndex = 0;
                            ReplaceInput(_history[_historyIndex]);
                        }
                        break;

                    case ConsoleKey.DownArrow:
                        if (_history.Count > 0)
                        {
                            if (_historyIndex < _history.Count - 1)
                            {
                                _historyIndex++;
                                ReplaceInput(_history[_historyIndex]);
                            }
                            else
                            {
                                _historyIndex = _history.Count;
                                ReplaceInput(string.Empty);
                            }
                        }
                        break;
                    case ConsoleKey.Tab:
                        try 
                        {
                            AutoComplete(_buffer);
                        }
                        catch (Exception ex)
                        {
                            ReplaceInput($"\nAutocomplete error: {ex.Message}");
                        }
                        break;
                    default:
                        if (!char.IsControl(key.KeyChar))
                        {
                            _buffer.Insert(_cursor, key.KeyChar);
                            _cursor++;
                            Redraw();
                        }
                        break;
                }
            }
        }

        private void ReplaceInput(string text)
        {
            _buffer.Clear();
            _buffer.Append(text);
            _cursor = _buffer.Length;
            Redraw();
        }

        private void Redraw()
        {
            string text = _buffer.ToString();
            int lineTop = Console.CursorTop;  // aktuelle Zeile merken

            // 1) an den Anfang der Zeile springen
            Console.SetCursorPosition(0, lineTop);

            // 2) Prompt + Text neu ausgeben
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write(_prompt);
            Console.ResetColor();
            Console.Write(text);

            // 3) Rest der alten Zeile mit Spaces überschreiben (falls neue kürzer)
            int totalLen = _prompt.Length + text.Length;
            int pad = Math.Max(0, _prevBufferLen - totalLen);
            if (pad > 0)
            {
                Console.Write(new string(' ', pad));
            }

            // 4) Cursor auf die logische Position setzen
            int cursorAbsolute = _prompt.Length + _cursor;
            Console.SetCursorPosition(cursorAbsolute, lineTop);

            _prevBufferLen = totalLen;
        }

        private void AutoComplete(StringBuilder currentBuffer)
        {
            string input = currentBuffer.ToString();

            // 1. Sanity Check: Don't autocomplete empty strings
            if (string.IsNullOrWhiteSpace(input)) return;

            // 2. Check if we are continuing a previous cycle
            // We are cycling if matches exist AND the current buffer equals the last thing we suggested
            bool _isCycling = _matches.Count > 0 &&
                             _tabIndex > 0 &&
                             input == _matches[_tabIndex - 1];

            // 3. If NOT cycling, this is a new search
            if (!_isCycling)
            {
                _lastSearchTerm = input;

                // Fetch commands from your registry
                var allCommands = _commandList;
                //var allCommands = new List<string> { "help", "hello", "shutdown", "reboot", "clear" };
                
                if (allCommands.Count == 0)
                {
                    Console.Write("Command-List is empty.");
                    return;
                }

                // Find matches (Case insensitive)
                _matches = allCommands
                    .Where(c => c.StartsWith(_lastSearchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                _tabIndex = 0;
            }

            // 4. Handle No Matches
            if (_matches.Count == 0) return;

            // 5. Pick the Suggestion
            string suggestion = _matches[_tabIndex];

            // 6. VISUAL UPDATE: Update the Console
            ReplaceInput(suggestion);

            // 7. INTERNAL UPDATE: Update the StringBuilder (Crucial!)
            currentBuffer.Clear();
            currentBuffer.Append(suggestion);

            // 8. Advance Index for next time (Loop back to 0 if at end)
            _tabIndex = (_tabIndex + 1) % _matches.Count;
        }
    }
}