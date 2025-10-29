using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace YargArchipelagoCore.Helpers
{
    public class MultiplatformHelpers
    {
        public sealed record DialogResponse(ConsoleKey Key, string Label, DialogResult Result);
        public enum MessageBoxButtons { OK, OKCancel, AbortRetryIgnore, YesNoCancel, YesNo, RetryCancel, CancelTryContinue }
        public enum DialogResult { None = 0, OK = 1, Cancel = 2, Abort = 3, Retry = 4, Ignore = 5, Yes = 6, No = 7, TryAgain = 10, Continue = 11 }
        public enum MessageBoxIcon
        {
            None = 0, Hand = 16, Question = 32, Exclamation = 48, Asterisk = 64,
            Stop = Hand, Error = Hand, Warning = Exclamation, Information = Asterisk
        }

        public static class MessageBox
        {
            public static Func<string, string?, MessageBoxButtons?, MessageBoxIcon?, DialogResult>? ShowAction;

            public static DialogResult Show(string message, string title = "", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None)
                => ShowAction?.Invoke(message, title, buttons, icon) ?? throw new Exception($"Message Box Handling was not defined");

            public static void ApplyConsoleTemplate() => ShowAction = (m, t, b, i) =>
            {
                var buttonsE = b ?? MessageBoxButtons.OK;

                DialogResponse[] map = buttonsE switch
                {
                    MessageBoxButtons.OK => [
                        new(ConsoleKey.O, "ok", DialogResult.OK)],
                    MessageBoxButtons.OKCancel => [
                        new(ConsoleKey.O, "ok", DialogResult.OK), 
                        new(ConsoleKey.C, "cancel", DialogResult.Cancel)],
                    MessageBoxButtons.AbortRetryIgnore => [
                        new(ConsoleKey.A, "abort", DialogResult.Abort), 
                        new(ConsoleKey.R, "retry", DialogResult.Retry), 
                        new(ConsoleKey.I, "ignore", DialogResult.Ignore)],
                    MessageBoxButtons.YesNoCancel => [
                        new(ConsoleKey.Y, "yes", DialogResult.Yes), 
                        new(ConsoleKey.N, "no", DialogResult.No),
                        new(ConsoleKey.C, "cancel", DialogResult.Cancel)],
                    MessageBoxButtons.YesNo => [
                        new(ConsoleKey.Y, "yes", DialogResult.Yes), 
                        new(ConsoleKey.N, "no", DialogResult.No)],
                    MessageBoxButtons.RetryCancel => [
                        new(ConsoleKey.R, "retry", DialogResult.Retry), 
                        new(ConsoleKey.C, "cancel", DialogResult.Cancel)],
                    MessageBoxButtons.CancelTryContinue => [
                        new(ConsoleKey.C, "cancel", DialogResult.Cancel), 
                        new(ConsoleKey.T, "try again", DialogResult.TryAgain), 
                        new(ConsoleKey.U, "continue", DialogResult.Continue)],
                    _ => []
                };

                if (!string.IsNullOrWhiteSpace(t)) Console.WriteLine(t);
                if (i is not null && i != MessageBoxIcon.None) Console.WriteLine($"[{i}]");
                Console.WriteLine(m);

                if (buttonsE == MessageBoxButtons.OK)
                {
                    Console.WriteLine("Press any key to continue…");
                    Console.ReadKey(true);
                    return DialogResult.OK;
                }
                var hasOk = map.Any(x => x.Result == DialogResult.OK);
                var hasCancel = map.Any(x => x.Result == DialogResult.Cancel);
                Console.WriteLine("[" + string.Join(", ", map.Select(x => $"{x.Key}: {x.Label}")) + "]");
                while (true)
                {
                    var info = Console.ReadKey(true);
                    var key = info.Key;

                    if (key == ConsoleKey.Escape && hasCancel) return DialogResult.Cancel;
                    if (key == ConsoleKey.Enter && hasOk) return DialogResult.OK;

                    if (map.FirstOrDefault(x => x.Key == key) is DialogResponse hit)
                        return hit.Result;

                    Console.WriteLine("Invalid key");
                }
            };
        }
    }
}
