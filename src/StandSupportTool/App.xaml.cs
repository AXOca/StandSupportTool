using System;
using System.Management;
using System.Windows;

namespace StandSupportTool
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Check if we have commands here, if we do, parse them in the switch case and perform the actual thing
            if (e.Args.Length > 0)
            {
                string command = e.Args[0];

                switch (command)
                {
                    case "--add-exclusion":
                        if (e.Args.Length == 2)
                        {
                            ExclusionManager ex_mgr = new ExclusionManager();
                            ex_mgr.AddExclusion(e.Args[1]);
                        }
                        else
                        {
                            MessageBox.Show(
                                "Invalid arguments for adding exclusion.",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error
                            );
                            Environment.Exit(1);
                        }
                        break;

                    // Future feature cases can be added here
                    // case "--whatever":
                    //     whatever();
                    //     break;

                    default:
                        MessageBox.Show(
                            "Unknown command.",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error
                        );
                        Environment.Exit(1);
                        break;
                }
            }
        }
    }
}
