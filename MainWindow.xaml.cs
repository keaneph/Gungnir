using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using glaive.Views; // Ensure this is present

namespace glaive
{
    public partial class MainWindow : Window
    {
        private CollegeDataService _collegeDataService;
        private AddCollegeControl _addCollegeControl;
        private ViewCollegesControl _viewCollegesControl;

        //private AddOption2View _addOption2View;
        //private AddOption3View _addOption3View;
        //private ViewOption2View _viewOption2View;
        //private ViewOption3View _viewOption3View;
        //private HistoryView _historyView;
        //private AboutView _aboutView;
        private DashboardView _dashboardView;  // Add this line

        // Property to store the logged-in user name.
        // (This would typically be set after a successful login.)
        public string CurrentUser { get; set; } = "Admin";

        public MainWindow()
        {
            InitializeComponent();
            _collegeDataService = new CollegeDataService("colleges.csv");
            _addCollegeControl = new AddCollegeControl(_collegeDataService);
            _viewCollegesControl = new ViewCollegesControl(_collegeDataService);

            //_addOption2View = new AddOption2View();
            //_addOption3View = new AddOption3View();
            //_viewOption2View = new ViewOption2View();
            //_viewOption3View = new ViewOption3View();
            //_historyView = new HistoryView();
            //_aboutView = new AboutView();
            _dashboardView = new DashboardView(); // Add this line

            // Update profile display with current user information.
            SideProfileName.Text = $"Logged in as: {CurrentUser}";
            ProfileName.Text = CurrentUser;

            // Set initial content to the Dashboard
            MainContent.Content = _dashboardView; // Change this line
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
            MainContent.Content = _dashboardView; // Change this line
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
            //MainContent.Content = _addOption2View;
            UpdateDirectory("Add/Option2");
        }

        private void NavigateAddOption3_Click(object sender, RoutedEventArgs e)
        {
            //MainContent.Content = _addOption3View;
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
            //MainContent.Content = _viewOption2View;
            UpdateDirectory("View/Option2");
        }

        private void NavigateViewOption3_Click(object sender, RoutedEventArgs e)
        {
            //MainContent.Content = _viewOption3View;
            UpdateDirectory("View/Option3");
        }

        private void NavigateHistory_Click(object sender, RoutedEventArgs e)
        {
            //MainContent.Content = _historyView;
            UpdateDirectory("History");
        }

        private void NavigateAbout_Click(object sender, RoutedEventArgs e)
        {
            //MainContent.Content = _aboutView;
            UpdateDirectory("About");
        }
    }
}