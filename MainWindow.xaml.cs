using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using glaive;

namespace sis_app
{
    public partial class MainWindow : Window
    {
        private CollegeDataService _collegeDataService;
        private AddCollegeControl _addCollegeControl;
        private ViewCollegesControl _viewCollegesControl;
        private DashboardView _dashboardView;

        // Property to store the logged-in user name.
        // (This would typically be set after a successful login.)
        public string CurrentUser { get; set; } = "Admin";

        public MainWindow()
        {
            InitializeComponent();
            _collegeDataService = new CollegeDataService("colleges.csv");
            _addCollegeControl = new AddCollegeControl(_collegeDataService);
            _viewCollegesControl = new ViewCollegesControl(_collegeDataService);

            _dashboardView = new DashboardView();

            // Update profile display with current user information.
            SideProfileName.Text = $"Logged in as: {CurrentUser}";
            ProfileName.Text = CurrentUser;

            // Set initial content to the Dashboard
            MainContent.Content = _dashboardView;
        }

        // Search Box Placeholder Logic
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

        // Helper to update the directory text in the Top Bar
        private void UpdateDirectory(string page)
        {
            DirectoryText.Text = $" | /{page}";
        }

        // Dashboard button: navigate to the dashboard page
        private void NavigateHome_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = _dashboardView;
            UpdateDirectory("Home");
        }

        // Add Expander Options
        private void NavigateAddOption1_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = _addCollegeControl;
            UpdateDirectory("Add/Option1");
        }
        private void NavigateAddOption2_Click(object sender, RoutedEventArgs e)
        {
            // Implement the logic for adding option 2
            UpdateDirectory("Add/Option2");
        }

        private void NavigateAddOption3_Click(object sender, RoutedEventArgs e)
        {
            // Implement the logic for adding option 3
            UpdateDirectory("Add/Option3");
        }

        // View Expander Options
        private void NavigateViewOption1_Click(object sender, RoutedEventArgs e)
        {
            _viewCollegesControl.LoadColleges();
            MainContent.Content = _viewCollegesControl;
            UpdateDirectory("View/Option1");
        }

        private void NavigateViewOption2_Click(object sender, RoutedEventArgs e)
        {
            // Implement the logic for viewing option 2
            UpdateDirectory("View/Option2");
        }

        private void NavigateViewOption3_Click(object sender, RoutedEventArgs e)
        {
            // Implement the logic for viewing option 3
            UpdateDirectory("View/Option3");
        }


        private void NavigateAbout_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new TextBlock { Text = "About This Application", FontSize = 20 };
            UpdateDirectory("About");
        }

        private void NavigateHistory_Click(object sender, RoutedEventArgs e)
        {
            // Implement the logic for the history page
            UpdateDirectory("History");
        }
    }
}