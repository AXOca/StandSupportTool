using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StandSupportTool
{
    public partial class MainWindow : Window
    {
        // Managers
        private static readonly ResetManager resetManager = new ResetManager();
        private static readonly ProtocolManager protocolManager = new ProtocolManager();
        private static readonly LogManager logManager = new LogManager();
        private static readonly ProfileManager profileManager = new ProfileManager();
        private static readonly ActivationManager activationManager = new ActivationManager();
        private static readonly HotkeyManager hotkeyManager = new HotkeyManager();
        private static readonly ClearHotkeysManager clearHotkeysManager = new ClearHotkeysManager();
        private static UpdateManager? updateManager;
        private static readonly DashboardLinkOpener dashboardLinkOpener = new DashboardLinkOpener();
        private static readonly CacheManager cacheManager = new CacheManager();

        public MainWindow()
        {
            InitializeComponent();
            InitializeActivationKeyText();
            InitializeProtocol();
            InitializeUpdateManager();

            // Check for updates asynchronously
            this.Loaded += async (s, e) => await CheckForUpdatesAsync();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var window = new WindowInteropHelper(this).Handle;
            var attr = DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE;
            var useImmersiveDarkMode = true;
            DwmSetWindowAttribute(window, attr, ref useImmersiveDarkMode, Marshal.SizeOf(typeof(bool)));
        }

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE attribute, ref bool pvAttribute, int cbAttribute);

        public enum DWMWINDOWATTRIBUTE
        {
            DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
        }

        // Initialize the activation key text box
        private void InitializeActivationKeyText()
        {
            ActivationKeyText.Text = activationManager
                .ReadActivationKey()
                .Replace("Stand-Activate-", "");
        }

        // Initialize protocol combo box
        private void InitializeProtocol()
        {
            string? currentProtocol = protocolManager.getProtocol();

            foreach (ComboBoxItem item in ProtocolComboBox.Items)
            {
                if (item.Content.ToString() == currentProtocol)
                {
                    item.IsSelected = true;
                    break;
                }
            }
        }

        // Extract the version number from the window title
        private string ExtractVersionFromTitle(string title)
        {
            string[] parts = title.Split(new[] { "Version: " }, StringSplitOptions.None);
            return parts.Length > 1 ? parts[1].TrimEnd(')') : "0.0"; // Default version if not found
        }

        // Initialize the update manager
        private void InitializeUpdateManager()
        {
            string currentVersion = ExtractVersionFromTitle(this.Title);
            string? executablePath = Process.GetCurrentProcess().MainModule?.FileName;

            if (executablePath == null)
            {
                throw new InvalidOperationException("Executable path cannot be null");
            }

            updateManager = new UpdateManager(currentVersion, executablePath);
        }

        // Check for updates asynchronously
        private async Task CheckForUpdatesAsync()
        {
            if (updateManager != null)
            {
                bool isUpdateAvailable = await updateManager.CheckForUpdates();
                if (isUpdateAvailable)
                {
                    UpdateButton.Visibility = Visibility.Visible;
                }
            }
        }

        // Event handler for the update button click
        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (updateManager != null)
            {
                await updateManager.DownloadUpdate();
            }
        }

        // Event handlers
        private void FullReset_Click(object sender, RoutedEventArgs e) => resetManager.FullReset();

        private async void ClearCache_Click(object sender, RoutedEventArgs e)
        {
            await cacheManager.ClearCache();
        }

        private void SwitchProtocol_Click(object sender, RoutedEventArgs e)
        {
            protocolManager.writeProtocol();
            InitializeProtocol();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                protocolManager.setProtocol(selectedItem.Content.ToString() ?? string.Empty);
            }
        }

        private void CopyLogToClipboard_Click(object sender, RoutedEventArgs e) => logManager.CopyLogToClipboard();

        private void CopyProfileToClipboard_Click(object sender, RoutedEventArgs e)
        {
            profileManager.CopyProfileToClipboard(profileManager.GetActiveProfile());
        }

        private void SetActivationKey_Click(object sender, RoutedEventArgs e)
        {
            activationManager.SetActivationKey(ActivationKeyText);
        }

        private void HotkeyButton_Click(object sender, RoutedEventArgs e)
        {
            HotkeysTable hotkeysTable = new HotkeysTable();
            hotkeysTable.Show();
        }

        private void OpenYouTubeLink_Click(object sender, RoutedEventArgs e) => YouTubeLinkOpener.OpenYouTubeLink();

        private void AddStandToExclusionsV2_Click(object sender, RoutedEventArgs e) => PowerShellExecutor.ExecuteAddMpPreference();

        private async void Launchpad_Click(object sender, RoutedEventArgs e)
        {
            await LaunchpadManager.PerformTest();
        }

        private void DisplayAntivirusInfo_Click(object sender, RoutedEventArgs e)
        {
            AvChecker avChecker = new AvChecker();
            avChecker.Show();
        }

        private async void Diagnostics_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await Diagnostics.DiagnosticsAsync();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"An error occurred while trying to run Diagnostics: {ex.Message}");
            }
        }
    }
}
