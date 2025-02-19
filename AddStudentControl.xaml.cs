﻿using System;
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

        public AddStudentControl(StudentDataService studentDataService, CollegeDataService collegeDataService, ProgramDataService programDataService)
        {
            InitializeComponent();
            _studentDataService = studentDataService;
            _collegeDataService = collegeDataService;
            _programDataService = programDataService;
        }

        private void AddStudentButton_Click(object sender, RoutedEventArgs e)
        {
            if (AreAllFieldsValid())
            {
                string idNumber = $"{YearTextBox.Text}-{NumberTextBox.Text}";

                Student newStudent = new Student
                {
                    IDNumber = idNumber,
                    FirstName = FirstNameTextBox.Text,
                    LastName = LastNameTextBox.Text,
                    YearLevel = int.Parse(YearLevelComboBox.SelectedItem.ToString()),
                    Gender = GenderComboBox.SelectedItem.ToString(),
                    ProgramCode = ProgramCodeComboBox.SelectedItem?.ToString(),
                    CollegeCode = CollegeCodeComboBox.SelectedItem.ToString(),
                    DateTime = DateTime.Now,
                    User = _studentDataService.CurrentUser
                };

                try
                {
                    _studentDataService.AddStudent(newStudent);
                    MessageBox.Show($"Added student {newStudent.FirstName} {newStudent.LastName} ({newStudent.IDNumber})", "Success");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding student: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                // Clear fields
                YearTextBox.Clear();
                NumberTextBox.Clear();
                FirstNameTextBox.Clear();
                LastNameTextBox.Clear();
                YearLevelComboBox.SelectedIndex = -1;
                GenderComboBox.SelectedIndex = -1;
                ProgramCodeComboBox.SelectedIndex = -1;
                CollegeCodeComboBox.SelectedIndex = -1;
            }
            else
            {
                MessageBox.Show("Please fill all fields correctly.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool AreAllFieldsValid()
        {
            if (string.IsNullOrWhiteSpace(YearTextBox.Text) ||
                string.IsNullOrWhiteSpace(NumberTextBox.Text) ||
                !int.TryParse(YearTextBox.Text, out _) ||
                YearTextBox.Text.Length != 4 ||
                !int.TryParse(NumberTextBox.Text, out _) ||
                string.IsNullOrWhiteSpace(FirstNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(LastNameTextBox.Text) ||
                GenderComboBox.SelectedItem == null ||
                ProgramCodeComboBox.SelectedItem == null ||
                CollegeCodeComboBox.SelectedItem == null ||
                YearLevelComboBox.SelectedItem == null)
            {
                return false;
            }

            return true;
        }

        public void LoadProgramCodes()
        {
            var programCodes = _programDataService.GetAllPrograms().Select(p => p.Code).ToList();
            ProgramCodeComboBox.ItemsSource = programCodes;

            CollegeCodeComboBox.Items.Clear();
            CollegeCodeComboBox.Items.Add("Select College");
            CollegeCodeComboBox.SelectedIndex = 0;
            CollegeCodeComboBox.IsEnabled = false;
        }

        private void FirstNameTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^a-zA-Z]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void LastNameTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^a-zA-Z]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
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
                string selectedProgramCode = ProgramCodeComboBox.SelectedItem.ToString();
                var selectedProgram = _programDataService.GetAllPrograms().FirstOrDefault(p => p.Code == selectedProgramCode);
                if (selectedProgram != null)
                {
                    CollegeCodeComboBox.IsEnabled = true;
                    CollegeCodeComboBox.Items.Clear();
                    CollegeCodeComboBox.Items.Add(selectedProgram.CollegeCode);
                    CollegeCodeComboBox.SelectedIndex = 0;
                }
            }
        }
    }
}