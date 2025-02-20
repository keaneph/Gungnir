using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using sis_app;

namespace glaive
{
    public partial class HistoryView : UserControl
    {
        private readonly HistoryService _historyService;
        private ObservableCollection<HistoryEntry> _historyEntries;

        public event EventHandler HistoryCleared; // You can remove this if not needed elsewhere

        public HistoryView(HistoryService historyService)
        {
            InitializeComponent();
            _historyService = historyService;
            _historyEntries = new ObservableCollection<HistoryEntry>();

            HistoryListView.ItemsSource = _historyEntries;
            SortComboBox.SelectedIndex = 0;

            LoadHistory();
            UpdateUIState();
        }

        public void LoadHistory()
        {
            try
            {
                var entries = _historyService.GetHistory();
                _historyEntries.Clear();
                foreach (var entry in entries)
                {
                    _historyEntries.Add(entry);
                }
                SortHistory();
                UpdateUIState();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading history: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateUIState()
        {
            // Always enable the Update button if you want it to be always available
            UpdateHistoryButton.IsEnabled = true;
        }

        // Rename the button click event handler
        private void UpdateHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadHistory();
                MessageBox.Show("History updated successfully.", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating history: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SortHistory();
        }

        private void SortHistory()
        {
            if (SortComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string sortOption = selectedItem.Content.ToString();

                switch (sortOption)
                {
                    case "Date and Time (Newest First)":
                        SortList(h => h.Timestamp, ListSortDirection.Descending);
                        break;
                    case "Date and Time (Oldest First)":
                        SortList(h => h.Timestamp, ListSortDirection.Ascending);
                        break;
                    case "Alphabetical User":
                        SortList(h => h.User, ListSortDirection.Ascending);
                        break;
                    case "Alphabetical Action":
                        SortList(h => h.Action, ListSortDirection.Ascending);
                        break;
                }
            }
        }

        private void SortList<TKey>(Func<HistoryEntry, TKey> keySelector, ListSortDirection direction)
        {
            List<HistoryEntry> sortedList;
            if (direction == ListSortDirection.Ascending)
            {
                sortedList = _historyEntries.OrderBy(keySelector).ToList();
            }
            else
            {
                sortedList = _historyEntries.OrderByDescending(keySelector).ToList();
            }

            _historyEntries.Clear();
            foreach (var item in sortedList)
            {
                _historyEntries.Add(item);
            }
            HistoryListView.Items.Refresh();
        }
    }
}