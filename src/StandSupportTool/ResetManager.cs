using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace StandSupportTool
{
    internal class ResetManager
    {
        // Show a confirmation dialog and return true if the user clicks 'Yes'
        private bool ShowConfirmation(string message, string caption, MessageBoxImage icon = MessageBoxImage.Question)
        {
            MessageBoxResult result = MessageBox.Show(message, caption, MessageBoxButton.YesNo, icon, MessageBoxResult.No);
            return result == MessageBoxResult.Yes;
        }

        // Perform a full reset with multiple confirmation steps
        public void FullReset()
        {
            // Initial prompt
            if (!ShowConfirmation("Please close GTA V and Stand's Launchpad. Continue?", "Initial Prompt"))
                return;

            // Confirmation dialog
            if (!ShowConfirmation("Are you sure you want to do that? All your outfits and individual settings will be lost.", "Confirmation Dialog", MessageBoxImage.Warning))
                return;

            // Backup choice
            if (ShowConfirmation("Do you want to back it up first?", "Backup Choice"))
            {
                bool backupSuccess = BackupStandData();
                if (!backupSuccess)
                    return;
            }

            // Delete Stand data
            DeleteStandData();
        }

        // Backup Stand data
        private bool BackupStandData()
        {
            try
            {
                string standDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Stand");
                var itemsToBackup = SelectItemsToBackup(standDir);

                if (itemsToBackup == null || !itemsToBackup.Any())
                    return false;

                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string backupDir = Path.Combine(desktop, "Stand_BackUp");
                Directory.CreateDirectory(backupDir);

                // Copy selected items to the backup directory
                foreach (var item in itemsToBackup)
                {
                    string itemPath = Path.Combine(standDir, item);
                    string destPath = Path.Combine(backupDir, item);

                    if (Directory.Exists(itemPath))
                    {
                        CopyDirectory(itemPath, destPath);
                    }
                    else if (File.Exists(itemPath))
                    {
                        File.Copy(itemPath, destPath, true);
                    }
                }

                MessageBox.Show("Backup completed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to backup Stand data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // Copy all files and subdirectories from source to destination
        private void CopyDirectory(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            foreach (var subDir in Directory.GetDirectories(sourceDir))
            {
                string destSubDir = Path.Combine(destDir, Path.GetFileName(subDir));
                CopyDirectory(subDir, destSubDir);
            }
        }

        // Select items to backup from the Stand directory
        private string[]? SelectItemsToBackup(string standDir)
        {
            var dialog = new Window
            {
                Title = "Select Items to Backup",
                Width = 400,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.CanResize
            };

            var grid = new Grid();
            dialog.Content = grid;

            var row1 = new RowDefinition { Height = new GridLength(1, GridUnitType.Star) };
            var row2 = new RowDefinition { Height = GridLength.Auto };
            grid.RowDefinitions.Add(row1);
            grid.RowDefinitions.Add(row2);

            var scrollViewer = new ScrollViewer();
            Grid.SetRow(scrollViewer, 0);
            grid.Children.Add(scrollViewer);

            var listBox = new ListBox
            {
                SelectionMode = SelectionMode.Multiple,
                Margin = new Thickness(10)
            };
            scrollViewer.Content = listBox;

            var folders = Directory.GetDirectories(standDir).Select(Path.GetFileName).OrderBy(f => f).ToArray();
            var files = Directory.GetFiles(standDir).Select(Path.GetFileName).OrderBy(f => f).ToArray();

            foreach (var folder in folders)
            {
                listBox.Items.Add($"[Folder] {folder}");
            }

            foreach (var file in files)
            {
                listBox.Items.Add($"[File] {file}");
            }

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(10)
            };
            Grid.SetRow(buttonPanel, 1);
            grid.Children.Add(buttonPanel);

            var okButton = new Button
            {
                Content = "OK",
                Width = 75,
                Margin = new Thickness(5)
            };
            okButton.Click += (sender, e) => dialog.DialogResult = true;
            buttonPanel.Children.Add(okButton);

            var cancelButton = new Button
            {
                Content = "Cancel",
                Width = 75,
                Margin = new Thickness(5)
            };
            cancelButton.Click += (sender, e) => dialog.DialogResult = false;
            buttonPanel.Children.Add(cancelButton);

            var backupAllButton = new Button
            {
                Content = "Backup All",
                Width = 75,
                Margin = new Thickness(5)
            };
            backupAllButton.Click += (sender, e) =>
            {
                listBox.SelectAll();
                dialog.DialogResult = true;
            };
            buttonPanel.Children.Add(backupAllButton);

            dialog.ShowDialog();

            if (dialog.DialogResult == true)
            {
                return listBox.SelectedItems
                    .Cast<string>()
                    .Select(item => item.Split(' ', 2)[1])
                    .ToArray();
            }
            return null;
        }

        // Delete Stand data
        private void DeleteStandData()
        {
            try
            {
                string standDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Stand");

                if (Directory.Exists(standDir))
                {
                    Directory.Delete(standDir, true);
                    MessageBox.Show("All Stand data has been deleted.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Stand directory does not exist.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete Stand data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
