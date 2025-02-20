using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace glaive
{
    public partial class ViewStudentControl : UserControl
    {
        private StudentDataService _studentDataService;
        private ObservableCollection<Student> _students;
        private Dictionary<Student, Student> _originalStudentData;

        // Add events for history tracking
        public event EventHandler<StudentEventArgs> StudentDeleted;
        public event EventHandler<StudentEventArgs> StudentUpdated;
        public event EventHandler StudentsCleared;

        public ViewStudentControl(StudentDataService studentDataService)
        {
            InitializeComponent();
            _studentDataService = studentDataService;
            _students = new ObservableCollection<Student>(_studentDataService.GetAllStudents());
            _originalStudentData = new Dictionary<Student, Student>();

            StudentListView.ItemsSource = _students;
            SortComboBox.SelectedIndex = 0;
            UpdateUIState();
        }

        public void LoadStudents()
        {
            try
            {
                List<Student> students = _studentDataService.GetAllStudents();
                _students.Clear();
                foreach (var student in students)
                {
                    _students.Add(student);
                }
                SortStudents();
                UpdateUIState();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading students: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateUIState()
        {
            bool hasData = _students.Any();
            DeleteSelectedButton.IsEnabled = hasData;
            ClearStudentsButton.IsEnabled = hasData;
            EditModeToggleButton.IsEnabled = hasData;

            if (!hasData)
            {
                EditModeToggleButton.IsChecked = false;
            }
        }

        private void ClearStudentsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_students.Any())
            {
                MessageBox.Show("No students to clear.", "Information",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to clear all student data?\nThis action cannot be undone.",
                "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    File.WriteAllText("students.csv", string.Empty);
                    StudentsCleared?.Invoke(this, EventArgs.Empty);
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
        }

        private void EditModeToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var item in StudentListView.Items)
                {
                    if (item is Student student && _originalStudentData.TryGetValue(student, out Student oldStudent))
                    {
                        if (HasChanges(student, oldStudent))
                        {
                            if (ValidateChanges(student))
                            {
                                _studentDataService.UpdateStudent(oldStudent, student);
                                StudentUpdated?.Invoke(this, new StudentEventArgs(student));
                            }
                            else
                            {
                                // Revert changes if validation fails
                                RevertChanges(student, oldStudent);
                            }
                        }
                    }
                }
                LoadStudents();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving changes: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                LoadStudents(); // Reload to revert changes
            }
        }

        private bool HasChanges(Student current, Student original)
        {
            return current.FirstName != original.FirstName ||
                   current.LastName != original.LastName ||
                   current.IDNumber != original.IDNumber ||
                   current.YearLevel != original.YearLevel ||
                   current.Gender != original.Gender ||
                   current.ProgramCode != original.ProgramCode ||
                   current.CollegeCode != original.CollegeCode;
        }

        private void RevertChanges(Student current, Student original)
        {
            current.FirstName = original.FirstName;
            current.LastName = original.LastName;
            current.IDNumber = original.IDNumber;
            current.YearLevel = original.YearLevel;
            current.Gender = original.Gender;
            current.ProgramCode = original.ProgramCode;
            current.CollegeCode = original.CollegeCode;
        }

        private bool ValidateChanges(Student student)
        {
            if (string.IsNullOrWhiteSpace(student.FirstName) ||
                string.IsNullOrWhiteSpace(student.LastName) ||
                string.IsNullOrWhiteSpace(student.IDNumber))
            {
                MessageBox.Show("Student name and ID cannot be empty.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Validate ID Number format (YYYY-NNNN)
            if (!IsValidIDNumber(student.IDNumber))
            {
                MessageBox.Show("Invalid ID Number format. Must be YYYY-NNNN.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Check for duplicate ID
            var otherStudents = _students.Where(s => s != student);
            if (otherStudents.Any(s => s.IDNumber.Equals(student.IDNumber, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show($"Student ID '{student.IDNumber}' is already in use.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private bool IsValidIDNumber(string idNumber)
        {
            if (!idNumber.Contains("-")) return false;

            var parts = idNumber.Split('-');
            if (parts.Length != 2) return false;

            if (!int.TryParse(parts[0], out int year) || year < 2000 || year > DateTime.Now.Year)
                return false;

            if (!int.TryParse(parts[1], out int number) || parts[1].Length != 4)
                return false;

            return true;
        }

        private void DeleteSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = StudentListView.SelectedItems.OfType<Student>().ToList();
            if (selectedItems.Any())
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Are you sure you want to delete the selected {selectedItems.Count} student(s)?",
                    "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        foreach (var student in selectedItems)
                        {
                            _studentDataService.DeleteStudent(student);
                            _students.Remove(student);
                            StudentDeleted?.Invoke(this, new StudentEventArgs(student));
                        }
                        LoadStudents();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting students: {ex.Message}",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select students to delete.",
                    "Information", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void StudentListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DeleteSelectedButton.IsEnabled = StudentListView.SelectedItems.Count > 0;
        }
    }
}