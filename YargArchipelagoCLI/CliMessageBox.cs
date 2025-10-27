using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YargArchipelagoCore.Helpers;
using static YargArchipelagoCore.Helpers.MultiplatformHelpers;

namespace YargArchipelagoCLI
{
    internal class CliMessageBox
    {
        public static void ApplyMessageBoxAction()
        {
            MessageBox.ShowAction = (m, t, b, i) =>
            {
                if (!string.IsNullOrWhiteSpace(t)) Console.WriteLine(t);
                Console.WriteLine(m);
                switch (b)
                {
                    case MessageBoxButtons.OK:
                        Console.WriteLine("Press Any Key to continue..");
                        break;
                    case MessageBoxButtons.OKCancel:
                        Console.WriteLine("[O: ok, C: cancel]");
                        break;
                    case MessageBoxButtons.AbortRetryIgnore:
                        Console.WriteLine("[A: abort, R: retry]");
                        break;
                    case MessageBoxButtons.YesNoCancel:
                        Console.WriteLine("[Y: yes, N: no, C: cancel]");
                        break;
                    case MessageBoxButtons.YesNo:
                        Console.WriteLine("[Y: yes, N: no]");
                        break;
                    case MessageBoxButtons.RetryCancel:
                        Console.WriteLine("[R: retry, C: cancel]");
                        break;
                    case MessageBoxButtons.CancelTryContinue:
                        Console.WriteLine("[C: cancel, T: try again, U: continue]");
                        break;
                }
            UserInput:
                var key = Console.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.O when b == MessageBoxButtons.OK || b == MessageBoxButtons.OKCancel:
                        return DialogResult.OK;
                    case ConsoleKey.C when b == MessageBoxButtons.OKCancel || b == MessageBoxButtons.YesNoCancel || b == MessageBoxButtons.RetryCancel || b == MessageBoxButtons.CancelTryContinue:
                        return DialogResult.Cancel;
                    case ConsoleKey.A when b == MessageBoxButtons.AbortRetryIgnore:
                        return DialogResult.Abort;
                    case ConsoleKey.R when b == MessageBoxButtons.AbortRetryIgnore || b == MessageBoxButtons.RetryCancel:
                        return DialogResult.Retry;
                    case ConsoleKey.Y when b == MessageBoxButtons.YesNoCancel || b == MessageBoxButtons.YesNo:
                        return DialogResult.Yes;
                    case ConsoleKey.N when b == MessageBoxButtons.YesNoCancel || b == MessageBoxButtons.YesNo:
                        return DialogResult.No;
                    case ConsoleKey.N when b == MessageBoxButtons.CancelTryContinue:
                        return DialogResult.TryAgain;
                    default:
                        if (b == MessageBoxButtons.OK)
                            return DialogResult.OK;
                        Console.WriteLine("Invalid Key");
                        goto UserInput;
                }
            };
        }
    }
}
