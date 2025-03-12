using System.ComponentModel;

namespace ArchipelagoPowerTools.Helpers
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


    }
}
