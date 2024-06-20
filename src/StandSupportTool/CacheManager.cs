using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace StandSupportTool
{
    public class CacheManager
    {
        // Define cache directory paths
        private static readonly string StandCacheDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Stand/Cache");
        private static readonly string StandBinDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Stand/Bin");
        private static readonly string LaunchpadCache1 = Path.Combine(Environment.GetEnvironmentVariable("ProgramData") ?? string.Empty, "Calamity, Inc");
        private static readonly string LaunchpadCache2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Calamity,_Inc");

        public async Task ClearCache()
        {
            string resultMessage = string.Empty;

            try
            {
                // Attempt to delete the Stand/Cache directory
                resultMessage += $"Stand-Cache: {await DeleteDirectoryAsync(StandCacheDir)}\n";
                // Attempt to delete the Stand/Bin directory
                resultMessage += $"Stand-Bin: {await DeleteDirectoryAsync(StandBinDir)}\n";
                // Attempt to delete the first launchpad directory
                resultMessage += $"Launchpad-Cache1: {await DeleteDirectoryAsync(LaunchpadCache1)}\n";
                // Attempt to delete the second launchpad directory
                resultMessage += $"Launchpad-Cache2: {await DeleteDirectoryAsync(LaunchpadCache2)}\n";

                MessageBox.Show(resultMessage, "Result", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete specified directories: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<string> DeleteDirectoryAsync(string path)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (Directory.Exists(path))
                    {
                        Directory.Delete(path, true);
                        return "Deleted";
                    }
                    else
                    {
                        return "Does not exist";
                    }
                }
                catch (Exception ex)
                {
                    return $"Error: {ex.Message}";
                }
            });
        }
    }
}