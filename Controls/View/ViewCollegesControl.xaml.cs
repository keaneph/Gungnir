using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Input;
using sis_app.Models;
using sis_app.Services;

namespace sis_app.Controls.View
{
    public partial class ViewCollegesControl : UserControl
    {
        // character limits for college name and code
        private const int MAX_COLLEGE_NAME_LENGTH = 27;
        private const int MAX_COLLEGE_CODE_LENGTH = 9;

        // services for handling data operations
        private CollegeDataService _collegeDataService;
        private ProgramDataService _programDataService;
        private ObservableCollection<College> _colleges;
        private Dictionary<College, College> _originalCollegeData = new Dictionary<College, College>();

        public ViewCollegesControl(CollegeDataService collegeDataService)
        {
            InitializeComponent();
            _collegeDataService = collegeDataService;
            _programDataService = new ProgramDataService("programs.csv");
            _colleges = new ObservableCollection<College>(_collegeDataService.GetAllColleges());
            CollegeListView.ItemsSource = _colleges;

            SortComboBox.SelectedIndex = 0;
        }

        public void LoadColleges()
        {
            List<College> colleges = _collegeDataService.GetAllColleges();
            _colleges.Clear();
            foreach (var college in colleges)
            {
                _colleges.Add(college);
            }
            SortColleges();
        }

