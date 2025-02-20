using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace glaive
{
    public partial class AddStudentControl : UserControl
    {
        private StudentDataService _studentDataService;
        private CollegeDataService _collegeDataService;
        private ProgramDataService _programDataService;

        // Change to use proper event args
        public event EventHandler<StudentEventArgs> StudentAdded;

        public AddStudentControl(StudentDataService studentDataService, CollegeDataService collegeDataService, ProgramDataService programDataService)
        {
            InitializeComponent();
            _studentDataService = studentDataService;
            _collegeDataService = collegeDataService;
            _programDataService = programDataService;
        }

        private void AddStudentButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                try
                {
                    string idNumber = $"{YearTextBox.Text}-{NumberTextBox.Text}";

                    // Check for duplicate ID Number
                    var existingStudents = _studentDataService.GetAllStudents();
                    if (existingStudents.Exists(s => s.IDNumber.Equals(idNumber, StringComparison.OrdinalIgnoreCase)))
                    {
                        MessageBox.Show($"A student with ID Number '{idNumber}' already exists.", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var yearLevelContent = ((ComboBoxItem)YearLevelComboBox.SelectedItem).Content.ToString();
                    var genderContent = ((ComboBoxItem)GenderComboBox.SelectedItem).Content.ToString();

                    Student newStudent = new Student
                    {
                        IDNumber = idNumber,
                        FirstName = FirstNameTextBox.Text.Trim(),
                        LastName = LastNameTextBox.Text.Trim(),
                        YearLevel = int.Parse(yearLevelContent),
                        Gender = genderContent,
                        ProgramCode = ProgramCodeComboBox.SelectedItem?.ToString(),
                        CollegeCode = CollegeCodeComboBox.SelectedItem.ToString(),
                        DateTime = DateTime.Now,
                        User = _studentDataService.CurrentUser
                    };

                    _studentDataService.AddStudent(newStudent);

                    // Raise event with proper event args
                    StudentAdded?.Invoke(this, new StudentEventArgs(newStudent));

                    MessageBox.Show($"Successfully added student:\nName: {newStudent.FirstName} {newStudent.LastName}\nID: {newStudent.IDNumber}\nProgram: {newStudent.ProgramCode}",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    ClearFields();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding student: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ValidateInput()
        {
            var validationMessages = new List<string>();

            // Year validation
            if (string.IsNullOrWhiteSpace(YearTextBox.Text))
                validationMessages.Add("Please enter a year.");
            else if (YearTextBox.Text.Length != 4)
                validationMessages.Add("Year must be 4 digits.");
            else if (!int.TryParse(YearTextBox.Text, out int year) || year < 2000 || year > DateTime.Now.Year)
                validationMessages.Add($"Year must be between 2000 and {DateTime.Now.Year}.");

            // Number validation
            if (string.IsNullOrWhiteSpace(NumberTextBox.Text))
                validationMessages.Add("Please enter a student number.");
            else if (NumberTextBox.Text.Length != 4)
                validationMessages.Add("Student number must be 4 digits.");

            // Name validation
            if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text))
                validationMessages.Add("Please enter a first name.");
            if (string.IsNullOrWhiteSpace(LastNameTextBox.Text))
                validationMessages.Add("Please enter a last name.");

            // ComboBox validation
            if (YearLevelComboBox.SelectedItem == null)
                validationMessages.Add("Please select a year level.");
            if (GenderComboBox.SelectedItem == null)
                validationMessages.Add("Please select a gender.");
            if (ProgramCodeComboBox.SelectedItem == null)
                validationMessages.Add("Please select a program code.");
            if (CollegeCodeComboBox.SelectedItem == null)
                validationMessages.Add("Please select a college code.");

            if (validationMessages.Any())
            {
                MessageBox.Show(
                    string.Join("\n", validationMessages),
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return false;
            }

            return true;
        }

        public void LoadProgramCodes()
        {
            try
            {
                var programCodes = _programDataService.GetAllPrograms()
                    .OrderBy(p => p.Code)
                    .Select(p => p.Code)
                    .ToList();

                if (!programCodes.Any())
                {
                    MessageBox.Show("No programs found. Please add a program first.",
                        "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                ProgramCodeComboBox.ItemsSource = programCodes;

                ResetCollegeCode();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading program codes: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetCollegeCode()
        {
            CollegeCodeComboBox.Items.Clear();
            CollegeCodeComboBox.Items.Add("Select College");
            CollegeCodeComboBox.SelectedIndex = 0;
            CollegeCodeComboBox.IsEnabled = false;
        }

        private void ClearFields()
        {
            YearTextBox.Clear();
            NumberTextBox.Clear();
            FirstNameTextBox.Clear();
            LastNameTextBox.Clear();
            YearLevelComboBox.SelectedIndex = -1;
            GenderComboBox.SelectedIndex = -1;
            ProgramCodeComboBox.SelectedIndex = -1;
            ResetCollegeCode();
        }

        private void FirstNameTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow letters and spaces
            Regex regex = new Regex("[^a-zA-Z ]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void LastNameTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow letters and spaces
            Regex regex = new Regex("[^a-zA-Z ]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void YearTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            if (regex.IsMatch(e.Text) || YearTextBox.Text.Length >= 4)
            {
                e.Handled = true;
            }
        }

        private void NumberTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            if (regex.IsMatch(e.Text) || NumberTextBox.Text.Length >= 4)
            {
                e.Handled = true;
            }
        }

        private void ProgramCodeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProgramCodeComboBox.SelectedItem != null)
            {
                try
                {
                    string selectedProgramCode = ProgramCodeComboBox.SelectedItem.ToString();
                    var selectedProgram = _programDataService.GetAllPrograms()
                        .FirstOrDefault(p => p.Code == selectedProgramCode);

                    if (selectedProgram != null)
                    {
                        CollegeCodeComboBox.IsEnabled = true;
                        CollegeCodeComboBox.Items.Clear();
                        CollegeCodeComboBox.Items.Add(selectedProgram.CollegeCode);
                        CollegeCodeComboBox.SelectedIndex = 0;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading college code: {ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    ResetCollegeCode();
                }
            }
            else
            {
                ResetCollegeCode();
            }
        }
    }
}