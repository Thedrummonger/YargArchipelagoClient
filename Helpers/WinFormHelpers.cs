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
    }
}
