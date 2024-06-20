using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StandSupportTool
{
    internal class DashboardLinkOpener
    {
        public void OpenDashboardLink()
        {
            string url = "https://stand.gg/account/";

            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
    }
}
