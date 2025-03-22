using System.ComponentModel;

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
    }
}
