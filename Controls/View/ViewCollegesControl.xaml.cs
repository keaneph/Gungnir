using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using sis_app.Models;      // For College class
using sis_app.Services;    // For CollegeDataService

namespace sis_app.Controls.View
{
    public partial class ViewCollegesControl : UserControl
    {
        private CollegeDataService _collegeDataService;
        private ObservableCollection<College> _colleges; // Use ObservableCollection
        private Dictionary<College, College> _originalCollegeData = new Dictionary<College, College>(); // Store original data

        public ViewCollegesControl(CollegeDataService collegeDataService)
        {
            InitializeComponent();
            _collegeDataService = collegeDataService;
            _colleges = new ObservableCollection<College>(_collegeDataService.GetAllColleges()); // Initialize
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

        private void ClearCollegesButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to clear all college data?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    File.WriteAllText("colleges.csv", string.Empty); // Clear the CSV file
                    LoadColleges(); // Refresh the list
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error clearing college data: {ex.Message}", "Error");
                }
            }
        }

        private void EditModeToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            // Store original data before editing
            _originalCollegeData.Clear();
            foreach (var college in _colleges)
            {
                _originalCollegeData[college] = new College { Name = college.Name, Code = college.Code, DateTime = college.DateTime, User = college.User }; // Create a copy
            }
        }

        private void EditModeToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var item in CollegeListView.Items)
            {
                if (item is College college)
                {
                    College oldCollege = _originalCollegeData[college]; // Get Original copy

                    if (oldCollege != null && (college.Name != oldCollege.Name || college.Code != oldCollege.Code))
                    {
                        // Only update if Name or Code has changed
                        _collegeDataService.UpdateCollege(oldCollege, college);
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
                MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete the selected {selectedItems.Count} colleges?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    foreach (var college in selectedItems)
                    {
                        _collegeDataService.DeleteCollege(college);
                        _colleges.Remove(college);
                    }
                }
                LoadColleges();
            }
            else
            {
                MessageBox.Show("Please select colleges to delete.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
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

        }
    }
}