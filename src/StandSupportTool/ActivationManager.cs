using System;
using System.IO;
using System.Windows;

namespace StandSupportTool
{
    internal class ActivationManager
    {
        private readonly string activationKeyFilePath;

        public ActivationManager()
        {
            // Initialize the path to the activation key file
            string appDataPath = Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData
            );
            string standDir = Path.Combine(appDataPath, "Stand");
            activationKeyFilePath = Path.Combine(standDir, "Activation Key.txt");
        }

        // Read the activation key from the file
        public string ReadActivationKey()
        {
            try
            {
                if (File.Exists(activationKeyFilePath))
                {
                    return File.ReadAllText(activationKeyFilePath).Trim();
                }
                else
                {
                    MessageBox.Show(
                        "Activation Key file does not exist.",
                        "Info",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to read activation key: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return string.Empty;
            }
        }

        // Write the activation key to the file
        public void WriteActivationKey(string activationKey)
        {
            try
            {
                string standDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Stand"
                );

                // Create directory if it doesn't exist
                if (!Directory.Exists(standDir))
                {
                    Directory.CreateDirectory(standDir);
                }

                // Write the activation key to the file
                File.WriteAllText(activationKeyFilePath, activationKey);

                // Show success message
                MessageBox.Show(
                    "Activation key saved successfully.",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                // Show error message
                MessageBox.Show(
                    $"Failed to write activation key: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // Method to show a dialog to set the activation key
        public void SetActivationKey(Window ownerWindow)
        {
            ActivationKey activationKeyWindow = new ActivationKey();
            activationKeyWindow.Owner = ownerWindow;
            activationKeyWindow.ShowDialog();
        }
    }
}
