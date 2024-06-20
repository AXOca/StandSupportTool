using System;
using System.IO;
using System.Windows;

namespace StandSupportTool
{
    public class ClearHotkeysManager
    {
        public void ClearHotkeys()
        {
            MessageBoxResult result = MessageBox.Show("This will delete your Hotkeys, do you want to continue?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                string hotkeysFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Stand/Hotkeys.txt");

                if (Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Stand")))
                {
                    if (File.Exists(hotkeysFilePath))
                    {
                        try
                        {
                            File.Delete(hotkeysFilePath);
                            MessageBox.Show("Hotkeys file has been deleted.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Failed to delete Hotkeys file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Hotkeys file does not exist.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBoxResult dirResult = MessageBox.Show("You never used Stand on this PC. Are you sure you are right here?", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    if (dirResult == MessageBoxResult.OK)
                    {
                        // Just close the dialog
                    }
                }
            }
        }
    }
}
