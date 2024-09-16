using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

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
            this.Title = $"Stand Support Tool (Version: {GetVersion()})";
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
            DwmSetWindowAttribute(
                window,
                attr,
                ref useImmersiveDarkMode,
                Marshal.SizeOf(typeof(bool))
            );
        }

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(
            IntPtr hwnd,
            DWMWINDOWATTRIBUTE attribute,
            ref bool pvAttribute,
            int cbAttribute
        );

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

        private string GetVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            // Fallback to 1.0 when we don't know the version we're running on!
            return version != null ? $"{version.Major}.{version.Minor}" : "1.0";
        }

        // Initialize the update manager
        private void InitializeUpdateManager()
        {
            string currentVersion = GetVersion();
            //Trace.WriteLine(currentVersion);
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

        private void CopyLogToClipboard_Click(object sender, RoutedEventArgs e) =>
            logManager.CopyLogToClipboard();

        private void CopyProfileToClipboard_Click(object sender, RoutedEventArgs e)
        {
            profileManager.CopyProfileToClipboard(profileManager.GetActiveProfile());
        }

        private void SetActivationKey_Click(object sender, RoutedEventArgs e)
        {
            activationManager.SetActivationKey(this);
            // Updates the key back when the user saved it
            ActivationKeyText.Text = activationManager
                .ReadActivationKey()
                .Replace("Stand-Activate-", "");
        }

        private void HotkeyButton_Click(object sender, RoutedEventArgs e)
        {
            HotkeysTable hotkeysTable = new HotkeysTable();
            hotkeysTable.Owner = this;
            hotkeysTable.ShowDialog();
        }

        private void OpenYouTubeLink_Click(object sender, RoutedEventArgs e) =>
            YouTubeLinkOpener.OpenYouTubeLink();

        private void AddStandToExclusionsV2_Click(object sender, RoutedEventArgs e) =>
            ExclusionManager.AddDefenderExclusion();

        private async void Launchpad_Click(object sender, RoutedEventArgs e)
        {
            AntivirusInfo avInfo = new AntivirusInfo();

            if (avInfo.Is3rdPartyInstalled())
            {
                MessageBox.Show(
                    "Third-party antivirus software detected\nThis could cause issues when downloading the Launchpad.",
                    "3rd Party Antivirus Detected",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }

            await LaunchpadManager.PerformTest();
        }

        private void DisplayAntivirusInfo_Click(object sender, RoutedEventArgs e)
        {
            AvChecker avChecker = new AvChecker();
            avChecker.Owner = this;
            avChecker.ShowDialog();
        }

        private async void Diagnostics_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await Diagnostics.DiagnosticsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred while trying to run Diagnostics: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }
}
