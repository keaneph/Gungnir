using System.Windows;
using System.Windows.Controls;

namespace glaive.Views
{
    public partial class AddCollegeControl : UserControl
    {
        private CollegeDataService _collegeDataService;

        public AddCollegeControl(CollegeDataService collegeDataService)
        {
            InitializeComponent();
            _collegeDataService = collegeDataService;
        }

        private void AddCollegeButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(CollegeNameTextBox.Text) && !string.IsNullOrEmpty(CollegeCodeTextBox.Text))
            {
                College newCollege = new College { Name = CollegeNameTextBox.Text, Code = CollegeCodeTextBox.Text };
                _collegeDataService.AddCollege(newCollege);

                MessageBox.Show($"Added college {newCollege.Name} ({newCollege.Code})", "Success");

                CollegeNameTextBox.Text = "";
                CollegeCodeTextBox.Text = "";

                // You might add a way to notify the ViewCollegesControl to refresh the list.
            }
            else
            {
                MessageBox.Show("Please enter both College Name and College Code.", "Error");
            }
        }
    }
}