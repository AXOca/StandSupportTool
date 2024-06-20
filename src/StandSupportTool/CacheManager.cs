using System;
using System.IO;
using System.Windows;

namespace StandSupportTool
{
    public class CacheManager
    {
        public void ClearCache()
        {
            try
            {
                // Define cache directory path
                string cacheDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Stand/Cache");

                if (Directory.Exists(cacheDir))
                {
                    Directory.Delete(cacheDir, true);
                    MessageBox.Show("All Cache data has been deleted.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Cache directory does not exist.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete Cache data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}