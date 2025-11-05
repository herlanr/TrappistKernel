using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TrappistOS
{
    public class CommandHistory
    {
        // statisch, damit die Historie erhalten bleibt, auch wenn eine neue Instanz erstellt wird
        private static readonly List<string> _history = new List<string>();
        private int _historyIndex = -1;

        private readonly StringBuilder _buffer = new StringBuilder();
        private int _cursor = 0;

        private string _prompt = "";
        private int _prevBufferLen = 0;

        public int MaxHistory { get; set; } = 200;

        public string ReadLine(string currentDir)
        {
            _prompt = currentDir + "> ";
            _buffer.Clear();
            _cursor = 0;
            _prevBufferLen = 0;
            _historyIndex = _history.Count;

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write(_prompt);
            Console.ForegroundColor = ConsoleColor.White;

            while (true)
            {
                var key = Console.ReadKey(true);

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
            // Volltext neu schreiben (Prompt + Buffer)
            string text = _buffer.ToString();
            string full = _prompt + text;

            // 1) an Zeilenanfang springen, alles neu ausgeben
            Console.Write("\r" + full);

            // 2) falls die neue Zeile kürzer ist: den "Rest" mit Spaces überschreiben
            int leftover = Math.Max(0, _prevBufferLen - full.Length);
            if (leftover > 0)
                Console.Write(new string(' ', leftover));

            // 3) Cursor an die logische Position KORRIGIEREN:
            //    Wenn der Cursor NICHT am Ende steht, laufen wir mit Backspace zurück.
            int cursorAbsolute = _prompt.Length + _cursor;          // Zielposition relativ zum Zeilenanfang
            int currentAbsolute = full.Length;                      // wir stehen aktuell am Ende
            int back = Math.Max(0, currentAbsolute - cursorAbsolute);
            if (back > 0)
                Console.Write(new string('\b', back));

            _prevBufferLen = full.Length;
        }
    }
}