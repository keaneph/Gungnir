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
    public partial class ViewStudentControl : UserControl
    {
        // Character limits for text fields
        private const int MAX_FIRSTNAME_LENGTH = 26;
        private const int MAX_LASTNAME_LENGTH = 14;

        // Services for handling data operations
        private readonly StudentDataService _studentDataService;
        private readonly ProgramDataService _programDataService;
        private readonly CollegeDataService _collegeDataService;
        private ObservableCollection<Student> _students;
        private Dictionary<Student, Student> _originalStudentData = new Dictionary<Student, Student>();

        // Lists for combobox items
        public List<string> _availableProgramCodes { get; private set; }
        public List<int> _yearLevels { get; } = new List<int> { 1, 2, 3, 4 };
        public List<string> _genders { get; } = new List<string> { "Male", "Female" };

        public ObservableCollection<Student> Students
        {
            get { return _students; }
        }

        public List<string> AvailableProgramCodes
        {
            get { return _availableProgramCodes; }
        }

        public ViewStudentControl(
            StudentDataService studentDataService,
            ProgramDataService programDataService,
            CollegeDataService collegeDataService)
        {
            InitializeComponent();
            _studentDataService = studentDataService;
            _programDataService = programDataService;
            _collegeDataService = collegeDataService;
            _students = new ObservableCollection<Student>(_studentDataService.GetAllStudents());
            StudentListView.ItemsSource = _students;

            this.DataContext = this;
            LoadProgramCodes();
            SortComboBox.SelectedIndex = 0;
        }

        private void LoadProgramCodes()
        {
            _availableProgramCodes = _programDataService.GetAllPrograms()
                .Select(p => p.Code)
                .OrderBy(code => code)
                .ToList();
        }

        public void LoadStudents()
        {
            try
            {
                List<Student> students = _studentDataService.GetAllStudents();
                var programs = _programDataService.GetAllPrograms().ToDictionary(
                    p => p.Code.ToUpper(),
                    StringComparer.OrdinalIgnoreCase);

                _students.Clear();
                foreach (var student in students)
                {
                    if (programs.TryGetValue(student.ProgramCode, out var program))
                    {
                        student.CollegeCode = program.CollegeCode;
                    }
                    else if (student.ProgramCode != "DELETED")
                    {
                        student.ProgramCode = "DELETED";
                        student.CollegeCode = "DELETED";
                        _studentDataService.UpdateStudent(student, student);
                    }

                    _students.Add(student);
                }
                SortStudents();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading students: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HandleProgramDeletion(string programCode)
        {
            var affectedStudents = _students.Where(s =>
                s.ProgramCode.Equals(programCode, StringComparison.OrdinalIgnoreCase)).ToList();

            if (affectedStudents.Any())
            {
                MessageBox.Show(
                    $"Warning: {affectedStudents.Count} students are enrolled in program '{programCode}'. " +
                    "Their program code will be marked as 'DELETED'.",
                    "Students Affected",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                foreach (var student in affectedStudents)
                {
                    student.ProgramCode = "DELETED";
                    student.CollegeCode = "DELETED";
                    _studentDataService.UpdateStudent(student, student);
                }
            }
        }

        private void HandleCollegeCodeChange(string oldCollegeCode, string newCollegeCode)
        {
            var affectedPrograms = _programDataService.GetAllPrograms()
                .Where(p => p.CollegeCode.Equals(oldCollegeCode, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var program in affectedPrograms)
            {
                var affectedStudents = _students.Where(s =>
                    s.ProgramCode.Equals(program.Code, StringComparison.OrdinalIgnoreCase)).ToList();

                foreach (var student in affectedStudents)
                {
                    student.CollegeCode = newCollegeCode;
                    _studentDataService.UpdateStudent(student, student);
                }
            }
        }

        private void HandleCollegeDeletion(string collegeCode)
        {
            var affectedPrograms = _programDataService.GetAllPrograms()
                .Where(p => p.CollegeCode.Equals(collegeCode, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var program in affectedPrograms)
            {
                HandleProgramDeletion(program.Code);
            }
        }

        private void ClearStudentsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to clear all student data?",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    File.WriteAllText("students.csv", string.Empty);
                    LoadStudents();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error clearing student data: {ex.Message}", "Error");
                }
            }
        }

        private void EditModeToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            // Refresh available program codes
            LoadProgramCodes();

            _originalStudentData.Clear();
            foreach (var student in _students)
            {
                _originalStudentData[student] = new Student
                {
                    IDNumber = student.IDNumber,
                    FirstName = student.FirstName,
                    LastName = student.LastName,
                    YearLevel = student.YearLevel,
                    Gender = student.Gender,
                    ProgramCode = student.ProgramCode,
                    CollegeCode = student.CollegeCode,
                    DateTime = student.DateTime,
                    User = student.User
                };
            }

            // Update comboboxes in listview
            if (StudentListView.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
            {
                foreach (var student in _students)
                {
                    var container = StudentListView.ItemContainerGenerator.ContainerFromItem(student) as ListViewItem;
                    if (container != null)
                    {
                        var comboBoxes = new List<ComboBox>();
                        FindVisualChildren<ComboBox>(container, comboBoxes);

                        foreach (var comboBox in comboBoxes)
                        {
                            var binding = comboBox.GetBindingExpression(ComboBox.SelectedItemProperty);
                            if (binding != null)
                            {
                                string bindingPath = binding.ParentBinding.Path.Path;

                                switch (bindingPath)
                                {
                                    case "YearLevel":
                                        comboBox.ItemsSource = _yearLevels;
                                        comboBox.SelectedItem = student.YearLevel;
                                        break;
                                    case "Gender":
                                        comboBox.ItemsSource = _genders;
                                        comboBox.SelectedItem = student.Gender;
                                        break;
                                    case "ProgramCode":
                                        comboBox.ItemsSource = _availableProgramCodes;
                                        comboBox.SelectedItem = student.ProgramCode;
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }
        private void EditModeToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var item in StudentListView.Items)
            {
                if (item is Student student)
                {
                    Student oldStudent = _originalStudentData[student];
                    if (oldStudent != null &&
                        (student.FirstName != oldStudent.FirstName ||
                         student.LastName != oldStudent.LastName ||
                         student.IDNumber != oldStudent.IDNumber ||
                         student.YearLevel != oldStudent.YearLevel ||
                         student.Gender != oldStudent.Gender ||
                         student.ProgramCode != oldStudent.ProgramCode))
                    {
                        // Validate the edited data
                        if (!ValidateEditedData(student))
                        {
                            // If validation fails, revert changes
                            student.FirstName = oldStudent.FirstName;
                            student.LastName = oldStudent.LastName;
                            student.IDNumber = oldStudent.IDNumber;
                            student.YearLevel = oldStudent.YearLevel;
                            student.Gender = oldStudent.Gender;
                            student.ProgramCode = oldStudent.ProgramCode;
                            student.CollegeCode = oldStudent.CollegeCode;
                            continue;
                        }

                        // Update college code based on program code
                        if (student.ProgramCode != "DELETED")
                        {
                            var program = _programDataService.GetAllPrograms()
                                .FirstOrDefault(p => p.Code.Equals(student.ProgramCode, StringComparison.OrdinalIgnoreCase));
                            if (program != null)
                            {
                                student.CollegeCode = program.CollegeCode;
                            }
                        }

                        _studentDataService.UpdateStudent(oldStudent, student);
                    }
                }
            }
            LoadStudents();
        }

        private bool ValidateEditedData(Student student)
        {
            // Validate ID Number
            if (string.IsNullOrWhiteSpace(student.IDNumber))
            {
                MessageBox.Show("ID Number cannot be empty.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Check for duplicate ID number
            var existingStudent = _students.FirstOrDefault(s =>
                s != student &&
                s.IDNumber.Equals(student.IDNumber, StringComparison.OrdinalIgnoreCase));
            if (existingStudent != null)
            {
                MessageBox.Show($"A student with ID number '{student.IDNumber}' already exists.",
                    "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Validate names
            if (string.IsNullOrWhiteSpace(student.FirstName))
            {
                MessageBox.Show("First name cannot be empty.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (student.FirstName.Length > MAX_FIRSTNAME_LENGTH)
            {
                MessageBox.Show($"First name cannot exceed {MAX_FIRSTNAME_LENGTH} characters.",
                    "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(student.LastName))
            {
                MessageBox.Show("Last name cannot be empty.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (student.LastName.Length > MAX_LASTNAME_LENGTH)
            {
                MessageBox.Show($"Last name cannot exceed {MAX_LASTNAME_LENGTH} characters.",
                    "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Validate names contain only letters
            if (!student.FirstName.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)) ||
                !student.LastName.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
            {
                MessageBox.Show("Names can only contain letters and spaces.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Validate program code and its relationships
            if (student.ProgramCode != "DELETED")
            {
                var program = _programDataService.GetAllPrograms()
                    .FirstOrDefault(p => p.Code.Equals(student.ProgramCode, StringComparison.OrdinalIgnoreCase));

                if (program == null)
                {
                    MessageBox.Show($"Program code '{student.ProgramCode}' does not exist.",
                        "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                // Validate that the program's college exists
                var college = _collegeDataService.GetAllColleges()
                    .FirstOrDefault(c => c.Code.Equals(program.CollegeCode, StringComparison.OrdinalIgnoreCase));

                if (college == null)
                {
                    MessageBox.Show($"The college for program '{student.ProgramCode}' does not exist.",
                        "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            // Validate ID number format (YYYY-NNNN)
            var idParts = student.IDNumber.Split('-');
            if (idParts.Length != 2 ||
                !int.TryParse(idParts[0], out int year) ||
                !int.TryParse(idParts[1], out int number) ||
                year < 2016 || year > 2025 ||
                number < 1 || number > 9999)
            {
                MessageBox.Show("ID Number must be in format YYYY-NNNN where:\nYYYY is between 2016-2025\nNNNN is between 0001-9999",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
        private void IDNumberTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9-]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void IDNumberTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                string text = textBox.Text;
                int caretIndex = textBox.CaretIndex;

                if (text.Contains("-"))
                {
                    var parts = text.Split('-');
                    if (parts.Length == 2)
                    {
                        if (parts[0].Length > 4)
                        {
                            parts[0] = parts[0].Substring(0, 4);
                        }

                        if (parts[1].Length > 4)
                        {
                            parts[1] = parts[1].Substring(0, 4);
                        }

                        textBox.Text = $"{parts[0]}-{parts[1]}";
                        textBox.CaretIndex = Math.Min(caretIndex, textBox.Text.Length);
                    }
                }
                else if (text.Length > 4 && !text.Contains("-"))
                {
                    textBox.Text = $"{text.Substring(0, 4)}-{text.Substring(4)}";
                    textBox.CaretIndex = textBox.Text.Length;
                }
            }
        }

        private void ProgramCodeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem != null)
            {
                string selectedProgramCode = comboBox.SelectedItem.ToString();

                if (ValidateProgramCodeChange(selectedProgramCode))
                {
                    var program = _programDataService.GetAllPrograms()
                        .FirstOrDefault(p => p.Code.Equals(selectedProgramCode, StringComparison.OrdinalIgnoreCase));

                    if (program != null && comboBox.DataContext is Student student)
                    {
                        student.CollegeCode = program.CollegeCode;
                    }
                }
                else
                {
                    // Revert selection if validation fails
                    if (comboBox.DataContext is Student student)
                    {
                        comboBox.SelectedItem = student.ProgramCode;
                    }
                }
            }
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

        private void FirstNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox.Text.Length > MAX_FIRSTNAME_LENGTH)
                {
                    textBox.Text = textBox.Text.Substring(0, MAX_FIRSTNAME_LENGTH);
                    textBox.CaretIndex = MAX_FIRSTNAME_LENGTH;
                }
            }
        }

        private void LastNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox.Text.Length > MAX_LASTNAME_LENGTH)
                {
                    textBox.Text = textBox.Text.Substring(0, MAX_LASTNAME_LENGTH);
                    textBox.CaretIndex = MAX_LASTNAME_LENGTH;
                }
            }
        }

        private void DeleteSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = StudentListView.SelectedItems.OfType<Student>().ToList();
            if (selectedItems.Any())
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Are you sure you want to delete the selected {selectedItems.Count} students?",
                    "Confirmation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    foreach (var student in selectedItems)
                    {
                        _studentDataService.DeleteStudent(student);
                        _students.Remove(student);
                    }
                    LoadStudents();
                }
            }
            else
            {
                MessageBox.Show("Please select students to delete.", "Information",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SortStudents();
        }

        private void SortStudents()
        {
            if (SortComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string sortOption = selectedItem.Content.ToString();

                switch (sortOption)
                {
                    case "Date and Time Modified (Oldest First)":
                        SortList(s => s.DateTime, ListSortDirection.Ascending);
                        break;
                    case "Date and Time Modified (Newest First)":
                        SortList(s => s.DateTime, ListSortDirection.Descending);
                        break;
                    case "Alphabetical First Name":
                        SortList(s => s.FirstName, ListSortDirection.Ascending);
                        break;
                    case "Alphabetical Last Name":
                        SortList(s => s.LastName, ListSortDirection.Ascending);
                        break;
                    case "ID Number":
                        SortList(s => s.IDNumber, ListSortDirection.Ascending);
                        break;
                    case "Year Level":
                        SortList(s => s.YearLevel, ListSortDirection.Ascending);
                        break;
                    case "Gender":
                        SortList(s => s.Gender, ListSortDirection.Ascending);
                        break;
                    case "Program Code":
                        SortList(s => s.ProgramCode, ListSortDirection.Ascending);
                        break;
                    case "College Code":
                        SortList(s => s.CollegeCode, ListSortDirection.Ascending);
                        break;
                    case "Alphabetical User":
                        SortList(s => s.User, ListSortDirection.Ascending);
                        break;
                }
            }
        }

        private void SortList<TKey>(Func<Student, TKey> keySelector, ListSortDirection direction)
        {
            List<Student> sortedList;
            if (direction == ListSortDirection.Ascending)
            {
                sortedList = _students.OrderBy(keySelector).ToList();
            }
            else
            {
                sortedList = _students.OrderByDescending(keySelector).ToList();
            }

            _students.Clear();
            foreach (var item in sortedList)
            {
                _students.Add(item);
            }
            StudentListView.Items.Refresh();
        }

        private void FindVisualChildren<T>(DependencyObject parent, List<T> results) where T : DependencyObject
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t)
                {
                    results.Add(t);
                }
                FindVisualChildren<T>(child, results);
            }
        }

        private void StudentListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Implementation if needed
        }

        private void HandleProgramCodeChange(string oldProgramCode, string newProgramCode)
        {
            var affectedStudents = _students.Where(s =>
                s.ProgramCode.Equals(oldProgramCode, StringComparison.OrdinalIgnoreCase)).ToList();

            if (affectedStudents.Any())
            {
                MessageBox.Show(
                    $"{affectedStudents.Count} students will be updated to the new program code '{newProgramCode}'.",
                    "Students Affected",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                foreach (var student in affectedStudents)
                {
                    student.ProgramCode = newProgramCode;
                    var program = _programDataService.GetAllPrograms()
                        .FirstOrDefault(p => p.Code.Equals(newProgramCode, StringComparison.OrdinalIgnoreCase));
                    if (program != null)
                    {
                        student.CollegeCode = program.CollegeCode;
                    }
                    _studentDataService.UpdateStudent(student, student);
                }
            }
        }

        private bool ValidateProgramCodeChange(string newProgramCode)
        {
            var program = _programDataService.GetAllPrograms()
                .FirstOrDefault(p => p.Code.Equals(newProgramCode, StringComparison.OrdinalIgnoreCase));

            if (program == null && newProgramCode != "DELETED")
            {
                MessageBox.Show($"Program code '{newProgramCode}' does not exist.",
                    "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (program != null)
            {
                var college = _collegeDataService.GetAllColleges()
                    .FirstOrDefault(c => c.Code.Equals(program.CollegeCode, StringComparison.OrdinalIgnoreCase));

                if (college == null)
                {
                    MessageBox.Show($"The college for program '{newProgramCode}' does not exist.",
                        "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            return true;
        }

        private bool HasAffectedStudents(string programCode)
        {
            return _students.Any(s => s.ProgramCode.Equals(programCode, StringComparison.OrdinalIgnoreCase));
        }
    }
}