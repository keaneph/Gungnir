using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace glaive
{
    public partial class ViewCollegesControl : UserControl
    {
        private CollegeDataService _collegeDataService;
        private ObservableCollection<College> _colleges;
        private Dictionary<College, College> _originalCollegeData;

        // Add events for history tracking
        public event EventHandler<CollegeEventArgs> CollegeDeleted;
        public event EventHandler<CollegeEventArgs> CollegeUpdated;
        public event EventHandler CollegesCleared;

        public ViewCollegesControl(CollegeDataService collegeDataService)
        {
            InitializeComponent();
            _collegeDataService = collegeDataService;
            _colleges = new ObservableCollection<College>(_collegeDataService.GetAllColleges());
            _originalCollegeData = new Dictionary<College, College>();

            CollegeListView.ItemsSource = _colleges;
            SortComboBox.SelectedIndex = 0;
            UpdateUIState();
        }

        public void LoadColleges()
        {
            try
            {
                List<College> colleges = _collegeDataService.GetAllColleges();
                _colleges.Clear();
                foreach (var college in colleges)
                {
                    _colleges.Add(college);
                }
                SortColleges();
                UpdateUIState();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading colleges: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateUIState()
        {
            bool hasData = _colleges.Any();
            DeleteSelectedButton.IsEnabled = hasData;
            ClearCollegesButton.IsEnabled = hasData;
            EditModeToggleButton.IsEnabled = hasData;

            if (!hasData)
            {
                EditModeToggleButton.IsChecked = false;
            }
        }

        private void ClearCollegesButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_colleges.Any())
            {
                MessageBox.Show("No colleges to clear.", "Information",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to clear all college data?\nThis action cannot be undone.",
                "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    File.WriteAllText("colleges.csv", string.Empty);
                    CollegesCleared?.Invoke(this, EventArgs.Empty);
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
            try
            {
                foreach (var item in CollegeListView.Items)
                {
                    if (item is College college && _originalCollegeData.TryGetValue(college, out College oldCollege))
                    {
                        if (HasChanges(college, oldCollege))
                        {
                            if (ValidateChanges(college))
                            {
                                _collegeDataService.UpdateCollege(oldCollege, college);
                                CollegeUpdated?.Invoke(this, new CollegeEventArgs(college));
                            }
                            else
                            {
                                // Revert changes if validation fails
                                college.Name = oldCollege.Name;
                                college.Code = oldCollege.Code;
                            }
                        }
                    }
                }
                LoadColleges();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving changes: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                LoadColleges(); // Reload to revert changes
            }
        }

        private bool HasChanges(College current, College original)
        {
            return current.Name != original.Name ||
                   current.Code != original.Code;
        }

        private bool ValidateChanges(College college)
        {
            if (string.IsNullOrWhiteSpace(college.Name) ||
                string.IsNullOrWhiteSpace(college.Code))
            {
                MessageBox.Show("College name and code cannot be empty.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (college.Code.Length < 2 || college.Code.Length > 5)
            {
                MessageBox.Show("College code must be between 2 and 5 characters.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            var otherColleges = _colleges.Where(c => c != college);
            if (otherColleges.Any(c => c.Code.Equals(college.Code, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show($"College code '{college.Code}' is already in use.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void DeleteSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = CollegeListView.SelectedItems.OfType<College>().ToList();
            if (selectedItems.Any())
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Are you sure you want to delete the selected {selectedItems.Count} colleges?",
                    "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        foreach (var college in selectedItems)
                        {
                            _collegeDataService.DeleteCollege(college);
                            _colleges.Remove(college);
                            CollegeDeleted?.Invoke(this, new CollegeEventArgs(college));
                        }
                        LoadColleges();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting colleges: {ex.Message}",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select colleges to delete.",
                    "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

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

        private void CollegeListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DeleteSelectedButton.IsEnabled = CollegeListView.SelectedItems.Count > 0;
        }
    }

}