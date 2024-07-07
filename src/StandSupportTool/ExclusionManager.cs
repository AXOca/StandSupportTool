using System;
using System.Diagnostics;
using System.Management;
using System.Security.Principal;
using System.Windows;

namespace StandSupportTool
{
    public class ExclusionManager
    {
        public static void AddDefenderExclusion()
        {
            AntivirusInfo antivirusInfo = new AntivirusInfo();

            if (!antivirusInfo.IsDefenderInstalled())
            {
                MessageBox.Show("Seems like Defender is not installed in your system, this is not supported.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "This feature will add an exclusion for Stand's BIN folder on Windows Defender.\n\nFor this, we need admin permissions.\n\nDo you want to continue?",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string exclusionPath = System.IO.Path.Combine(appDataPath, "Stand", "Bin");

            RestartAsAdmin($"--add-exclusion \"{exclusionPath}\"");
        }

        private bool IsUserAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private static void RestartAsAdmin(string args = "")
        {
            string? exeName = Environment.ProcessPath;
            if (exeName != null) 
            {
                var startInfo = new ProcessStartInfo(exeName)
                {
                    UseShellExecute = true,
                    Verb = "runas",
                    Arguments = args
                };

                try
                {
                    Process.Start(startInfo);
                }
                catch (System.ComponentModel.Win32Exception)
                {
                    MessageBox.Show("This application requires administrator privileges to run. Please restart the application as an administrator.", "Administrator Privileges Required", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

        }
        public void AddExclusion(string exclusionPath)
        {
            try
            {
                if (IsExclusionAdded(exclusionPath))
                {
                    MessageBox.Show("Exclusion already exists.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    ManagementScope scope = new ManagementScope(@"\\.\root\Microsoft\Windows\Defender");
                    scope.Connect();

                    ManagementClass instance = new ManagementClass(scope, new ManagementPath("MSFT_MpPreference"), null);

                    ManagementBaseObject inParams = instance.GetMethodParameters("Add");
                    inParams["ExclusionPath"] = new string[] { exclusionPath };

                    instance.InvokeMethod("Add", inParams, null);

                    if (IsExclusionAdded(exclusionPath))
                    {
                        MessageBox.Show("Exclusion added.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to verify exclusion.", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    Environment.Exit(0);
                }
            }
            catch (ManagementException mEx)
            {
                MessageBox.Show($"WMI ManagementException: {mEx.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (UnauthorizedAccessException uEx)
            {
                MessageBox.Show($"Unauthorized Access: {uEx.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // We failed if we reach here
            Environment.Exit(1);
        }

        private bool IsExclusionAdded(string exclusionPath)
        {
            try
            {
                ManagementScope scope = new ManagementScope(@"\\.\root\Microsoft\Windows\Defender");
                scope.Connect();

                ObjectQuery query = new ObjectQuery("SELECT * FROM MSFT_MpPreference");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
                ManagementObjectCollection queryCollection = searcher.Get();

                foreach (ManagementObject m in queryCollection)
                {
                    string[] currentExclusions = (string[])m["ExclusionPath"];
                    if (currentExclusions != null)
                    {
                        foreach (string path in currentExclusions)
                        {
                            if (string.Equals(path, exclusionPath, StringComparison.OrdinalIgnoreCase))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected error when checking exclusion: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return false;
        }
    }
}
