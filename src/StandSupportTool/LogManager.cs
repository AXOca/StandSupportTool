using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace StandSupportTool
{
    internal class LogManager
    {
        private readonly string logFilePath;

        public LogManager()
        {
            // Initialize the path to the log file
            logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Stand", "Log.txt");
        }

        // Read the log file
        public string ReadLog()
        {
            try
            {
                if (File.Exists(logFilePath))
                {
                    FileInfo logFileInfo = new FileInfo(logFilePath);
                    if (logFileInfo.Length > 24.9 * 1024 * 1024) // Check if the file size is greater than 24.9 MB
                    {
                        return ReadLastLines(logFilePath, 1000); // Read last 1000 lines
                    }
                    else
                    {
                        return File.ReadAllText(logFilePath); // Read the entire log file
                    }
                }
                else
                {
                    MessageBox.Show("Log file not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to read log file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return string.Empty;
            }
        }

        // Read the last specified number of lines from a file
        private string ReadLastLines(string path, int lineCount)
        {
            var lines = new LinkedList<string>();
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var sr = new StreamReader(fs, Encoding.Default))
                {
                    fs.Seek(0, SeekOrigin.End);

                    var buffer = new char[1024];
                    int charsRead;
                    var sb = new StringBuilder();

                    while (fs.Position > 0)
                    {
                        int readLength = (int)Math.Min(fs.Position, buffer.Length);
                        fs.Seek(-readLength, SeekOrigin.Current);
                        charsRead = sr.Read(buffer, 0, readLength);
                        fs.Seek(-readLength, SeekOrigin.Current);

                        sb.Insert(0, buffer, 0, charsRead);

                        if (sb.ToString().Count(c => c == '\n') >= lineCount)
                        {
                            break;
                        }
                    }

                    foreach (var line in sb.ToString().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None))
                    {
                        if (lines.Count >= lineCount)
                        {
                            lines.RemoveFirst();
                        }
                        lines.AddLast(line);
                    }
                }
            }
            return string.Join(Environment.NewLine, lines);
        }

        // Copy the log file content to the clipboard
        public void CopyLogToClipboard()
        {
            try
            {
                string logContent = ReadLog();
                if (!string.IsNullOrEmpty(logContent))
                {
                    Clipboard.SetText(logContent);
                    MessageBox.Show("Log file copied to clipboard.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to copy log file to clipboard: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
