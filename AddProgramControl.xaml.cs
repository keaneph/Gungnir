using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;

namespace glaive
{
    public partial class AddProgramControl : UserControl
    {
        private ProgramDataService _programDataService;
        private CollegeDataService _collegeDataService;

        // Add event for history tracking
        public event EventHandler<ProgramEventArgs> ProgramAdded;

        public AddProgramControl(ProgramDataService programDataService, CollegeDataService collegeDataService)
        {
            InitializeComponent();
            _programDataService = programDataService;
            _collegeDataService = collegeDataService;
            LoadCollegeCodes();
        }

        public void LoadCollegeCodes()
        {
            try
            {
                List<string> collegeCodes = _collegeDataService.GetAllColleges()
                    .OrderBy(c => c.Code)
                    .Select(c => c.Code)
                    .ToList();

                CollegeCodeComboBox.ItemsSource = collegeCodes;

                if (collegeCodes.Count == 0)
                {
                    MessageBox.Show("No colleges found. Please add a college first.", "Information",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading college codes: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddProgramButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                try
                {
                    Program newProgram = new Program
                    {
                        Name = ProgramNameTextBox.Text.Trim(),
                        Code = ProgramCodeTextBox.Text.Trim(),
                        CollegeCode = CollegeCodeComboBox.SelectedItem.ToString(),
                        DateTime = DateTime.Now,
                        User = _programDataService.CurrentUser
                    };

                    // Check for duplicate program code
                    var existingPrograms = _programDataService.GetAllPrograms();
                    if (existingPrograms.Exists(p => p.Code.Equals(newProgram.Code, StringComparison.OrdinalIgnoreCase)))
                    {
                        MessageBox.Show($"A program with code '{newProgram.Code}' already exists.", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    _programDataService.AddProgram(newProgram);

                    // Raise event for history tracking
                    ProgramAdded?.Invoke(this, new ProgramEventArgs(newProgram));

                    MessageBox.Show($"Added program {newProgram.Name} ({newProgram.Code})", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    ClearFields();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding program: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(ProgramNameTextBox.Text))
            {
                MessageBox.Show("Please enter a program name.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                ProgramNameTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(ProgramCodeTextBox.Text))
            {
                MessageBox.Show("Please enter a program code.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                ProgramCodeTextBox.Focus();
                return false;
            }

            if (ProgramCodeTextBox.Text.Length < 2 || ProgramCodeTextBox.Text.Length > 5)
            {
                MessageBox.Show("Program code must be between 2 and 5 characters.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                ProgramCodeTextBox.Focus();
                return false;
            }

            if (CollegeCodeComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a college code.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                CollegeCodeComboBox.Focus();
                return false;
            }

            return true;
        }

        private void ClearFields()
        {
            ProgramNameTextBox.Text = "";
            ProgramCodeTextBox.Text = "";
            CollegeCodeComboBox.SelectedIndex = -1;
        }

        private void ProgramNameTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow letters and spaces
            Regex regex = new Regex("[^a-zA-Z ]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void ProgramCodeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow only letters
            Regex regex = new Regex("[^a-zA-Z]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void ProgramCodeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Convert to uppercase
            if (sender is TextBox textBox)
            {
                int caretIndex = textBox.CaretIndex;
                textBox.Text = textBox.Text.ToUpper();
                textBox.CaretIndex = caretIndex;
            }
        }
    }

}