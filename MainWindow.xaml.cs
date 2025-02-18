﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using glaive;

namespace sis_app
{
    public partial class MainWindow : Window
    {
        private CollegeDataService _collegeDataService;
        private ProgramDataService _programDataService;
        private AddCollegeControl _addCollegeControl;
        private AddProgramControl _addProgramControl;
        private ViewCollegesControl _viewCollegesControl;
        private ViewProgramsControl _viewProgramsControl;
        private DashboardView _dashboardView;

        public string CurrentUser { get; set; } = "Admin";

        public MainWindow()
        {
            InitializeComponent();

            _collegeDataService = new CollegeDataService("colleges.csv") { CurrentUser = CurrentUser };
            _programDataService = new ProgramDataService("programs.csv") { CurrentUser = CurrentUser };


            _addCollegeControl = new AddCollegeControl(_collegeDataService);
            _addProgramControl = new AddProgramControl(_programDataService, _collegeDataService);
            _viewCollegesControl = new ViewCollegesControl(_collegeDataService);
            _viewProgramsControl = new ViewProgramsControl(_programDataService); 
            _dashboardView = new DashboardView();

            SideProfileName.Text = $"Logged in as: {CurrentUser}";
            ProfileName.Text = CurrentUser;

            MainContent.Content = _dashboardView;

            SearchBox.Text = "Search...";
            SearchBox.Foreground = Brushes.Gray;
        }
        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "Search...")
            {
                SearchBox.Text = "";
                SearchBox.Foreground = Brushes.Black;
            }
        }
        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchBox.Text = "Search...";
                SearchBox.Foreground = Brushes.Gray;
            }
        }

        private void UpdateDirectory(string page)
        {
            DirectoryText.Text = $" | /{page}";
        }
        private void NavigateHome_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = _dashboardView;
            UpdateDirectory("Home");
        }

        private void NavigateAddOption1_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = _addCollegeControl;
            UpdateDirectory("Add/College");
        }

        private void NavigateAddOption2_Click(object sender, RoutedEventArgs e)
        {
            _addProgramControl.LoadCollegeCodes(); // Refresh college codes in the dropdown
            MainContent.Content = _addProgramControl;
            UpdateDirectory("Add/Program");
        }

        private void NavigateAddOption3_Click(object sender, RoutedEventArgs e)
        {
            // Implement logic for adding students here
            UpdateDirectory("Add/Student"); // Placeholder
        }

        private void NavigateViewOption1_Click(object sender, RoutedEventArgs e)
        {
            _viewCollegesControl.LoadColleges();
            MainContent.Content = _viewCollegesControl;
            UpdateDirectory("View/Colleges");
        }


        private void NavigateViewOption2_Click(object sender, RoutedEventArgs e)
        {
            _viewProgramsControl.LoadPrograms();  // Uncomment and implement when you create ViewProgramsControl
            MainContent.Content = _viewProgramsControl; // Uncomment when you have ViewProgramsControl
            UpdateDirectory("View/Programs"); // Placeholder for now
        }

        private void NavigateViewOption3_Click(object sender, RoutedEventArgs e)
        {
            // Implement logic for viewing students here
            UpdateDirectory("View/Students"); // Placeholder
        }

        private void NavigateAbout_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new TextBlock { Text = "About This Application", FontSize = 20 };
            UpdateDirectory("About");
        }

        private void NavigateHistory_Click(object sender, RoutedEventArgs e)
        {
            // Implement logic for history page here
            UpdateDirectory("History");
        }
    }
}