using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace StandSupportTool
{
    public class HotkeyManager
    {
        private readonly string hotkeysFilePath;
        private ObservableCollection<Hotkey> _hotkeys;

        bool writeSmallerKeyboardHotkeysOnSave = false;

        List<Hotkey> Hotkeys60 = new List<Hotkey>
        {
            new Hotkey { KeyBinding = "Tab", Action = "Open/Close Menu" },
            new Hotkey { KeyBinding = "O", Action = "Previous Tab" },
            new Hotkey { KeyBinding = "P", Action = "Next Tab" },
            new Hotkey { KeyBinding = "I", Action = "Up" },
            new Hotkey { KeyBinding = "K", Action = "Down" },
            new Hotkey { KeyBinding = "J", Action = "Left" },
            new Hotkey { KeyBinding = "L", Action = "Right" },
            new Hotkey { KeyBinding = "Enter", Action = "Click" },
            new Hotkey { KeyBinding = "Backspace", Action = "Back" },
            new Hotkey { KeyBinding = "O, Numpad 3, H", Action = "Context Menu" }
        };

        public ObservableCollection<Hotkey> Hotkeys
        {
            get => _hotkeys;
            private set => _hotkeys = value;
        }

        public class Hotkey
        {
            public string Action { get; set; }
            public string KeyBinding { get; set; }
        }

        public HotkeyManager()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string standDir = Path.Combine(appDataPath, "Stand");
            hotkeysFilePath = Path.Combine(standDir, "Hotkeys.txt");

            Directory.CreateDirectory(standDir);

            _hotkeys = new ObservableCollection<Hotkey>();

            if (File.Exists(hotkeysFilePath))
            {
                var parsedHotkeys = Parse();
                foreach (var hotkey in parsedHotkeys)
                {
                    _hotkeys.Add(hotkey);
                }
            }
        }


        public List<Hotkey> Parse()
        {
            List<Hotkey> hotkeys = new List<Hotkey>();

            using (StreamReader reader = new StreamReader(hotkeysFilePath))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains(":"))
                    {
                        // Skip the Compatibility Tree Version!
                        if (line.Contains("Tree Compatibility Version"))
                            continue;

                        string[] parts = line.Split(':');
                        string actionName = parts[0].Trim();
                        string keyBinding = parts[1].Trim();

                        hotkeys.Add(new Hotkey
                        {
                            Action = actionName,
                            KeyBinding = keyBinding
                        });
                    }
                }
            }

            return hotkeys;
        }

        // Configures the 60% keyboard keys
        public void SmallerKeyboard()
        {
            foreach (var hotkey in Hotkeys60)
            {
                if (Hotkeys.Any(h => h.KeyBinding == hotkey.KeyBinding))
                {
                    MessageBox.Show($"The key '{hotkey.KeyBinding}' is already used. Please replace it.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    Hotkeys.Add(hotkey);
                }
            }
            writeSmallerKeyboardHotkeysOnSave = true;
        }
        private string ReplaceLineWithPreservedIndentation(string originalLine, string newLine)
        {
            int tabCount = originalLine.Length - originalLine.TrimStart('\t').Length;
            return new string('\t', tabCount) + newLine; 
        }

        public void SaveHotkeys()
        {
            List<string> lines = new List<string>();
            if (File.Exists(hotkeysFilePath))
            {
                lines.AddRange(File.ReadAllLines(hotkeysFilePath));
            }

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];

                if (!line.Contains(":"))
                    continue;

                string[] parts = line.Split(':');
                string actionName = parts[0].Trim();
                string keyBinding = parts[1].Trim();

                // Check if keyBinding matches any KeyBinding in Hotkeys60
                if (!Hotkeys60.Any(h => h.KeyBinding == keyBinding))
                {
                    // Find matching hotkey in Hotkeys list
                    var hotkey = Hotkeys.FirstOrDefault(h => h.Action == actionName);
                    if (hotkey != null)
                    {
                        // Construct the updated line preserving original tab-based indentation
                        string updatedLine = $"{actionName}: {hotkey.KeyBinding}";

                        // Replace the line while preserving original tab-based indentation
                        lines[i] = ReplaceLineWithPreservedIndentation(lines[i], updatedLine);
                    }
                }
            }

            // Check if we need to write smaller keyboard hotkeys
            if (writeSmallerKeyboardHotkeysOnSave)
            {
                // Find or create the "Keyboard Input Scheme" section
                int inputSchemeIndex = lines.FindIndex(line => line.Trim() == "Keyboard Input Scheme");
                if (inputSchemeIndex == -1)
                {
                    // If "Keyboard Input Scheme" section is not found, add it at the end
                    lines.Add("Stand");
                    lines.Add("\tSettings");
                    lines.Add("\t\tInput");
                    lines.Add("\t\t\tKeyboard Input Scheme");
                    inputSchemeIndex = lines.Count;
                }
                else
                {
                    // Move to the line after the "Keyboard Input Scheme" header
                    inputSchemeIndex++;
                }

                // Add or update hotkeys under the "Context Menu" subsection
                foreach (var hotkey in Hotkeys60)
                {
                    int hotkeyIndex = lines.FindIndex(inputSchemeIndex, line => line.Trim().StartsWith(hotkey.Action + ":"));

                    if (hotkeyIndex != -1)
                    {
                        // Update existing hotkey
                        lines[hotkeyIndex] = $"\t\t\t\t{hotkey.Action}: {hotkey.KeyBinding}";
                    }
                    else
                    {
                        // Add new hotkey
                        string hotkeyLine = $"\t\t\t\t{hotkey.Action}: {hotkey.KeyBinding}";
                        lines.Insert(inputSchemeIndex++, hotkeyLine);
                    }
                }
            }
            File.WriteAllLines(hotkeysFilePath, lines);

            // Avoid writing multiple times
            writeSmallerKeyboardHotkeysOnSave = false;
        }
    }
}
