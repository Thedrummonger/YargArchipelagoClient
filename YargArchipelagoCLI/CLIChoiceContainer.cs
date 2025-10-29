using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YargArchipelagoCLI
{
    public class CLIChoiceData(string display, Action? onSelect = null, Func<bool>? condition = null, object? tag = null)
    {
        public Func<bool> Condition = condition ?? (() => true);
        public Action? OnSelect = onSelect;
        public object? tag = tag;
        public override string ToString() => display;
    }
    class CLIChoiceContainer(object CancelInput, string? Header, bool IsKey)
    {
        public Dictionary<object, CLIChoiceData> Choices = [];
        public bool AreAnyValid() => Choices.Any(x => x.Value.Condition());

        bool InvalidFeedback = true;
        bool HeaderTop = true;
        bool Separators = true;
        string SelectText = $"Select a value..";
        string CancelText = (CancelInput is string CS && CS == string.Empty) ? 
            "Enter nothing to cancel" : 
            $"{(IsKey ? "Press" : "Type")} {CancelInput} to cancel";
        public CLIChoiceContainer ToggleInvalidFeedback(bool Feedback) { InvalidFeedback = Feedback; return this; }
        public CLIChoiceContainer PlaceHeaderAboveValues(bool Above) { HeaderTop = Above; return this; }
        public CLIChoiceContainer ToggleSeparators(bool PrintSeparators) { Separators = PrintSeparators; return this; }
        public CLIChoiceContainer SetSelectText(string text) { SelectText = text; return this; }
        public CLIChoiceContainer SetCancelText(string text){ CancelText = text; return this; }
        public CLIChoiceData? GetChoice()
        {
            if (Choices.ContainsKey(CancelInput))
                throw new Exception($"{CancelInput} was designated aas your Cancel Input, it can not also be a selectable value.");
            if (Header is string ST && HeaderTop)
                Console.WriteLine(ST);
            if (Choices.Count < 1)
                return null;

            if (Separators)
                Console.WriteLine(new string('=', Console.WindowWidth));

            foreach (var i in Choices.Where(x => x.Value.Condition()))
                Console.WriteLine($"{i.Key}: {i.Value}");

            if (Separators)
                Console.WriteLine(new string('=', Console.WindowWidth));

            if (Header is string SB && !HeaderTop)
                Console.WriteLine(SB);

            if (!string.IsNullOrWhiteSpace(SelectText))
                Console.WriteLine(SelectText);
            if (!string.IsNullOrWhiteSpace(CancelText))
                Console.WriteLine(CancelText);

            while (true)
            {
                object? result = IsKey ? Console.ReadKey().Key : Console.ReadLine()??string.Empty;
                if (result.Equals(CancelInput))
                    return null;
                if (Choices.TryGetValue(result, out var Selected) && Selected.Condition())
                    return Selected;
                if (InvalidFeedback)
                    Console.WriteLine($"Invalid Choice '{result?.ToString()}'..");
            }
        }
    }
    class CLITextChoiceContainer(string? Header = null, string CancelInputString = "exit") : CLIChoiceContainer(CancelInputString, Header, false)
    {
        public void AddOption(string key, string display, Action? onSelect = null, Func<bool>? condition = null, object? tag = null) =>
            Choices[key] = new CLIChoiceData(display, onSelect, condition, tag);
    }
    class CLIKeyChoiceContainer(string? Header = null, ConsoleKey CancelInput = ConsoleKey.Escape) : CLIChoiceContainer(CancelInput, Header, true)
    {
        public void AddOption(ConsoleKey key, string display, Action? onSelect = null, Func<bool>? condition = null, object? tag = null) =>
            Choices[key] = new CLIChoiceData(display, onSelect, condition, tag);
    }
}
