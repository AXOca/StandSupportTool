using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace StandSupportTool
{
    public static class LaunchpadManager
    {
        private const string LaunchPadUrl =
            "https://api.github.com/repos/calamity-inc/Stand-Launchpad/releases/latest";
        private const string ConfirmationMessage =
            "This feature will download Stand's Launchpad on your desktop and exclude it automatically from Windows Defender.\n\nFor this, we need admin permissions.\n\nDo you want to continue?";
        private const string SuccessMessage =
            "Done!\nYou should have the Launchpad on the Desktop.";
        private const string ErrorMessage =
            "Operation cancelled or failed due to insufficient permissions.";

        public static async Task PerformTest()
        {
            if (!ConfirmUserPermission())
            {
                return;
            }

            string launchpadPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "StandLaunchpad.exe"
            );
            AddExclusion(launchpadPath);

            await DownloadLatestExeAsync(launchpadPath);

            InformUserOfSuccess();
        }

        private static bool ConfirmUserPermission()
        {
            MessageBoxResult result = MessageBox.Show(
                ConfirmationMessage,
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            return result == MessageBoxResult.Yes;
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
                CreateNoWindow = true,
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

        private static async Task DownloadLatestExeAsync(string destinationPath)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "C# App");

                var response = await client.GetAsync(LaunchPadUrl);
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    using (JsonDocument doc = JsonDocument.Parse(jsonResponse))
                    {
                        JsonElement root = doc.RootElement;
                        JsonElement assets = root.GetProperty("assets");

                        foreach (JsonElement asset in assets.EnumerateArray())
                        {
                            string name = asset.GetProperty("name").GetString();
                            if (name.EndsWith(".exe"))
                            {
                                string downloadUrl = asset
                                    .GetProperty("browser_download_url")
                                    .GetString();
                                await DownloadFileAsync(downloadUrl, destinationPath);
                                return;
                            }
                        }
                    }
                    MessageBox.Show(
                        "No .exe file found in the latest release.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
                else
                {
                    MessageBox.Show(
                        $"Failed to retrieve the latest release. Status code: {response.StatusCode}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            }
        }

        private static async Task DownloadFileAsync(string url, string destinationPath)
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                await using (
                    var fs = new FileStream(
                        destinationPath,
                        FileMode.Create,
                        FileAccess.Write,
                        FileShare.None
                    )
                )
                {
                    await response.Content.CopyToAsync(fs);
                }
            }
        }

        private static void InformUserOfSuccess()
        {
            MessageBox.Show(
                SuccessMessage,
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }
    }
}
