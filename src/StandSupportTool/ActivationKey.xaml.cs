using System.Windows;

namespace StandSupportTool
{
    public partial class ActivationKey : BaseWindow
    {
        private readonly ActivationManager activationManager;

        public ActivationKey()
        {
            InitializeComponent();
            activationManager = new ActivationManager();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            string activationKey = ActivationKeyText.Text;
            string validPattern = "Stand-Activate-";

            if (!activationKey.Contains(validPattern))
            {
                MessageBox.Show(
                    "The key you are trying to use doesn't follow the correct pattern, it must contain \"Stand-Activate-\".",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }

            int startIndex = activationKey.IndexOf(validPattern) + validPattern.Length;
            string keySuffix = activationKey.Substring(startIndex);

            // This will work as long as keys don't contain uppercase characters
            if (!keySuffix.All(c => char.IsLower(c) || char.IsDigit(c)))
            {
                MessageBox.Show(
                    "This is not an activation key, it contains uppercase characters.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }

            activationManager.WriteActivationKey(activationKey);
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
