using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Text.Json;

namespace StandSupportTool
{
    public class UpdateManager
    {
        private const string RepoOwner = "AXOca";
        private const string RepoName = "StandSupportTool";
        private const string ApiUrl = $"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest";
        private readonly string currentVersion;
        private readonly string executablePath;

        public UpdateManager(string currentVersion, string executablePath)
        {
            this.currentVersion = currentVersion;
            this.executablePath = executablePath;
        }
        public async Task<bool> CheckForUpdates()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; StandSupportToolClient/1.0)");
                    string json = await client.GetStringAsync(ApiUrl);
                    using JsonDocument doc = JsonDocument.Parse(json);
                    JsonElement root = doc.RootElement;
                    if (root.TryGetProperty("tag_name", out JsonElement tagNameElement) && tagNameElement.GetString() is string latestVersion)
                    {
                        return CompareVersions(latestVersion.Trim(), currentVersion);
                    }
                    else
                    {
                        MessageBox.Show("Failed to retrieve the latest version.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to check for updates: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task DownloadUpdate()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; StandSupportToolClient/1.0)");
                    string json = await client.GetStringAsync(ApiUrl);
                    using JsonDocument doc = JsonDocument.Parse(json);
                    JsonElement root = doc.RootElement;
                    if (root.TryGetProperty("assets", out JsonElement assetsElement) && assetsElement[0].TryGetProperty("browser_download_url", out JsonElement downloadUrlElement) && downloadUrlElement.GetString() is string downloadUrl)
                    {
                        byte[] data = await client.GetByteArrayAsync(downloadUrl);
                        string tempFilePath = Path.Combine(Path.GetTempPath(), "StandSupportTool_new.exe");

                        File.WriteAllBytes(tempFilePath, data);

                        string batchFilePath = Path.Combine(Path.GetTempPath(), "update.bat");

                        string batchScript = $@"
                            @echo off
                            taskkill /f /im {Path.GetFileName(executablePath)} > nul 2>&1
                            timeout /t 2 /nobreak > nul
                            del /f ""{executablePath}""
                            if exist ""{executablePath}"" (
                                echo Old file still exists, cannot replace.
                                pause
                                exit /b 1
                            )
                            move /y ""{tempFilePath}"" ""{executablePath}""
                            if %errorlevel% neq 0 (
                                echo Error replacing the file.
                                pause
                                exit /b %errorlevel%
                            )
                            start """" ""{executablePath}""
                            del ""{batchFilePath}""
                        ";

                        File.WriteAllText(batchFilePath, batchScript);

                        ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd.exe", $"/c \"{batchFilePath}\"")
                        {
                            CreateNoWindow = true,
                            UseShellExecute = false
                        };

                        Process.Start(processStartInfo);

                        MessageBox.Show($"Update downloaded to: {tempFilePath}", "Update Info", MessageBoxButton.OK, MessageBoxImage.Information);

                        LogUpdateProcess(tempFilePath, "Update process initiated.");

                        Application.Current.Shutdown();
                    }
                    else
                    {
                        MessageBox.Show("Failed to retrieve the download URL.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to download update: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                LogUpdateProcess(null, $"Error - {ex.Message}");
            }
        }

        private bool CompareVersions(string latestVersion, string currentVersion)
        {
            Version v1 = new Version(latestVersion);
            Version v2 = new Version(currentVersion);
            return v1 > v2;
        }

        private void LogUpdateProcess(string? tempFilePath, string message)
        {
            string logMessage = $"{DateTime.Now}: {message}";
            if (!string.IsNullOrEmpty(tempFilePath))
            {
                logMessage += $" Temp file path: {tempFilePath}";
            }

            File.AppendAllText(Path.Combine(Path.GetTempPath(), "update_log.txt"), logMessage + Environment.NewLine);
        }
    }
}