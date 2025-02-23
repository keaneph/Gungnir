using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using sis_app.Models;
using sis_app.Services;

namespace sis_app.Controls.View
{
    public partial class ViewProgramsControl : UserControl
    {
        // character limits for program name and code
        private const int MAX_PROGRAM_NAME_LENGTH = 27;
        private const int MAX_PROGRAM_CODE_LENGTH = 7;

        // services for handling data operations
        private readonly ProgramDataService _programDataService;
        private readonly StudentDataService _studentDataService;
        private readonly CollegeDataService _collegeDataService;
        private ObservableCollection<Program> _programs;
        private Dictionary<Program, Program> _originalProgramData;
        private List<string> _availableCollegeCodes;
        public string CurrentUser { get; set; }

        public ViewProgramsControl(ProgramDataService programDataService, StudentDataService studentDataService)
        {
            InitializeComponent();

            // Initialize services
            _programDataService = programDataService ?? throw new ArgumentNullException(nameof(programDataService));
            _studentDataService = studentDataService ?? throw new ArgumentNullException(nameof(studentDataService));
            _collegeDataService = new CollegeDataService("colleges.csv");

            // Initialize collections
            _programs = new ObservableCollection<Program>();
            _originalProgramData = new Dictionary<Program, Program>();
            _availableCollegeCodes = new List<string>();

            // Set ItemsSource
            ProgramListView.ItemsSource = _programs;

            // Load initial data
            LoadPrograms();
            LoadAvailableCollegeCodes();

            // Set default sort
            if (SortComboBox != null)
            {
                SortComboBox.SelectedIndex = 0;
            }
        }

        // loads available college codes for combobox
        private void LoadAvailableCollegeCodes()
        {
            _availableCollegeCodes = _collegeDataService.GetAllColleges()
                .Select(c => c.Code)
                .OrderBy(code => code)
                .ToList();
        }

