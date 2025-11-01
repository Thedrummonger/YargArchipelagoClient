using System.ComponentModel;
using TDMUtils;
using YargArchipelagoClient.Data;
using YargArchipelagoCore.Helpers;

namespace YargArchipelagoClient.Helpers
{
    public static class WinFormHelpers
    {
        public static void RefreshListControl(this ListControl control)
        {
            if (control.DataSource is not IEnumerable<object> originalData) return;
            var selectedItem = control.SelectedIndex;
            control.DataSource = null;
            control.DataSource = originalData.ToArray();
            control.SelectedIndex = selectedItem;
        }
        public static void SafeInvoke(this Control control, Action action)
        {
            if (control.InvokeRequired)
                control.Invoke(action);
            else
                action();
        }
        public static string GetDescription(this Enum value) =>
        value.GetType().GetField(value.ToString())?.GetCustomAttributes(typeof(DescriptionAttribute), false)
             .OfType<DescriptionAttribute>()
             .FirstOrDefault()?.Description ?? value.ToString();

        public class ContainerItem(object? value, string display)
        {
            public object? Value { get; } = value;
            public string Display { get; } = display;
            public override string ToString() => Display;
            public static ContainerItem[] ToContainerList<T>(IEnumerable<T> items, Func<T, string> Display) =>
                [.. items.Select(i => new ContainerItem(i, Display(i)))];
            public static ContainerItem[] ToContainerList<T>(IEnumerable<T> items, Func<T, object> Tags, Func<T, string> Display) =>
                [.. items.Select(i => new ContainerItem(Tags(i), Display(i)))];
        }
        public static T? GetSelectedContainerItem<T>(this ComboBox comboBox)
        {
            if (comboBox.SelectedItem is ContainerItem containerItem && containerItem.Value is T value)
                return value;
            return default;
        }
        public static T? GetSelectedContainerItem<T>(this ListBox comboBox)
        {
            if (comboBox.SelectedItem is ContainerItem containerItem && containerItem.Value is T value)
                return value;
            return default;
        }

        public static void SafeSetValue(this NumericUpDown nud, int Current, int Max = int.MaxValue, int Min = int.MinValue)
        {
            nud.Maximum = int.MaxValue;
            nud.Minimum = int.MinValue;
            if (Current > Max) Current = Max;
            if (Current < Min) Current = Min;
            nud.Value = Current;
            nud.Maximum = Max;
            nud.Minimum = Min;
        }

        public static void AppendString(this RichTextBox rtb, params ColoredString[] coloredStrings)
        {
            rtb.AppendString(() =>
            {
                string rtf = ColoredString.BuildRtf(coloredStrings, rtb.ForeColor);
                rtb.SelectedRtf = rtf;
            });
        }
        public static void AppendString(this RichTextBox rtb, string text, Color? color = null)
        {
            rtb.AppendString(() =>
            {
                if (color is not null)
                    rtb.SelectionColor = color.Value;
                rtb.AppendText(text + Environment.NewLine);
                rtb.SelectionColor = rtb.ForeColor;
            });
        }
        public static void AppendString(this RichTextBox rtb, Action appendAction)
        {
            bool wasReadOnly = rtb.ReadOnly;
            if (wasReadOnly) rtb.ReadOnly = false;

            bool autoScroll = rtb.SelectionStart == rtb.TextLength;

            rtb.SelectionStart = rtb.TextLength;
            appendAction();

            if (autoScroll)
            {
                rtb.SelectionStart = rtb.TextLength;
                rtb.ScrollToCaret();
            }

            if (wasReadOnly) rtb.ReadOnly = true;
        }
    }
    public static class WinFormsMessageBoxTemplate
    {
        public static void Apply() =>
            MultiplatformHelpers.MessageBox.ShowAction = (text, title, buttons, icon) =>
                (MultiplatformHelpers.DialogResult)MessageBox.Show(text, title,
                    (MessageBoxButtons)(buttons ?? MultiplatformHelpers.MessageBoxButtons.OK),
                    (MessageBoxIcon)(icon ?? MultiplatformHelpers.MessageBoxIcon.None));
    }
}
