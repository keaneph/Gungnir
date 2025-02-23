﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Input;
using sis_app.Models;
using sis_app.Services;

namespace sis_app.Controls.View
{
    /// <summary>
    /// Control for viewing and managing college data in the Student Information System.
    /// Handles CRUD operations and maintains relationships with programs.
    /// </summary>
    public partial class ViewCollegesControl : UserControl
    {
        #region Constants
        private const int MAX_COLLEGE_NAME_LENGTH = 27;
        private const int MAX_COLLEGE_CODE_LENGTH = 9;
        private const int MIN_CODE_LENGTH = 2;
        private const string DELETED_MARKER = "DELETED";
        #endregion

        #region Private Fields
        private readonly CollegeDataService _collegeDataService;
        private readonly ProgramDataService _programDataService;
        private readonly ObservableCollection<College> _colleges;
        private readonly Dictionary<College, College> _originalCollegeData;
        #endregion

        #region Constructor
        public ViewCollegesControl(CollegeDataService collegeDataService)
        {
            InitializeComponent();

            _collegeDataService = collegeDataService ?? throw new ArgumentNullException(nameof(collegeDataService));
            _programDataService = new ProgramDataService("programs.csv");
            _colleges = new ObservableCollection<College>();
            _originalCollegeData = new Dictionary<College, College>();

            InitializeUserInterface();
        }
        #endregion

        #region Initialization Methods
        private void InitializeUserInterface()
        {
            CollegeListView.ItemsSource = _colleges;
            LoadColleges();
            SortComboBox.SelectedIndex = 0;
        }
        #endregion

        #region Data Loading Methods
        public void LoadColleges()
        {
            try
            {
                var colleges = _collegeDataService.GetAllColleges();
                _colleges.Clear();
                foreach (var college in colleges)
                {
                    _colleges.Add(college);
                }
                SortColleges();
            }
            catch (Exception ex)
            {
                HandleLoadError(ex);
            }
        }

        private void HandleLoadError(Exception ex)
        {
            MessageBox.Show(
                $"Error loading colleges: {ex.Message}",
                "Load Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
        #endregion

        #region Input Validation Methods
        private void CollegeNameTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsValidNameInput(e.Text);
        }

        private void CollegeCodeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsValidCodeInput(e.Text);
        }

        private static bool IsValidNameInput(string text)
        {
            return text.All(c => char.IsLetter(c));
        }

        private static bool IsValidCodeInput(string text)
        {
            return text.All(c => char.IsLetter(c));
        }
        #endregion

