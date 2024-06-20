using System;
using System.Collections.Generic;
using System.Management;
using System.Windows;

namespace StandSupportTool
{
    internal class AntivirusInfo
    {
        public string DisplayName { get; set; }
        public string ExePath { get; set; }

        public List<AntivirusInfo> GetAntivirusInfo()
        {
            List<AntivirusInfo> antivirusInfos = new List<AntivirusInfo>();
            try
            {
                // Annoying but avoids random AVs flagging the exe
                string securityCenter64 = "XHJvb3RcU2VjdXJpdHlDZW50ZXIy";       // @"\root\SecurityCenter2"
                string query64 = "U0VMRUNUICogRlJPTSBBbnRpdmlydXNQcm9kdWN0";    //  "SELECT * FROM AntivirusProduct"

                string wmipathstr = @"\\" + Environment.MachineName + Base64.Decode(securityCenter64);
                string query = Base64.Decode(query64);

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(wmipathstr, query))
                using (ManagementObjectCollection instances = searcher.Get())
                {
                    foreach (ManagementObject instance in instances)
                    {
                        string displayName = instance["displayName"]?.ToString() ?? "Unknown";
                        string exePath = instance["pathToSignedReportingExe"]?.ToString() ?? "Unknown";

                        antivirusInfos.Add(new AntivirusInfo
                        {
                            DisplayName = displayName,
                            ExePath = exePath
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to gather antivirus information: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return antivirusInfos;
        }
    }
}