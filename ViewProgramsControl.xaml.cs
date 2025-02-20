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
    public partial class ViewProgramsControl : UserControl
    {
        private ProgramDataService _programDataService;
        private ObservableCollection<Program> _programs;
        private Dictionary<Program, Program> _originalProgramData;

        // Add events for history tracking
        public event EventHandler<ProgramEventArgs> ProgramDeleted;
        public event EventHandler<ProgramEventArgs> ProgramUpdated;
        public event EventHandler ProgramsCleared;

        public ViewProgramsControl(ProgramDataService programDataService)
        {
            InitializeComponent();
            _programDataService = programDataService;
            _programs = new ObservableCollection<Program>(_programDataService.GetAllPrograms());
            _originalProgramData = new Dictionary<Program, Program>();

            ProgramListView.ItemsSource = _programs;
            SortComboBox.SelectedIndex = 0;
            UpdateUIState();
        }

        public void LoadPrograms()
        {
            try
            {
                List<Program> programs = _programDataService.GetAllPrograms();
                _programs.Clear();
                foreach (var program in programs)
                {
                    _programs.Add(program);
                }
                SortPrograms();
                UpdateUIState();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading programs: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateUIState()
        {
            bool hasData = _programs.Any();
            DeleteSelectedButton.IsEnabled = hasData;
            ClearProgramsButton.IsEnabled = hasData;
            EditModeToggleButton.IsEnabled = hasData;

            if (!hasData)
            {
                EditModeToggleButton.IsChecked = false;
            }
        }

        private void ClearProgramsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_programs.Any())
            {
                MessageBox.Show("No programs to clear.", "Information",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to clear all program data?\nThis action cannot be undone.",
                "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    File.WriteAllText("programs.csv", string.Empty);
                    ProgramsCleared?.Invoke(this, EventArgs.Empty);
                    LoadPrograms();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error clearing program data: {ex.Message}", "Error");
                }
            }
        }

        private void DeleteSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = ProgramListView.SelectedItems.OfType<Program>().ToList();
            if (selectedItems.Any())
            {
                // Check for dependencies
                foreach (var program in selectedItems)
                {
                    if (HasDependencies(program))
                    {
                        MessageBox.Show(
                            $"Cannot delete program '{program.Name}' ({program.Code}) because it has associated students.",
                            "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                MessageBoxResult result = MessageBox.Show(
                    $"Are you sure you want to delete the selected {selectedItems.Count} program(s)?",
                    "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        foreach (var program in selectedItems)
                        {
                            _programDataService.DeleteProgram(program);
                            _programs.Remove(program);
                            ProgramDeleted?.Invoke(this, new ProgramEventArgs(program));
                        }
                        LoadPrograms();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting programs: {ex.Message}",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select programs to delete.",
                    "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private bool HasDependencies(Program program)
        {
            // Implement check for students using this program
            // Return true if there are dependencies
            return false; // Placeholder
        }

        private void EditModeToggleButton_Checked(object sender, RoutedEventArgs e)
        {
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
        }

        private void EditModeToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                var programsCopy = _programs.ToList();

                foreach (var program in programsCopy)
                {
                    if (_originalProgramData.TryGetValue(program, out Program originalProgram))
                    {
                        if (HasChanges(program, originalProgram))
                        {
                            if (ValidateChanges(program))
                            {
                                _programDataService.UpdateProgram(originalProgram, program);
                                ProgramUpdated?.Invoke(this, new ProgramEventArgs(program));
                            }
                            else
                            {
                                // Revert changes if validation fails
                                program.Name = originalProgram.Name;
                                program.Code = originalProgram.Code;
                                program.CollegeCode = originalProgram.CollegeCode;
                            }
                        }
                    }
                }
                LoadPrograms();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving changes: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                LoadPrograms(); // Reload to revert changes
            }
        }

        private bool HasChanges(Program current, Program original)
        {
            return current.Name != original.Name ||
                   current.Code != original.Code ||
                   current.CollegeCode != original.CollegeCode;
        }

        private bool ValidateChanges(Program program)
        {
            if (string.IsNullOrWhiteSpace(program.Name) ||
                string.IsNullOrWhiteSpace(program.Code) ||
                string.IsNullOrWhiteSpace(program.CollegeCode))
            {
                MessageBox.Show("Program name, code, and college code cannot be empty.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (program.Code.Length < 2 || program.Code.Length > 5)
            {
                MessageBox.Show("Program code must be between 2 and 5 characters.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            var otherPrograms = _programs.Where(p => p != program);
            if (otherPrograms.Any(p => p.Code.Equals(program.Code, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show($"Program code '{program.Code}' is already in use.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

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
                        SortList(p => p.DateTime, ListSortDirection.Ascending);
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

        private void ProgramListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DeleteSelectedButton.IsEnabled = ProgramListView.SelectedItems.Count > 0;
        }
    }

}