        // validates college name input to allow only letters
        private void CollegeNameTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^a-zA-Z]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        // validates college code input to allow only letters
        private void CollegeCodeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^a-zA-Z]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        // handles college name text changes
        private void CollegeNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox.Text.Length > MAX_COLLEGE_NAME_LENGTH)
                {
                    textBox.Text = textBox.Text.Substring(0, MAX_COLLEGE_NAME_LENGTH);
                    textBox.CaretIndex = MAX_COLLEGE_NAME_LENGTH;
                }
            }
        }

        // handles college code text changes and converts to uppercase
        private void CollegeCodeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                // store current caret position
                int caretIndex = textBox.CaretIndex;

                // convert to uppercase
                textBox.Text = textBox.Text.ToUpper();

                // enforce maximum length for college code
                if (textBox.Text.Length > MAX_COLLEGE_CODE_LENGTH)
                {
                    textBox.Text = textBox.Text.Substring(0, MAX_COLLEGE_CODE_LENGTH);
                    caretIndex = MAX_COLLEGE_CODE_LENGTH;
                }

                // restore caret position
                textBox.CaretIndex = caretIndex;
            }
        }

        private void ClearCollegesButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Warning: Clearing colleges will also affect programs using these college codes. Continue?",
                "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // clear both colleges and related programs
                    File.WriteAllText("colleges.csv", string.Empty);
                    File.WriteAllText("programs.csv", string.Empty);
                    LoadColleges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error clearing college data: {ex.Message}", "Error");
                }
            }
        }

        private void EditModeToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            _originalCollegeData.Clear();
            foreach (var college in _colleges)
            {
                _originalCollegeData[college] = new College
                {
                    Name = college.Name,
                    Code = college.Code,
                    DateTime = college.DateTime,
                    User = college.User
                };
            }
        }

        private void EditModeToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var item in CollegeListView.Items)
            {
                if (item is College college)
                {
                    College oldCollege = _originalCollegeData[college];

                    if (oldCollege != null && (college.Name != oldCollege.Name || college.Code != oldCollege.Code))
                    {
                        // validate the edited data
                        if (!ValidateEditedData(college))
                        {
                            // if validation fails, revert changes
                            college.Name = oldCollege.Name;
                            college.Code = oldCollege.Code;
                            continue;
                        }

                        // check for duplicate code (excluding the current college)
                        var existingCollege = _colleges.FirstOrDefault(c =>
                            c != college &&
                            c.Code.Equals(college.Code, StringComparison.OrdinalIgnoreCase));

                        if (existingCollege != null)
                        {
                            MessageBox.Show($"A college with code '{college.Code}' already exists.", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            college.Code = oldCollege.Code;
                            continue;
                        }

                        // update all programs that reference this college code
                        var programs = _programDataService.GetAllPrograms();
                        bool programsUpdated = false;
                        foreach (var program in programs.Where(p =>
                            p.CollegeCode.Equals(oldCollege.Code, StringComparison.OrdinalIgnoreCase)))
                        {
                            program.CollegeCode = college.Code;
                            _programDataService.UpdateProgram(program, program);
                            programsUpdated = true;
                        }

                        _collegeDataService.UpdateCollege(oldCollege, college);

                        // notify user if programs were updated
                        if (programsUpdated)
                        {
                            MessageBox.Show(
                                $"College code updated from '{oldCollege.Code}' to '{college.Code}'. " +
                                "All associated programs have been updated.",
                                "Programs Updated", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
            }
            LoadColleges();
        }

        private void DeleteSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = CollegeListView.SelectedItems.OfType<College>().ToList();
            if (selectedItems.Any())
            {
                // check if any programs use these colleges
                var programs = _programDataService.GetAllPrograms();
                var affectedPrograms = programs.Where(p =>
                    selectedItems.Any(c => c.Code.Equals(p.CollegeCode, StringComparison.OrdinalIgnoreCase))).ToList();

                string message = $"Are you sure you want to delete the selected {selectedItems.Count} colleges?";
                if (affectedPrograms.Any())
                {
                    message += $"\nWarning: {affectedPrograms.Count} programs are using these colleges and will be affected.";
                }

                MessageBoxResult result = MessageBox.Show(message, "Confirmation",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    foreach (var college in selectedItems)
                    {
                        // delete the college and update related programs
                        _collegeDataService.DeleteCollege(college);
                        _colleges.Remove(college);

                        // update programs that used this college
                        foreach (var program in affectedPrograms.Where(p =>
                            p.CollegeCode.Equals(college.Code, StringComparison.OrdinalIgnoreCase)))
                        {
                            program.CollegeCode = "DELETED";
                            _programDataService.UpdateProgram(program, program);
                        }
                    }
                }
                LoadColleges();
            }
            else
            {
                MessageBox.Show("Please select colleges to delete.", "Information",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // rest of your existing sort methods remain the same
        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SortColleges();
        }

        private void SortColleges()
        {
            if (SortComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string sortOption = selectedItem.Content.ToString();

                switch (sortOption)
                {
                    case "Date and Time Modified (Oldest First)":
                        SortList(c => c.DateTime, ListSortDirection.Ascending);
                        break;
                    case "Date and Time Modified (Newest First)":
                        SortList(c => c.DateTime, ListSortDirection.Descending);
                        break;
                    case "Alphabetical College Name":
                        SortList(c => c.Name, ListSortDirection.Ascending);
                        break;
                    case "Alphabetical College Code":
                        SortList(c => c.Code, ListSortDirection.Ascending);
                        break;
                    case "Alphabetical User":
                        SortList(c => c.User, ListSortDirection.Ascending);
                        break;
                }
            }
        }

        private void SortList<TKey>(Func<College, TKey> keySelector, ListSortDirection direction)
        {
            List<College> sortedList;
            if (direction == ListSortDirection.Ascending)
            {
                sortedList = _colleges.OrderBy(keySelector).ToList();
            }
            else
            {
                sortedList = _colleges.OrderByDescending(keySelector).ToList();
            }

            _colleges.Clear();
            foreach (var item in sortedList)
            {
                _colleges.Add(item);
            }
            CollegeListView.Items.Refresh();
        }

        // validates the edited data
        // validates the edited data
        private bool ValidateEditedData(College college)
        {
            // check if college name is empty
            if (string.IsNullOrWhiteSpace(college.Name))
            {
                MessageBox.Show("College name cannot be empty.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // check college name length
            if (college.Name.Length > MAX_COLLEGE_NAME_LENGTH)
            {
                MessageBox.Show($"College name cannot exceed {MAX_COLLEGE_NAME_LENGTH} characters.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // check if college code is empty
            if (string.IsNullOrWhiteSpace(college.Code))
            {
                MessageBox.Show("College code cannot be empty.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // check minimum length for college code
            if (college.Code.Length < 2)
            {
                MessageBox.Show("College code must be at least 2 characters.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // check maximum length for college code
            if (college.Code.Length > MAX_COLLEGE_CODE_LENGTH)
            {
                MessageBox.Show($"College code cannot exceed {MAX_COLLEGE_CODE_LENGTH} characters.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // validate only letters (and spaces for name)
            if (!college.Name.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
            {
                MessageBox.Show("College name can only contain letters and spaces.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!college.Code.All(char.IsLetter))
            {
                MessageBox.Show("College code can only contain letters.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
    }
}