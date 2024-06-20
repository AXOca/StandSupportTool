using System;
using System.Diagnostics;
using System.Windows;

namespace StandSupportTool
{
    public static class PowerShellExecutor
    {
        public static void ExecuteAddMpPreference()
        {
            // Prompt user for confirmation
            MessageBoxResult result = MessageBox.Show(
                "This feature will add an exclusion for Stand's BIN folder on Windows Defender.\n\nFor this, we need admin permissions.\n\nDo you want to continue?",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            // Exit method if user selects 'No'
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            // Construct the exclusion path
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string exclusionPath = System.IO.Path.Combine(appDataPath, "Stand", "Bin");
            string script = $"-Command Add-MpPreference -ExclusionPath '{exclusionPath}'";

            // Execute the command with admin privileges
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
