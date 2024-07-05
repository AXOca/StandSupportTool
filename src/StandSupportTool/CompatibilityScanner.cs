using System;
using System.Diagnostics;
using System.Windows;
using Microsoft.Win32;

namespace StandSupportTool
{
    internal class CompatibilityScanner
    {
        private string gameExe = "GTA5.exe";
        private string registryPath = Base64.Decode("U29mdHdhcmVcTWljcm9zb2Z0XFdpbmRvd3MgTlRcQ3VycmVudFZlcnNpb25cQXBwQ29tcGF0RmxhZ3NcTGF5ZXJz"); // @"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers";
        private string[] compatibilityFlags = { "WIN8RTM", "WIN7RTM", "VISTASP2", "VISTASP1", "VISTARTM" };

        public bool IsGameRunningInCompatibilityMode()
        {
            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(registryPath))
                {
                    if (key != null)
                    {
                        foreach (string valueName in key.GetValueNames())
                        {
                            if (valueName.EndsWith(gameExe, StringComparison.OrdinalIgnoreCase))
                            {
                                string? compatibilitySettings = key.GetValue(valueName) as string;
                                if (!string.IsNullOrEmpty(compatibilitySettings))
                                {
                                    foreach (var flag in compatibilityFlags)
                                    {
                                        if (compatibilitySettings.Contains(flag))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Failed to open registry key to get compatibility mode.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while checking compatibility mode: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            // Seems like nothing has been flagged
            return false;
        }
    }
}
