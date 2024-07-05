using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using System.Security.Cryptography;

namespace StandSupportTool
{
    internal class Diagnostics
    {
        static string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        static string standDir = Path.Combine(appDataPath, "Stand");

        public class StandInjectionResult
        {
            public string[] Log { get; set; }
            public bool HasNetworkIssues { get; set; }
        }

        public class StandChecks
        {
            public bool IsAbleToConnect { get; set; }
            public bool SHAValidationCheck { get; set; }
            public bool isUpToDate { get; set; }
        }
        public class StandMetaState
        {
            public string[] state { get; set; }
            public string profileName { get; set; }
        }

        public class NetworkInfo
        {
            public string ISP { get; set; }
            public string ASN { get; set; }
        }

        public class EnvironmentData
        {
            public string OS_VERSION { get; set; }
            public NetworkInfo networkInfo { get; set; }
            public bool isRunningInCompatibilityMode { get; set; }
        }

        public class LaunchpadData
        {
            public string GameLauncher { get; set; } // This is the type of launcher used... it is defined as an enum
            public string[] CustomDlls { get; set; }
        }

        public class StandDumpData
        {
            public string ticketID { get; set; }
            public StandInjectionResult LastInjection { get; set; }
            public StandMetaState metaState { get; set; }
            public string[] profile { get; set; }
            public StandChecks Checks { get; set; }
            public EnvironmentData EnvironmentData { get; set; }
            public Dictionary<string, List<string>> installedLuaScripts { get; set; }
            public LaunchpadData LaunchpadData { get; set; }
        }

