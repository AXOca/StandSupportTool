using System;
using System.IO;
using System.Windows;

namespace StandSupportTool
{
    internal class ProfileManager
    {
        private readonly string metaStateFilePath;
        private readonly string profilesDirectoryPath;

        public ProfileManager()
        {
            // Initialize paths
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string standDir = Path.Combine(appDataPath, "Stand");
            metaStateFilePath = Path.Combine(standDir, "Meta State.txt");
            profilesDirectoryPath = Path.Combine(standDir, "Profiles");
        }

        // Get the name of the active profile
        public string GetActiveProfile()
        {
            try
            {
                if (File.Exists(metaStateFilePath))
                {
                    var lines = File.ReadAllLines(metaStateFilePath);
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("Active Profile:"))
                        {
                            return line.Split(':')[1].Trim();
                        }
                    }
                }

                MessageBox.Show("Active profile not found.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get active profile: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return string.Empty;
            }
        }

        // Get the content of a specified profile
        public string GetProfileContent(string profileName)
        {
            try
            {
                string profileFilePath = Path.Combine(profilesDirectoryPath, $"{profileName}.txt");

                if (File.Exists(profileFilePath))
                {
                    return File.ReadAllText(profileFilePath);
                }
                else
                {
                    MessageBox.Show($"Profile {profileName} not found.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to read profile: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return string.Empty;
            }
        }

        // Copy the content of a specified profile to the clipboard
        public void CopyProfileToClipboard(string profileName)
        {
            try
            {
                string profileContent = GetProfileContent(profileName);
                if (!string.IsNullOrEmpty(profileContent))
                {
                    Clipboard.SetText(profileContent);
                    MessageBox.Show($"Profile {profileName} copied to clipboard.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to copy profile to clipboard: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
