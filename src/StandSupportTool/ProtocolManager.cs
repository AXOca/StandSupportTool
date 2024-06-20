using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace StandSupportTool
{
    internal class ProtocolManager
    {
        private string choosenProtocol;

        public ProtocolManager()
        {
            ReadProtocolFromFile();
        }

        // Read protocol from the file
        private void ReadProtocolFromFile()
        {
            try
            {
                string standDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Stand");
                string stateFile = Path.Combine(standDir, "Meta State.txt");

                if (File.Exists(stateFile))
                {
                    var lines = File.ReadAllLines(stateFile);
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("DNS Conduit:"))
                        {
                            choosenProtocol = line.Split(':')[1].Trim();
                            return;
                        }
                    }
                }

                // Default protocol if not found in the file
                choosenProtocol = "SMART";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to read protocol: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                choosenProtocol = "SMART"; // Fallback to default in case of an error
            }
        }

        // Set the chosen protocol
        public void setProtocol(string protocol)
        {
            choosenProtocol = protocol;
        }

        // Get the chosen protocol
        public string getProtocol()
        {
            return choosenProtocol;
        }

        // Write the chosen protocol to the file
        public void writeProtocol()
        {
            try
            {
                string standDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Stand");
                string stateFile = Path.Combine(standDir, "Meta State.txt");

                if (!Directory.Exists(standDir))
                {
                    MessageBox.Show("Stand folder doesn't exist, make sure to run Stand before changing protocol!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var lines = new List<string>();
                if (File.Exists(stateFile))
                {
                    lines = File.ReadAllLines(stateFile).ToList();
                }

                using (StreamWriter sw = new StreamWriter(stateFile))
                {
                    bool found = false;
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("DNS Conduit:"))
                        {
                            sw.WriteLine($"DNS Conduit: {choosenProtocol}");
                            found = true;
                        }
                        else
                        {
                            sw.WriteLine(line);
                        }
                    }
                    if (!found)
                    {
                        sw.WriteLine($"DNS Conduit: {choosenProtocol}");
                    }
                }

                MessageBox.Show("Protocol changed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to write protocol: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