        public void LoadPrograms()
        {
            try
            {
                var programs = _programDataService.GetAllPrograms();
                _programs.Clear();
                foreach (var program in programs)
                {
                    _programs.Add(program);
                }
                ProgramListView.ItemsSource = _programs;
                SortPrograms();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading programs: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // validates program name input to allow only letters
        private void ProgramNameTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^a-zA-Z]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        // validates program code input to allow only letters
        private void ProgramCodeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^a-zA-Z]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        // handles program name text changes
        private void ProgramNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox.Text.Length > MAX_PROGRAM_NAME_LENGTH)
                {
                    textBox.Text = textBox.Text.Substring(0, MAX_PROGRAM_NAME_LENGTH);
                    textBox.CaretIndex = MAX_PROGRAM_NAME_LENGTH;
                }
            }
        }

        // handles program code text changes
        private void ProgramCodeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                int caretIndex = textBox.CaretIndex;
                string newText = textBox.Text.ToUpper();

                // Remove any invalid characters
                newText = new string(newText.Where(c => char.IsLetter(c)).ToArray());

                if (newText.Length > MAX_PROGRAM_CODE_LENGTH)
                {
                    newText = newText.Substring(0, MAX_PROGRAM_CODE_LENGTH);
                    caretIndex = MAX_PROGRAM_CODE_LENGTH;
                }

                textBox.Text = newText;
                textBox.CaretIndex = Math.Min(caretIndex, newText.Length);
            }
        }

        private void ClearProgramsButton_Click(object sender, RoutedEventArgs e)
        {
            var students = _studentDataService.GetAllStudents();
            var affectedStudents = students.Where(s => s.ProgramCode != "DELETED").ToList();

            string message = "Are you sure you want to clear all program data?";
            if (affectedStudents.Any())
            {
                message += $"\nWarning: {affectedStudents.Count} students will be affected.";
            }

            MessageBoxResult result = MessageBox.Show(message, "Confirmation",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Update all affected students
                    foreach (var student in affectedStudents)
                    {
                        student.ProgramCode = "DELETED";
                        student.CollegeCode = "DELETED";
                        _studentDataService.UpdateStudent(student, student);
                    }

                    File.WriteAllText("programs.csv", string.Empty);
                    LoadPrograms();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error clearing program data: {ex.Message}", "Error");
                }
            }
        }

        private void EditModeToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            // refresh available college codes
            LoadAvailableCollegeCodes();

            // store original data
            _originalProgramData.Clear();
            foreach (var program in _programs)
            {
                _originalProgramData[program] = new Program
                {
                    Name = program.Name,
                    Code = program.Code,
                    CollegeCode = program.CollegeCode,
                    DateTime = program.DateTime,
                    User = program.User
                };
            }

            // update comboboxes in listview
            if (ProgramListView.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
            {
                foreach (var program in _programs)
                {
                    var container = ProgramListView.ItemContainerGenerator.ContainerFromItem(program) as ListViewItem;
                    if (container != null)
                    {
                        var collegeCodeComboBox = FindVisualChild<ComboBox>(container);
                        if (collegeCodeComboBox != null)
                        {
                            collegeCodeComboBox.ItemsSource = _availableCollegeCodes;
                            collegeCodeComboBox.SelectedItem = program.CollegeCode;
                        }
                    }
                }
            }
        }

        private void EditModeToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var program in _programs.ToList())
                {
                    if (!_originalProgramData.TryGetValue(program, out Program originalProgram))
                        continue;

                    bool isChanged = program.Name != originalProgram.Name ||
                                   program.Code != originalProgram.Code ||
                                   program.CollegeCode != originalProgram.CollegeCode;

                    if (!isChanged)
                        continue;

                    // Validate the edited data
                    if (!ValidateEditedData(program))
                    {
                        RevertChanges(program, originalProgram);
                        continue;
                    }

                    // Check for duplicate code
                    if (IsDuplicateProgramCode(program, originalProgram))
                    {
                        RevertChanges(program, originalProgram);
                        continue;
                    }

                    // Update students if program code has changed
                    if (!string.Equals(program.Code, originalProgram.Code, StringComparison.OrdinalIgnoreCase))
                    {
                        UpdateStudentsForProgramCodeChange(originalProgram.Code, program.Code, program.CollegeCode);
                    }
                    // Update students if only college code has changed
                    else if (!string.Equals(program.CollegeCode, originalProgram.CollegeCode, StringComparison.OrdinalIgnoreCase))
                    {
                        UpdateStudentsForCollegeCodeChange(program.Code, program.CollegeCode);
                    }

                    // Update the program in the database
                    _programDataService.UpdateProgram(originalProgram, program);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating programs: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoadPrograms();
            }
        }

        private void RevertChanges(Program program, Program originalProgram)
        {
            program.Name = originalProgram.Name;
            program.Code = originalProgram.Code;
            program.CollegeCode = originalProgram.CollegeCode;
        }

        private bool IsDuplicateProgramCode(Program program, Program originalProgram)
        {
            var existingProgram = _programs.FirstOrDefault(p =>
                p != program &&
                p.Code.Equals(program.Code, StringComparison.OrdinalIgnoreCase));

            if (existingProgram != null)
            {
                MessageBox.Show($"A program with code '{program.Code}' already exists.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return true;
            }
            return false;
        }

        private void UpdateStudentsForProgramCodeChange(string oldCode, string newCode, string newCollegeCode)
        {
            var students = _studentDataService.GetAllStudents()
                .Where(s => s.ProgramCode.Equals(oldCode, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (students.Any())
            {
                MessageBox.Show(
                    $"Program code changed from '{oldCode}' to '{newCode}'. " +
                    $"{students.Count} students will be updated.",
                    "Students Affected",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                foreach (var student in students)
                {
                    student.ProgramCode = newCode;
                    student.CollegeCode = newCollegeCode;
                    _studentDataService.UpdateStudent(student, student);
                }
            }
        }

        private void UpdateStudentsForCollegeCodeChange(string programCode, string newCollegeCode)
        {
            var students = _studentDataService.GetAllStudents()
                .Where(s => s.ProgramCode.Equals(programCode, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var student in students)
            {
                student.CollegeCode = newCollegeCode;
                _studentDataService.UpdateStudent(student, student);
            }
        }

        private bool ValidateProgramCode(string programCode)
        {
            if (string.IsNullOrWhiteSpace(programCode))
                return false;

            if (programCode.Length < 2 || programCode.Length > MAX_PROGRAM_CODE_LENGTH)
                return false;

            return programCode.All(char.IsLetter);
        }

        // validates the edited data
        private bool ValidateEditedData(Program program)
        {
            // check if program name is empty
            if (string.IsNullOrWhiteSpace(program.Name))
            {
                MessageBox.Show("Program name cannot be empty.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // check program name length
            if (program.Name.Length > MAX_PROGRAM_NAME_LENGTH)
            {
                MessageBox.Show($"Program name cannot exceed {MAX_PROGRAM_NAME_LENGTH} characters.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // check if program code is empty
            if (string.IsNullOrWhiteSpace(program.Code))
            {
                MessageBox.Show("Program code cannot be empty.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // check minimum length for program code
            if (program.Code.Length < 2)
            {
                MessageBox.Show("Program code must be at least 2 characters.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // check maximum length for program code
            if (program.Code.Length > MAX_PROGRAM_CODE_LENGTH)
            {
                MessageBox.Show($"Program code cannot exceed {MAX_PROGRAM_CODE_LENGTH} characters.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // validate only letters (and spaces for name)
            if (!program.Name.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
            {
                MessageBox.Show("Program name can only contain letters and spaces.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!program.Code.All(char.IsLetter))
            {
                MessageBox.Show("Program code can only contain letters.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // validate college code exists
            if (!_availableCollegeCodes.Contains(program.CollegeCode, StringComparer.OrdinalIgnoreCase))
            {
                MessageBox.Show($"College code '{program.CollegeCode}' does not exist.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        // helper method to find visual child
        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T found)
                    return found;
                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }

        // rest of your existing methods remain the same
        private void DeleteSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = ProgramListView.SelectedItems.OfType<Program>().ToList();
            if (selectedItems.Any())
            {
                // Check for affected students
                var students = _studentDataService.GetAllStudents();
                var affectedStudents = students.Where(s =>
                    selectedItems.Any(p => p.Code.Equals(s.ProgramCode, StringComparison.OrdinalIgnoreCase))).ToList();

                string message = $"Are you sure you want to delete the selected {selectedItems.Count} programs?";
                if (affectedStudents.Any())
                {
                    message += $"\nWarning: {affectedStudents.Count} students are enrolled in these programs and will be affected.";
                }

                MessageBoxResult result = MessageBox.Show(message, "Confirmation",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    foreach (var program in selectedItems)
                    {
                        // Update affected students
                        foreach (var student in affectedStudents.Where(s =>
                            s.ProgramCode.Equals(program.Code, StringComparison.OrdinalIgnoreCase)))
                        {
                            student.ProgramCode = "DELETED";
                            student.CollegeCode = "DELETED";
                            _studentDataService.UpdateStudent(student, student);
                        }

                        _programDataService.DeleteProgram(program);
                        _programs.Remove(program);
                    }
                    LoadPrograms();
                }
            }
            else
            {
                MessageBox.Show("Please select programs to delete.", "Information",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // your existing sort methods remain the same
        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SortPrograms();
        }

        private void SortPrograms()
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
                        SortList(p => p.DateTime, ListSortDirection.Descending);
                        break;
                    case "Alphabetical Program Name":
                        SortList(p => p.Name, ListSortDirection.Ascending);
                        break;
                    case "Alphabetical Program Code":
                        SortList(p => p.Code, ListSortDirection.Ascending);
                        break;
                    case "Alphabetical College Code":
                        SortList(p => p.CollegeCode, ListSortDirection.Ascending);
                        break;
                    case "Alphabetical User":
                        SortList(p => p.User, ListSortDirection.Ascending);
                        break;
                }
            }
        }

        private void SortList<TKey>(Func<Program, TKey> keySelector, ListSortDirection direction)
        {
            List<Program> sortedList;
            if (direction == ListSortDirection.Ascending)
            {
                sortedList = _programs.OrderBy(keySelector).ToList();
            }
            else
            {
                sortedList = _programs.OrderByDescending(keySelector).ToList();
            }

            _programs.Clear();
            foreach (var item in sortedList)
            {
                _programs.Add(item);
            }
            ProgramListView.Items.Refresh();
        }
    }
}