using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YargArchipelagoCLI
{
    internal class ConsoleHelper
    {
        public static string ReadLineWithDefault(string prompt, string? defaultValue, bool password = false)
        {
            Console.WriteLine($"{prompt}:");

            var input = new StringBuilder(defaultValue ?? "");
            Console.Write(password ? new string('*', input.Length) : input.ToString());
            int cursorPos = input.Length;

            while (true)
            {
                var key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return input.ToString();
                }
                else if (key.Key == ConsoleKey.Backspace && cursorPos > 0)
                {
                    input.Remove(cursorPos - 1, 1);
                    cursorPos--;
                    Console.Write("\b \b");
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    input.Insert(cursorPos, key.KeyChar);
                    cursorPos++;
                    Console.Write(password ? '*' : key.KeyChar);
                }
            }
        }

    }
}
