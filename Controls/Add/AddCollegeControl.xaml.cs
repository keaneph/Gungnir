using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using sis_app.Models;
using sis_app.Services;

namespace sis_app.Controls.Add
{
    public partial class AddCollegeControl : UserControl
    {
        // service for handling college data operations
        private CollegeDataService _collegeDataService;

        // constructor initializes the control and sets up the data service
        public AddCollegeControl(CollegeDataService collegeDataService)
        {
            InitializeComponent();
            _collegeDataService = collegeDataService;
        }

        // handles the add college button click event
        private void AddCollegeButton_Click(object sender, RoutedEventArgs e)
        {
            // check if both fields are filled
            if (!string.IsNullOrEmpty(CollegeNameTextBox.Text) && !string.IsNullOrEmpty(CollegeCodeTextBox.Text))
            {
                // create new college object with current data
                College newCollege = new College
                {
                    Name = CollegeNameTextBox.Text,
                    Code = CollegeCodeTextBox.Text,
                    DateTime = System.DateTime.Now,
                    User = _collegeDataService.CurrentUser
                };

                // add the college to the data service
                _collegeDataService.AddCollege(newCollege);

                // show success message
                MessageBox.Show($"Added college {newCollege.Name} ({newCollege.Code})", "Success");

                // clear input fields after successful addition
                CollegeNameTextBox.Text = "";
                CollegeCodeTextBox.Text = "";
            }
            else
            {
                // show error if fields are empty
                MessageBox.Show("Please enter both College Name and College Code.", "Error");
            }
        }

        // validates college name input to allow only letters
        private void CollegeNameTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // regex pattern to match any character that's not a letter
            Regex regex = new Regex("[^a-zA-Z]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        // validates college code input to allow only letters
        private void CollegeCodeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // regex pattern to match any character that's not a letter
            Regex regex = new Regex("[^a-zA-Z]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        // placeholder for future text change handling
        private void CollegeCodeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // currently not implemented
            // can be used for real-time validation or auto-formatting
        }
    }
}