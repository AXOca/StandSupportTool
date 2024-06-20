using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;

namespace StandSupportTool
{
    public static class LaunchpadManager
    {
        public static void PerformTest()
        {
            // Prompt user for confirmation
            MessageBoxResult result = MessageBox.Show(
                "This feature will download Stand's Launchpad into a new folder on your desktop and exclude it automatically from Windows Defender.\n\nFor this, we need admin permissions.\n\nDo you want to continue?",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            // Exit method if user selects 'No'
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            // Define paths
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string folderPath = Path.Combine(desktopPath, "Stand_Launchpad");

            // Create folder on desktop
            Directory.CreateDirectory(folderPath);

            // Add exclusion for the newly created folder
            AddExclusion(folderPath);

            // Download the zip file into the folder
            string zipUrl = "https://github.com/AXOca/Stand-Tools/raw/main/StandSupportTool-cs/resources/launchpad.zip"; // Replace with actual URL
            string zipPath = Path.Combine(folderPath, "launchpad.zip");

            using (WebClient client = new WebClient())
            {
                client.DownloadFile(zipUrl, zipPath);
            }

            // Inform the user about the success
            MessageBox.Show("Done.\n\nLook at your Desktop, go into the \"Stand_Launchpad\" folder and unpack it.\n\nThe password is: stand.gg", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private static void AddExclusion(string folderPath)
        {
            // PowerShell script to add an exclusion
            string script = $"-Command Add-MpPreference -ExclusionPath '{folderPath}'";

            // Execute the script with admin privileges
            ExecuteCommandAsAdmin(script);
        }

        private static void ExecuteCommandAsAdmin(string script)
        {
            // Set up the command line arguments for the PowerShell script
            string cmdArguments = $"/C powershell {script}";
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = cmdArguments,
                Verb = "runas", // Request admin privileges
                UseShellExecute = true,
                CreateNoWindow = true
            };

            try
            {
                // Start the process and wait for it to exit
                using (Process process = new Process { StartInfo = startInfo })
                {
                    process.Start();
                    process.WaitForExit();
                }
            }
            catch (System.ComponentModel.Win32Exception)
            {
                // Handle case where the user cancels the UAC prompt
                MessageBox.Show("Operation cancelled or failed due to insufficient permissions.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}