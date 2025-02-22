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
        private ProgramDataService _programDataService;
        private CollegeDataService _collegeDataService;
        private ObservableCollection<Program> _programs;
        private Dictionary<Program, Program> _originalProgramData = new Dictionary<Program, Program>();
        private List<string> _availableCollegeCodes;

        public ViewProgramsControl(ProgramDataService programDataService)
        {
            InitializeComponent();
            _programDataService = programDataService;
            _collegeDataService = new CollegeDataService("colleges.csv");
            _programs = new ObservableCollection<Program>(_programDataService.GetAllPrograms());
            ProgramListView.ItemsSource = _programs;

            LoadAvailableCollegeCodes();
            SortComboBox.SelectedIndex = 0;
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
            List<Program> programs = _programDataService.GetAllPrograms();
            _programs.Clear();
            foreach (var program in programs)
            {
                _programs.Add(program);
            }
            SortPrograms();
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
                textBox.Text = textBox.Text.ToUpper();

                if (textBox.Text.Length > MAX_PROGRAM_CODE_LENGTH)
                {
                    textBox.Text = textBox.Text.Substring(0, MAX_PROGRAM_CODE_LENGTH);
                    caretIndex = MAX_PROGRAM_CODE_LENGTH;
                }

                textBox.CaretIndex = caretIndex;
            }
        }

        private void ClearProgramsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to clear all program data?", "Confirmation",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
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
            var programsCopy = _programs.ToList();

            foreach (var program in programsCopy)
            {
                if (_originalProgramData.ContainsKey(program))
                {
                    Program originalProgram = _originalProgramData[program];

                    if (program.Name != originalProgram.Name ||
                        program.Code != originalProgram.Code ||
                        program.CollegeCode != originalProgram.CollegeCode)
                    {
                        // validate the edited data
                        if (!ValidateEditedData(program))
                        {
                            // if validation fails, revert changes
                            program.Name = originalProgram.Name;
                            program.Code = originalProgram.Code;
                            program.CollegeCode = originalProgram.CollegeCode;
                            continue;
                        }

                        // check for duplicate code
                        var existingProgram = _programs.FirstOrDefault(p =>
                            p != program &&
                            p.Code.Equals(program.Code, StringComparison.OrdinalIgnoreCase));

                        if (existingProgram != null)
                        {
                            MessageBox.Show($"A program with code '{program.Code}' already exists.", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            program.Code = originalProgram.Code;
                            continue;
                        }

                        _programDataService.UpdateProgram(originalProgram, program);
                    }
                }
            }

            LoadPrograms();
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
                MessageBoxResult result = MessageBox.Show(
                    $"Are you sure you want to delete the selected {selectedItems.Count} programs?",
                    "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    foreach (var program in selectedItems)
                    {
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