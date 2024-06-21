using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace StandSupportTool
{
    public static class LaunchpadManager
    {
        private const string ZipUrl = "https://github.com/AXOca/StandSupportTool/raw/launchpad/launchpad.zip";
        private const string ConfirmationMessage = "This feature will download Stand's Launchpad into a new folder on your desktop and exclude it automatically from Windows Defender.\n\nFor this, we need admin permissions.\n\nDo you want to continue?";
        private const string SuccessMessage = "Done.\n\nLook at your Desktop, go into the \"Stand_Launchpad\" folder and unpack it.\n\nThe password is: stand.gg";
        private const string ErrorMessage = "Operation cancelled or failed due to insufficient permissions.";

        public static async Task PerformTest()
        {
            if (!ConfirmUserPermission())
            {
                return;
            }

            string folderPath = CreateDesktopFolder("Stand_Launchpad");

            AddExclusion(folderPath);

            await DownloadLaunchpadAsync(folderPath);

            InformUserOfSuccess();
        }

        private static bool ConfirmUserPermission()
        {
            MessageBoxResult result = MessageBox.Show(
                ConfirmationMessage,
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            return result == MessageBoxResult.Yes;
        }

        private static string CreateDesktopFolder(string folderName)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string folderPath = Path.Combine(desktopPath, folderName);
            Directory.CreateDirectory(folderPath);
            return folderPath;
        }

        private static void AddExclusion(string folderPath)
        {
            string script = $"-Command Add-MpPreference -ExclusionPath '{folderPath}'";
            ExecuteCommandAsAdmin(script);
        }

        private static void ExecuteCommandAsAdmin(string script)
        {
            string cmdArguments = $"/C powershell {script}";
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = cmdArguments,
                Verb = "runas",
                UseShellExecute = true,
                CreateNoWindow = true
            };

            try
            {
                using (Process process = new Process { StartInfo = startInfo })
                {
                    process.Start();
                    process.WaitForExit();
                }
            }
            catch (System.ComponentModel.Win32Exception)
            {
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static async Task DownloadLaunchpadAsync(string folderPath)
        {
            string zipPath = Path.Combine(folderPath, "launchpad.zip");
            await DownloadFileAsync(ZipUrl, zipPath);
        }

        private static async Task DownloadFileAsync(string url, string destinationPath)
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                await using (var fs = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await response.Content.CopyToAsync(fs);
                }
            }
        }

        private static void InformUserOfSuccess()
        {
            MessageBox.Show(SuccessMessage, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
