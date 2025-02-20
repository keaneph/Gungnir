using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace glaive
{
    public partial class AddCollegeControl : UserControl
    {
        private CollegeDataService _collegeDataService;

        // Add event for history tracking
        public event EventHandler<CollegeEventArgs> CollegeAdded;

        public AddCollegeControl(CollegeDataService collegeDataService)
        {
            InitializeComponent();
            _collegeDataService = collegeDataService;
        }

        private void AddCollegeButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(CollegeNameTextBox.Text) && !string.IsNullOrEmpty(CollegeCodeTextBox.Text))
            {
                try
                {
                    College newCollege = new College
                    {
                        Name = CollegeNameTextBox.Text,
                        Code = CollegeCodeTextBox.Text,
                        DateTime = DateTime.Now,
                        User = _collegeDataService.CurrentUser
                    };

                    // Validate college code doesn't already exist
                    var existingColleges = _collegeDataService.GetAllColleges();
                    if (existingColleges.Exists(c => c.Code.Equals(newCollege.Code, StringComparison.OrdinalIgnoreCase)))
                    {
                        MessageBox.Show($"A college with code '{newCollege.Code}' already exists.", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    _collegeDataService.AddCollege(newCollege);

                    // Raise the event for history tracking
                    CollegeAdded?.Invoke(this, new CollegeEventArgs(newCollege));

                    MessageBox.Show($"Added college {newCollege.Name} ({newCollege.Code})", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    // Clear input fields
                    ClearFields();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding college: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please enter both College Name and College Code.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ClearFields()
        {
            CollegeNameTextBox.Text = "";
            CollegeCodeTextBox.Text = "";
        }

        private void CollegeNameTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow only letters and spaces
            Regex regex = new Regex("[^a-zA-Z ]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void CollegeCodeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow only letters
            Regex regex = new Regex("[^a-zA-Z]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void CollegeCodeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Convert to uppercase
            if (sender is TextBox textBox)
            {
                int caretIndex = textBox.CaretIndex;
                textBox.Text = textBox.Text.ToUpper();
                textBox.CaretIndex = caretIndex;
            }
        }

        // Optional: Method to validate input
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(CollegeNameTextBox.Text))
            {
                MessageBox.Show("Please enter a college name.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                CollegeNameTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(CollegeCodeTextBox.Text))
            {
                MessageBox.Show("Please enter a college code.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                CollegeCodeTextBox.Focus();
                return false;
            }

            if (CollegeCodeTextBox.Text.Length < 2 || CollegeCodeTextBox.Text.Length > 5)
            {
                MessageBox.Show("College code must be between 2 and 5 characters.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                CollegeCodeTextBox.Focus();
                return false;
            }

            return true;
        }
    }

}