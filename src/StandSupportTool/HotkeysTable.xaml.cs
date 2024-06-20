using System.Windows;

namespace StandSupportTool
{
    public partial class HotkeysTable : Window
    {
        private static HotkeyManager hotkeyManager;
        private static ClearHotkeysManager clearHotkeysManager = new ClearHotkeysManager();

        public HotkeysTable()
        {
            InitializeComponent();

            // Do it like this so it updates the data everytime we update the window!
            hotkeyManager = new HotkeyManager();

            DataContext = hotkeyManager;
        }

        private void Hotkeys60_Click(object sender, RoutedEventArgs e)
        {
            hotkeyManager.SmallerKeyboard();
        }

        private void SaveHotkey_Click(object sender, RoutedEventArgs e)
        {
            hotkeyManager.SaveHotkeys();
        }

        private void ClearHotkeys_Click(object sender, RoutedEventArgs e)
        {
            hotkeyManager.Hotkeys.Clear();
            clearHotkeysManager.ClearHotkeys();
        }
    }
}
