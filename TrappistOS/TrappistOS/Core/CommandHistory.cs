
using System;
using System.Collections.Generic;
using System.Text;

namespace TrappistOS
{
    public class CommandHistory
    {
        private static readonly List<string> _history = new List<string>();
        private int _historyIndex = -1;

        private readonly StringBuilder _buffer = new StringBuilder();
        private int _cursor = 0;

        private string _prompt = "";
        private int _prevBufferLen = 0;

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

                switch (key.Key)
                {
                    case ConsoleKey.C:
                        if ((key.Modifiers & ConsoleModifiers.Control) != 0)
                        {
                            Console.WriteLine("^C");
                            Kernel.AbortRequest = true;
                            _buffer.Clear();
                            _cursor = 0;
                            return string.Empty;
                        }
                        goto default;

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
    }
}