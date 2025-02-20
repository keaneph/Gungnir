using System.Windows;
using System.Windows.Controls;
using sis_app.Services;

namespace sis_app.Views
{
    public partial class LoginWindow : Window
    {
        private readonly UserDataService _userDataService;

        public LoginWindow()
        {
            InitializeComponent();
            _userDataService = new UserDataService("users.csv");
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (_userDataService.ValidateUser(username, password))
            {
                MainWindow mainWindow = new MainWindow(username); // Pass the username here
                mainWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid credentials. Please try again or register if you don't have an account.",
                    "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Add this event handler
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please fill in all fields.", "Registration Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_userDataService.UsernameExists(username))
            {
                MessageBox.Show("Username already exists. Please choose a different username.",
                    "Registration Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_userDataService.RegisterUser(username, password))
            {
                MessageBox.Show("Registration successful! You can now sign in.", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Clear the fields after successful registration
                UsernameTextBox.Text = "";
                PasswordBox.Password = "";
            }
            else
            {
                MessageBox.Show("Registration failed. Please try again.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UsernameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Add any username validation logic here if needed
        }
    }
}