        public static StandInjectionResult ParseLastStandInjection()
        {
            string logFilePath = Path.Combine(standDir, "Log.txt");
            string standInjectionRegex = @"^.*Stand\s\d+(\.\d+)?\sreporting\sfor\sduty!$";
            string networkErrorRegex = @"Failed to send a request to stand\.gg:";

            StandInjectionResult result = new StandInjectionResult();

            try
            {
                if (File.Exists(logFilePath))
                {
                    // Allows us to read the log even if the game is running with Stand injected.
                    using (FileStream fs = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        var lines = new List<string>();
                        string? line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            lines.Add(line);
                        }

                        // Find the index of the last line matching the regex
                        int matchIndex = lines.FindLastIndex(line => Regex.IsMatch(line, standInjectionRegex));

                        if (matchIndex != -1)
                        {
                            result.Log = lines.Skip(matchIndex).ToArray();
                        }
                        else
                        {
                            MessageBox.Show("No 'Stand' injection found in the log.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }

                        // Check for network issues within the log
                        result.HasNetworkIssues = Regex.IsMatch(string.Join(Environment.NewLine, result.Log), networkErrorRegex);
                    }
                }
                else
                {
                    MessageBox.Show("Log file does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to read log file, reason: {ex.Message}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return result;
        }

        static string GetInstalledDllSHA256()
        {
            string BinPath = System.IO.Path.Combine(appDataPath, "Stand", "Bin");
            string dllFilePath = Directory.GetFiles(BinPath, "Stand*.dll")[0];
            try
            {
                using (FileStream stream = File.OpenRead(dllFilePath))
                {
                    using (SHA256 sha256 = SHA256.Create())
                    {
                        byte[] hash = sha256.ComputeHash(stream);
                        return BitConverter.ToString(hash).Replace("-", "").ToLower();
                    }
                }
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show($"Stand.dll not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (IOException ex)
            {
                MessageBox.Show($"Error reading Stand.dll, reason:{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error calculating SHA-256 hash for Stand.dll: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return "";
        }

        static string CalculateSHA256(byte[] inputBytes)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        static async Task<bool> PerformSHAChecks()
        {
            try
            {
                string BinPath = System.IO.Path.Combine(appDataPath, "Stand", "Bin");
                string[] dllFiles = Directory.GetFiles(BinPath, "Stand*.dll");

                if (dllFiles.Length == 0)
                {
                    MessageBox.Show($"Cannot find any Stand dll in '{BinPath}'.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                string dllName = Path.GetFileName(dllFiles[0]);
                string remoteUrl = $"https://stand.gg/{Uri.EscapeDataString(dllName)}";

                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(remoteUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        byte[] dllBytes = await response.Content.ReadAsByteArrayAsync();

                        string downloadedSHA256 = CalculateSHA256(dllBytes);
                        string installedSHA256 = GetInstalledDllSHA256();

                        bool shaMatches = downloadedSHA256.Equals(installedSHA256, StringComparison.OrdinalIgnoreCase);
                        return shaMatches;
                    }
                    else
                    {
                        MessageBox.Show($"Failed to download DLL '{dllName}'. Status code: {response.StatusCode}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error downloading upstream DLL, reason: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        static async Task<bool> ConnectionCheck()
        {
            string url = "https://stand.gg/";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(url);

                    return response.IsSuccessStatusCode;
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"HTTP request error, reason: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to stand.gg: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        static async Task<string> GetUpstreamVersion()
        {
            string url = "https://stand.gg/versions.txt";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string receivedVersions = await response.Content.ReadAsStringAsync();

                        int colonIndex = receivedVersions.IndexOf(':');
                        return colonIndex != -1 ? new string(receivedVersions.Skip(colonIndex + 1).ToArray()).Trim() : string.Empty;
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"HTTP request error when trying to get upstream: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to stand.gg for upstream: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return "";
        }

        static async Task<bool> isStandUpToDate()
        {
            string upstreamVersion = await GetUpstreamVersion();

            string BinPath = System.IO.Path.Combine(appDataPath, "Stand", "Bin");
            string dllName = Path.GetFileName(Directory.GetFiles(BinPath, "Stand*.dll")[0]);

            string localVersion = dllName.Replace("Stand ", "").Replace(".dll", "");

            if (upstreamVersion == localVersion)
                return true;

            return false;
        }

        static string GenerateRandomString(int length)
        {
            if (length <= 0)
                throw new ArgumentException("Length must be a positive integer.", nameof(length));

            string guidString = Guid.NewGuid().ToString("N");

            while (guidString.Length < length)
            {
                guidString += Guid.NewGuid().ToString("N");
            }

            return guidString.Substring(0, length);
        }

        public static async Task<Dictionary<string, List<string>>> GetInstalledLuaScripts()
        {
            string LuaPath = Path.Combine(appDataPath, "Stand", "Lua Scripts");

            if (!Directory.Exists(LuaPath))
            {
                MessageBox.Show("Lua Script Directory doesn't exist", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return new Dictionary<string, List<string>>();
            }

            Dictionary<string, List<string>> directoryFiles = new Dictionary<string, List<string>>();
            Stack<string> directories = new Stack<string>();
            directories.Push(LuaPath);

            try
            {
                while (directories.Count > 0)
                {
                    string currentPath = directories.Pop();
                    string relativeDir = Path.GetRelativePath(LuaPath, currentPath);

                    if (!directoryFiles.ContainsKey(relativeDir))
                    {
                        directoryFiles[relativeDir] = new List<string>();
                    }

                    string[] files = await Task.Run(() => Directory.GetFiles(currentPath));
                    foreach (string file in files)
                    {
                        string relativeFilePath = Path.GetRelativePath(LuaPath, file);
                        directoryFiles[relativeDir].Add(Path.GetFileName(relativeFilePath));
                    }

                    string[] subDirectories = await Task.Run(() => Directory.GetDirectories(currentPath));
                    foreach (string directory in subDirectories)
                    {
                        directories.Push(directory);
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show($"Access denied: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (PathTooLongException ex)
            {
                MessageBox.Show($"Path too long: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (IOException ex)
            {
                MessageBox.Show($"IO error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return directoryFiles;
        }
        static StandMetaState GetMetaState()
        {
            string metaStatePath = Path.Combine(standDir, "Meta State.txt");
            StandMetaState metaState = new StandMetaState();

            try
            {
                if (File.Exists(metaStatePath))
                {
                    metaState.state = File.ReadAllLines(metaStatePath);

                    Regex profileRegex = new Regex(@"Load On Inject: (\w+)");
                    Regex pre116ProfileRegex = new Regex(@"Active Profile: (\w+)");

                    foreach (string line in metaState.state)
                    {
                        // We are returning in the if statements just to avoid having override issues when someone might have
                        // a corrupted Meta State for whatever reason

                        Match match = profileRegex.Match(line);
                        if (match.Success)
                        {
                            metaState.profileName = match.Groups[1].Value;
                            return metaState;
                        }

                        // This one is for people still using pre-116 Stand... Really improbable but still...
                        match = pre116ProfileRegex.Match(line);
                        if (match.Success)
                        {
                            metaState.profileName = match.Groups[1].Value;
                            return metaState;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Meta State file does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to read Meta State file, reason: {ex.Message}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }

            // Nothing was found in there, must be Default profile!
            metaState.profileName = "Default";

            return metaState;
        }

        static string[] GetUsedProfile(string profile_name)
        {
            if (profile_name == "Default")
            {
                return ["Default"];
            }

            string profilePath = Path.Combine(standDir, "Profiles", profile_name + ".txt");

            try
            {
                if (File.Exists(profilePath))
                {
                    return File.ReadAllLines(profilePath);
                }
                else
                {
                    MessageBox.Show("Profile file does not exist", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to read Profile file, reason: {ex.Message}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return Array.Empty<string>();
        }
        static async Task<NetworkInfo> GetNetworkInfoAsync()
        {
            string url = "https://ipinfo.io/json";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();

                    if (responseBody != null && !string.IsNullOrWhiteSpace(responseBody))
                    {
                        JsonNode jsonNode = JsonNode.Parse(responseBody)!.AsObject();
                        if (jsonNode["org"] is JsonNode orgNode && orgNode.ToString() != null)
                        {
                            string org = orgNode.ToString();

                            // Split the ASN from the provider
                            string[] orgParts = org.Split(' ', 2);
                            string asn = orgParts[0];
                            string provider = orgParts.Length > 1 ? orgParts[1] : string.Empty;

                            return new NetworkInfo
                            {
                                ASN = asn,
                                ISP = provider
                            };
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"HTTP request error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to stand.gg: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Return empty info
            return new NetworkInfo();
        }
        static List<string> GetXMLValue(XElement settings, string settingName)
        {
            var settingElement =
                settings.Elements("setting")
                        .FirstOrDefault(e =>
                        {
                            var attributeName = e.Attribute("name");
                            if (attributeName != null)
                            {
                                return attributeName.Value == settingName;
                            }
                            return false;
                        });

            if (settingElement != null)
            {
                if (settingName == "CustomDll")
                {
                    var valueElement = settingElement.Element("value");
                    if (valueElement != null)
                    {
                        string attributeValue = valueElement.Value;
                        string[] paths = attributeValue.Split('|', StringSplitOptions.RemoveEmptyEntries);
                        return paths.Select(path => Path.GetFileName(path.Trim())).ToList();
                    }
                }

                if (settingName == "GameLauncher")
                {
                    var valueElement = settingElement.Element("value");
                    if (valueElement != null)
                    {
                        // Convert LauncherId to Stringfied version
                        switch (valueElement.Value)
                        {
                            case "0":
                                return new List<string> { "EGS" };
                            case "1":
                                return new List<string> { "STEAM" };
                            case "2":
                                return new List<string> { "RSG" };
                            default:
                                return new List<string> { "Unknown" };
                        }
                    }
                }
            }

            return new List<string>(); // Return an empty list if setting element is not found or invalid
        }

        static LaunchpadData ParseLaunchpadData(string xmlContent)
        {
            var doc = XDocument.Parse(xmlContent);
            var settings = doc.Descendants("Stand_Launchpad.Properties.Settings").FirstOrDefault();

            if (settings != null)
            {
                return new LaunchpadData
                {
                    GameLauncher = GetXMLValue(settings, "GameLauncher").FirstOrDefault() ?? "Unknown",
                    CustomDlls = GetXMLValue(settings, "CustomDll").ToArray()
                };
            }

            // Return empty data
            return new LaunchpadData();
        }
        static LaunchpadData GetLaunchpadProfile()
        {
            string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string launchpadFolder = Path.Combine(localAppDataPath, "Calamity,_Inc");

            try
            {
                if (Directory.Exists(launchpadFolder))
                {
                    var files = new DirectoryInfo(launchpadFolder).GetFiles("user.config", SearchOption.AllDirectories);

                    var lastModifiedFile = files.OrderByDescending(f => f.LastWriteTime).FirstOrDefault();

                    if (lastModifiedFile != null)
                    {
                        var xmlContent = File.ReadAllText(lastModifiedFile.FullName);
                        return ParseLaunchpadData(xmlContent);
                    }
                    else
                    {
                        MessageBox.Show("No user.config file found for Stand Launchpad.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    Trace.WriteLine($"The Launchpad local folder doesn't exist.");
                    // Keep it as a trace since some users might not be using the Launchpad at all!
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred when trying to get Launchpad Data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Return empty data
            return new LaunchpadData();
        }

        public static async Task DiagnosticsAsync()
        {
            StandInjectionResult lastInjection = ParseLastStandInjection();
            bool isAbleToConnect = await ConnectionCheck();

            StandChecks standChecks = new StandChecks();

            EnvironmentData environmentData = new EnvironmentData();

            CompatibilityScanner compatibilityScanner = new CompatibilityScanner();

            if (isAbleToConnect)
            {
                bool shaValidationCheck = await PerformSHAChecks();

                standChecks = new StandChecks
                {
                    IsAbleToConnect = isAbleToConnect,
                    SHAValidationCheck = shaValidationCheck,
                    isUpToDate = await isStandUpToDate()
                };
            }

            environmentData.networkInfo = await GetNetworkInfoAsync();
            environmentData.OS_VERSION = Environment.OSVersion.ToString();
            environmentData.isRunningInCompatibilityMode = compatibilityScanner.IsGameRunningInCompatibilityMode();

            StandDumpData data = new StandDumpData
            {
                ticketID = GenerateRandomString(8),
                LastInjection = lastInjection,
                metaState = GetMetaState(),
                profile = GetUsedProfile(GetMetaState().profileName),
                Checks = standChecks,
                EnvironmentData = environmentData,
                installedLuaScripts = await GetInstalledLuaScripts(),
                LaunchpadData = GetLaunchpadProfile()
            };

            string jsonData = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string filePath = Path.Combine(desktopPath, $"stand_{data.ticketID}.json");

            try
            {
                await File.WriteAllTextAsync(filePath, jsonData);
                MessageBox.Show($"A file named 'stand_{data.ticketID}.json' has been created on your Desktop.\nPlease send this file to the support chat for further assistance.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error writing output data to file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