        #region Text Change Handlers
        private void CollegeNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                EnforceLengthLimit(textBox, MAX_COLLEGE_NAME_LENGTH);
            }
        }

        private void CollegeCodeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                int caretIndex = textBox.CaretIndex;
                string newText = textBox.Text.ToUpper();

                newText = EnforceCodeRules(newText);

                textBox.Text = newText;
                textBox.CaretIndex = Math.Min(caretIndex, newText.Length);
            }
        }

        private static void EnforceLengthLimit(TextBox textBox, int maxLength)
        {
            if (textBox.Text.Length > maxLength)
            {
                textBox.Text = textBox.Text.Substring(0, maxLength);
                textBox.CaretIndex = maxLength;
            }
        }

        private static string EnforceCodeRules(string text)
        {
            return new string(text.Where(char.IsLetter).Take(MAX_COLLEGE_CODE_LENGTH).ToArray());
        }
        #endregion

        #region Edit Mode Handlers
        private void EditModeToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            StoreOriginalData();
        }

        private void EditModeToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            ProcessEditedData();
        }

        private void StoreOriginalData()
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

        private void ProcessEditedData()
        {
            foreach (var college in _colleges.ToList())
            {
                if (!_originalCollegeData.TryGetValue(college, out College originalCollege))
                    continue;

                if (!HasChanges(college, originalCollege))
                    continue;

                if (!ValidateAndUpdateCollege(college, originalCollege))
                {
                    RevertChanges(college, originalCollege);
                }
            }
            LoadColleges();
        }

        private static bool HasChanges(College current, College original)
        {
            return current.Name != original.Name || current.Code != original.Code;
        }

        private void RevertChanges(College current, College original)
        {
            current.Name = original.Name;
            current.Code = original.Code;
        }
        #endregion

        #region Validation Methods
        private bool ValidateAndUpdateCollege(College college, College originalCollege)
        {
            if (!ValidateEditedData(college))
                return false;

            if (IsDuplicateCode(college))
            {
                ShowDuplicateCodeError(college.Code);
                return false;
            }

            UpdateRelatedPrograms(originalCollege.Code, college.Code);
            _collegeDataService.UpdateCollege(originalCollege, college);
            return true;
        }

        private bool ValidateEditedData(College college)
        {
            if (string.IsNullOrWhiteSpace(college.Name))
            {
                ShowValidationError("College name cannot be empty.");
                return false;
            }

            if (college.Name.Length > MAX_COLLEGE_NAME_LENGTH)
            {
                ShowValidationError($"College name cannot exceed {MAX_COLLEGE_NAME_LENGTH} characters.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(college.Code))
            {
                ShowValidationError("College code cannot be empty.");
                return false;
            }

            if (college.Code.Length < MIN_CODE_LENGTH)
            {
                ShowValidationError($"College code must be at least {MIN_CODE_LENGTH} characters.");
                return false;
            }

            if (college.Code.Length > MAX_COLLEGE_CODE_LENGTH)
            {
                ShowValidationError($"College code cannot exceed {MAX_COLLEGE_CODE_LENGTH} characters.");
                return false;
            }

            if (!college.Name.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
            {
                ShowValidationError("College name can only contain letters and spaces.");
                return false;
            }

            if (!college.Code.All(char.IsLetter))
            {
                ShowValidationError("College code can only contain letters.");
                return false;
            }

            return true;
        }

        private bool IsDuplicateCode(College college)
        {
            return _colleges.Any(c =>
                c != college &&
                c.Code.Equals(college.Code, StringComparison.OrdinalIgnoreCase));
        }

        private static void ShowValidationError(string message)
        {
            MessageBox.Show(
                message,
                "Validation Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
        }

        private static void ShowDuplicateCodeError(string code)
        {
            MessageBox.Show(
                $"A college with code '{code}' already exists.",
                "Duplicate Code Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
        #endregion

        #region Data Management Methods
        private void UpdateRelatedPrograms(string oldCode, string newCode)
        {
            var affectedPrograms = _programDataService.GetAllPrograms()
                .Where(p => p.CollegeCode.Equals(oldCode, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!affectedPrograms.Any()) return;

            ShowProgramsUpdatedMessage(oldCode, newCode, affectedPrograms.Count);

            foreach (var program in affectedPrograms)
            {
                program.CollegeCode = newCode;
                _programDataService.UpdateProgram(program, program);
            }
        }

        private static void ShowProgramsUpdatedMessage(string oldCode, string newCode, int count)
        {
            MessageBox.Show(
                $"College code updated from '{oldCode}' to '{newCode}'. {count} associated programs have been updated.",
                "Programs Updated",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }
        #endregion

        #region Delete Operations
        private void DeleteSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = CollegeListView.SelectedItems.OfType<College>().ToList();
            if (!selectedItems.Any())
            {
                ShowNoSelectionMessage();
                return;
            }

            if (ConfirmDeletion(selectedItems))
            {
                DeleteColleges(selectedItems);
                LoadColleges();
            }
        }

        private void ClearCollegesButton_Click(object sender, RoutedEventArgs e)
        {
            if (ConfirmClearAll())
            {
                ClearAllData();
            }
        }

        private static void ShowNoSelectionMessage()
        {
            MessageBox.Show(
                "Please select colleges to delete.",
                "Information",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        private bool ConfirmDeletion(List<College> selectedItems)
        {
            var affectedPrograms = GetAffectedPrograms(selectedItems);
            string message = BuildDeleteConfirmationMessage(selectedItems.Count, affectedPrograms.Count);

            return MessageBox.Show(
                message,
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            ) == MessageBoxResult.Yes;
        }

        private void DeleteColleges(List<College> colleges)
        {
            foreach (var college in colleges)
            {
                DeleteCollegeAndUpdatePrograms(college);
            }
        }

        private void DeleteCollegeAndUpdatePrograms(College college)
        {
            _collegeDataService.DeleteCollege(college);
            _colleges.Remove(college);

            var affectedPrograms = _programDataService.GetAllPrograms()
                .Where(p => p.CollegeCode.Equals(college.Code, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var program in affectedPrograms)
            {
                program.CollegeCode = DELETED_MARKER;
                _programDataService.UpdateProgram(program, program);
            }
        }

        private List<Program> GetAffectedPrograms(List<College> colleges)
        {
            return _programDataService.GetAllPrograms()
                .Where(p => colleges.Any(c =>
                    c.Code.Equals(p.CollegeCode, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        private static string BuildDeleteConfirmationMessage(int collegeCount, int programCount)
        {
            string message = $"Are you sure you want to delete the selected {collegeCount} colleges?";
            if (programCount > 0)
            {
                message += $"\nWarning: {programCount} programs are using these colleges and will be affected.";
            }
            return message;
        }

        private static bool ConfirmClearAll()
        {
            return MessageBox.Show(
                "Warning: Clearing colleges will also affect programs using these college codes. Continue?",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            ) == MessageBoxResult.Yes;
        }

        private void ClearAllData()
        {
            try
            {
                File.WriteAllText("colleges.csv", string.Empty);
                File.WriteAllText("programs.csv", string.Empty);
                LoadColleges();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error clearing college data: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
        #endregion

        #region Sorting Methods
        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SortColleges();
        }

        private void SortColleges()
        {
            if (SortComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                ApplySorting(selectedItem.Content.ToString());
            }
        }

        private void ApplySorting(string sortOption)
        {
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

        private void SortList<TKey>(Func<College, TKey> keySelector, ListSortDirection direction)
        {
            var sortedList = direction == ListSortDirection.Ascending
                ? _colleges.OrderBy(keySelector).ToList()
                : _colleges.OrderByDescending(keySelector).ToList();

            _colleges.Clear();
            foreach (var item in sortedList)
            {
                _colleges.Add(item);
            }
            CollegeListView.Items.Refresh();
        }
        #endregion
    }
}