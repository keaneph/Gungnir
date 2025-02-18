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
        private Dictionary<Program, Program> _originalProgramData = new Dictionary<Program, Program>();


        public ViewProgramsControl(ProgramDataService programDataService)
        {
            InitializeComponent();
            _programDataService = programDataService;
            _programs = new ObservableCollection<Program>(_programDataService.GetAllPrograms());
            ProgramListView.ItemsSource = _programs;

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

        private void ClearProgramsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to clear all program data?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
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

        private void DeleteSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = ProgramListView.SelectedItems.OfType<Program>().ToList();
            if (selectedItems.Any())
            {
                MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete the selected {selectedItems.Count} programs?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    foreach (var program in selectedItems)
                    {
                        _programDataService.DeleteProgram(program);
                        _programs.Remove(program);
                    }
                    LoadPrograms(); // Refresh the list after deleting
                }

            }
            else
            {
                MessageBox.Show("Please select programs to delete.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void EditModeToggleButton_Checked(object sender, RoutedEventArgs e)
        {

            _originalProgramData.Clear();
            foreach (var program in _programs)
            {
                _originalProgramData[program] = new Program { Name = program.Name, Code = program.Code, CollegeCode = program.CollegeCode, DateTime = program.DateTime, User = program.User };
            }
        }

        private void EditModeToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            // Create a copy for safe iteration and to prevent issues with dictionary keys changing mid-iteration
            var programsCopy = _programs.ToList();  //  Create a list copy to prevent issues during iteration

            foreach (var program in programsCopy)
            {
                if (_originalProgramData.ContainsKey(program))  //Check if program key exists in _originalProgramData
                {
                    Program originalProgram = _originalProgramData[program];

                    if (program.Name != originalProgram.Name || program.Code != originalProgram.Code || program.CollegeCode != originalProgram.CollegeCode)
                    {
                        _programDataService.UpdateProgram(originalProgram, program);
                    }
                }


            }

            LoadPrograms(); // Refresh the list after saving changes
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