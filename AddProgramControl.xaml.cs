using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Generic; // For List<T>
using System.Linq; // For LINQ

namespace glaive
{
    public partial class AddProgramControl : UserControl
    {
        private ProgramDataService _programDataService; // New data service
        private CollegeDataService _collegeDataService;

        public AddProgramControl(ProgramDataService programDataService, CollegeDataService collegeDataService)  // Inject both services
        {
            InitializeComponent();
            _programDataService = programDataService;
            _collegeDataService = collegeDataService;
            LoadCollegeCodes();
        }

        public void LoadCollegeCodes()
        {
            List<string> collegeCodes = _collegeDataService.GetAllColleges().Select(c => c.Code).ToList();
            CollegeCodeComboBox.ItemsSource = collegeCodes;
        }


        private void AddProgramButton_Click(object sender, RoutedEventArgs e)
        {
            // ... (Similar validation logic as AddCollegeButton_Click) ...
            if (!string.IsNullOrEmpty(ProgramNameTextBox.Text) &&
               !string.IsNullOrEmpty(ProgramCodeTextBox.Text) &&
               CollegeCodeComboBox.SelectedItem != null) // Check if college code selected
            {
                Program newProgram = new Program
                {
                    Name = ProgramNameTextBox.Text,
                    Code = ProgramCodeTextBox.Text,
                    CollegeCode = CollegeCodeComboBox.SelectedItem.ToString(), // Get selected college code
                    DateTime = DateTime.Now,
                    User = _programDataService.CurrentUser //_collegeDataService.CurrentUser should be handled in a common logic / injection
                };


                _programDataService.AddProgram(newProgram);

                MessageBox.Show($"Added college {newProgram.Name} ({newProgram.Code})", "Success");

                ProgramNameTextBox.Text = "";
                ProgramCodeTextBox.Text = "";
            }
            else
            {
                MessageBox.Show("Please enter Program Name, Code and College Code.", "Error");
            }
        }

        private void ProgramNameTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^a-zA-Z]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void ProgramCodeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^a-zA-Z]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}