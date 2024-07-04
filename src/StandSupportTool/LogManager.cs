using System;
using System.IO;
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

        // Display a window for the user to select log entries and copy to clipboard
        public void CopyLogToClipboard()
        {
            SelectLogWindow selectLogWindow = new SelectLogWindow(logFilePath);
            selectLogWindow.ShowDialog();
        }
    }
}
