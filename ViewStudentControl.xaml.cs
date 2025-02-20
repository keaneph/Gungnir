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
        private Dictionary<Student, Student> _originalStudentData = new Dictionary<Student, Student>();

        public ViewStudentControl(StudentDataService studentDataService)
        {
            InitializeComponent();
            _studentDataService = studentDataService;
            _students = new ObservableCollection<Student>(_studentDataService.GetAllStudents());
            StudentListView.ItemsSource = _students;
            SortComboBox.SelectedIndex = 0;
        }

        public void LoadStudents()
        {
            List<Student> students = _studentDataService.GetAllStudents();
            _students.Clear();
            foreach (var student in students)
            {
                _students.Add(student);
            }
            SortStudents();
        }

        private void ClearStudentsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to clear all student data?",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

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
            foreach (var item in StudentListView.Items)
            {
                if (item is Student student)
                {
                    Student oldStudent = _originalStudentData[student];
                    if (oldStudent != null &&
                        (student.FirstName != oldStudent.FirstName ||
                         student.LastName != oldStudent.LastName ||
                         student.IDNumber != oldStudent.IDNumber ||
                         student.ProgramCode != oldStudent.ProgramCode ||
                         student.CollegeCode != oldStudent.CollegeCode))
                    {
                        _studentDataService.UpdateStudent(oldStudent, student);
                    }
                }
            }
            LoadStudents();
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
                MessageBox.Show("Please select students to delete.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
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
            // Implement if needed
        }
    }
}