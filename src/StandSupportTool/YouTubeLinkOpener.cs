using System.Diagnostics;

namespace StandSupportTool
{
    public static class YouTubeLinkOpener
    {
        // This class is unused.
        public static void OpenYouTubeLink()
        {
            // URL of the YouTube video to be opened
            string url = "https://www.youtube.com/watch?v=UbtuQwgNfp0";

            // Start the process to open the YouTube link in the default web browser
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
    }
}
