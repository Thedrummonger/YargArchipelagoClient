using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YargArchipelagoCore.Helpers
{
    public class MultiplatformHelpers
    {
        public enum MessageBoxButtons
        {
            OK,
            OKCancel,
            AbortRetryIgnore,
            YesNoCancel,
            YesNo,
            RetryCancel,
            CancelTryContinue,
        }
        public enum DialogResult
        {
            None = 0,
            OK = 1,
            Cancel = 2,
            Abort = 3,
            Retry = 4,
            Ignore = 5,
            Yes = 6,
            No = 7,
            TryAgain = 10,
            Continue = 11
        }
        public enum MessageBoxIcon
        {
            None = 0,
            Hand = 16,
            Question = 32,
            Exclamation = 48,
            Asterisk = 64,
            Stop = Hand,
            Error = Hand,
            Warning = Exclamation,
            Information = Asterisk,
        }
        public static class MessageBox
        {
            public static Func<string, string?, MessageBoxButtons?, MessageBoxIcon?, DialogResult>? ShowAction;
            public static DialogResult Show(string Message, string Title = "", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None)
            {
                if (ShowAction is not null)
                    return ShowAction(Message, Title, buttons, icon);
                Debug.WriteLine(Message);
                return DialogResult.None;
            }
        }
    }
}
