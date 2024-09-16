using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace StandSupportTool
{
    public partial class AvChecker : BaseWindow
    {
        static AntivirusInfo antivirusInfo = new AntivirusInfo();

        public AvChecker()
        {
            InitializeComponent();
            LoadAntivirusInfo();
        }

        private void LoadAntivirusInfo()
        {
            List<AntivirusInfo> avInfos = antivirusInfo.GetAntivirusInfo();
            avListView.ItemsSource = avInfos;
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string exePath)
            {
                // We must expand the EV since AVs like Defender have that in their PATH
                string expandedPath = Environment.ExpandEnvironmentVariables(exePath);

                if (expandedPath.Contains("MsMpeng.exe")) // This is Michaelsoft Defender, we must run it differently!
                {
                    try
                    {
                        Process.Start(
                            new ProcessStartInfo("windowsdefender://threat")
                            {
                                UseShellExecute = true,
                            }
                        );
                        return;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Failed to start Windows Defender: {ex.Message}",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error
                        );
                    }
                }

                // Not Michaelsoft Defender, run whatever AV it is
                try
                {
                    Process.Start(new ProcessStartInfo(expandedPath) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Failed to start process: {ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            }
        }
    }
}
