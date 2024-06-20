using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace StandSupportTool
{
    internal class ActivationManager
    {
        private readonly string activationKeyFilePath;

        public ActivationManager()
        {
            // Initialize the path to the activation key file
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
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
                    MessageBox.Show("Activation Key file does not exist.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to read activation key: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return string.Empty;
            }
        }

        // Write the activation key to the file
        public void WriteActivationKey(string activationKey)
        {
            try
            {
                string standDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Stand");

                // Create directory if it doesn't exist
                if (!Directory.Exists(standDir))
                {
                    Directory.CreateDirectory(standDir);
                }

                // Write the activation key to the file
                File.WriteAllText(activationKeyFilePath, activationKey);

                // Show success message
                MessageBox.Show("Activation key saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // Show error message
                MessageBox.Show($"Failed to write activation key: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Method to show a dialog to set the activation key
        public void SetActivationKey(TextBox activationKeyTextBox)
        {
            // Create and configure the input dialog window
            Window inputDialog = new Window
            {
                Title = "Enter Activation Key",
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };

            StackPanel stackPanel = new StackPanel { Margin = new Thickness(10) };

            // Add components to the dialog window
            stackPanel.Children.Add(new TextBlock { Text = "Enter Activation Key:", Margin = new Thickness(0, 0, 0, 10) });
            TextBox textBox = new TextBox { Width = 250, Margin = new Thickness(0, 0, 0, 10) };
            stackPanel.Children.Add(textBox);

            StackPanel buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };

            // OK button
            Button okButton = new Button { Content = "OK", Width = 75, Margin = new Thickness(0, 0, 10, 0) };
            okButton.Click += (s, args) =>
            {
                inputDialog.DialogResult = true;
                inputDialog.Close();
            };
            buttonPanel.Children.Add(okButton);

            // Cancel button
            Button cancelButton = new Button { Content = "Cancel", Width = 75 };
            cancelButton.Click += (s, args) =>
            {
                inputDialog.DialogResult = false;
                inputDialog.Close();
            };
            buttonPanel.Children.Add(cancelButton);

            stackPanel.Children.Add(buttonPanel);
            inputDialog.Content = stackPanel;

            // Show dialog and set activation key if OK was clicked
            if (inputDialog.ShowDialog() == true)
            {
                string activationKey = textBox.Text;
                WriteActivationKey(activationKey);
                activationKeyTextBox.Text = ReadActivationKey().Replace("Stand-Activate-", "");
            }
        }
    }
}