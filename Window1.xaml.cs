using System.Windows;

namespace glaive // Ensure this matches the x:Class in LoginWindow.xaml
{
    using sis_app;  // Add this using directive
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // For demonstration, use hard-coded credentials.
            // In a real app, validate against a database or an authentication service.
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (username == "admin" && password == "password")
            {
                // Credentials are valid.
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid credentials. Please try again.", "Login Failed",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}