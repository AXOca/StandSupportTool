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

            if (!activationKey.Contains("Stand-Activate"))
            {
                MessageBox.Show("The key you are trying to use doesn't follow the correct pattern, it must contain \"Stand-Activate-\".", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
