using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StandSupportTool
{
    public partial class SelectLogWindow : BaseWindow
    {
        private readonly string logFilePath;
        private readonly string standInjectionRegex =
            @"^.*Stand\s\d+(\.\d+)?\sreporting\sfor\sduty!$";
        private List<string> allLogLines;

        public SelectLogWindow(string logFilePath)
        {
            InitializeComponent();
            this.logFilePath = logFilePath;
            LoadLogEntries();
        }

        private void LoadLogEntries()
        {
            allLogLines = new List<string>();
            List<string> matchingEntries = new List<string>();

            try
            {
                if (File.Exists(logFilePath))
                {
                    using (
                        FileStream fs = new FileStream(
                            logFilePath,
                            FileMode.Open,
                            FileAccess.Read,
                            FileShare.ReadWrite
                        )
                    )
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        string? line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            allLogLines.Add(line);
                            if (Regex.IsMatch(line, standInjectionRegex))
                            {
                                matchingEntries.Add(line);
                            }
                        }
                    }
                    // Puts them by most recent to oldest
                    matchingEntries.Reverse();
                    MostRecentLogListBox.ItemsSource = new List<string> { matchingEntries.First() };
                    LogListBox.ItemsSource = matchingEntries.Skip(1).ToList(); // Skip the first entry for LogListBox

                    // Skip the first entry since that's the top one
                    AdjustWindowWidthToFitLogEntries(matchingEntries.Skip(1).ToList());
                }
                else
                {
                    MessageBox.Show(
                        "Log file not found.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to read log file: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void AdjustWindowWidthToFitLogEntries(List<string> logEntries)
        {
            var formattedText = logEntries
                .Select(log => new FormattedText(
                    log,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(
                        LogListBox.FontFamily,
                        LogListBox.FontStyle,
                        LogListBox.FontWeight,
                        LogListBox.FontStretch
                    ),
                    LogListBox.FontSize,
                    Brushes.White,
                    new NumberSubstitution(),
                    1
                ))
                .ToList();

            // Gives padding on the side to fix the content
            var maxWidth = formattedText.Max(ft => ft.Width) + 100;
            this.Width = maxWidth;
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedEntry = LogListBox.SelectedItem as string;
            if (selectedEntry != null)
            {
                string logContent = GetLogEntriesAfterSelected(selectedEntry);
                Clipboard.SetText(logContent);
                MessageBox.Show(
                    "Selected log entries copied to clipboard.",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            else
            {
                var mostRecentEntry = MostRecentLogListBox
                    .ItemsSource.Cast<string>()
                    .FirstOrDefault();
                if (mostRecentEntry != null)
                {
                    Clipboard.SetText(GetLogEntriesAfterSelected(mostRecentEntry));
                    MessageBox.Show(
                        "Most recent log entry copied to clipboard.",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
                else
                {
                    MessageBox.Show(
                        "No entry selected.",
                        "Warning",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                }
            }
        }

        private string GetLogEntriesAfterSelected(string selectedEntry)
        {
            List<string> logSegment = new List<string>();
            bool isSegment = false;

            foreach (var entry in allLogLines)
            {
                if (isSegment)
                {
                    if (Regex.IsMatch(entry, standInjectionRegex))
                    {
                        break;
                    }
                    logSegment.Add(entry);
                }

                if (entry == selectedEntry)
                {
                    isSegment = true;
                    logSegment.Add(entry);
                }
            }

            return string.Join(Environment.NewLine, logSegment);
        }
    }
}
