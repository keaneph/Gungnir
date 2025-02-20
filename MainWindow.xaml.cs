using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Diagnostics;
using System.Text;
using System.Linq;
using glaive;

namespace sis_app
{
    public partial class MainWindow : Window
    {
        private CollegeDataService _collegeDataService;
        private ProgramDataService _programDataService;
        private StudentDataService _studentDataService;
        private HistoryService _historyService;

        private AddCollegeControl _addCollegeControl;
        private AddProgramControl _addProgramControl;
        private AddStudentControl _addStudentControl;

        private ViewCollegesControl _viewCollegesControl;
        private ViewProgramsControl _viewProgramsControl;
        private ViewStudentControl _viewStudentControl;

        private DashboardView _dashboardView;
        private AboutView _aboutView;

        public string CurrentUser { get; set; } = "Admin";

        public MainWindow(string username)
        {
            InitializeComponent();

            CurrentUser = username;

            // Initialize Services
            _historyService = new HistoryService(clearExistingHistory: true);
            _collegeDataService = new CollegeDataService("colleges.csv") { CurrentUser = CurrentUser };
            _programDataService = new ProgramDataService("programs.csv") { CurrentUser = CurrentUser };
            _studentDataService = new StudentDataService("students.csv") { CurrentUser = CurrentUser };

            // Initialize Views
            InitializeViews();

            // Set up event handlers for tracking
            SetupEventHandlers();

            // Initialize UI
            LoginStatus.Text = CurrentUser;
            ProfileName.Text = CurrentUser;
            MainContent.Content = _dashboardView;

            // Load existing history
            LoadExistingHistory();

            // Log application start
            _historyService.AddEntry(CurrentUser, "Application Start", "User logged in");
        }

        private void InitializeViews()
        {
            _dashboardView = new DashboardView();
            _aboutView = new AboutView();

            _addCollegeControl = new AddCollegeControl(_collegeDataService);
            _addProgramControl = new AddProgramControl(_programDataService, _collegeDataService);
            _addStudentControl = new AddStudentControl(_studentDataService, _collegeDataService, _programDataService);

            _viewCollegesControl = new ViewCollegesControl(_collegeDataService);
            _viewProgramsControl = new ViewProgramsControl(_programDataService);
            _viewStudentControl = new ViewStudentControl(_studentDataService);

            // Subscribe to new history entries
            _historyService.NewEntryAdded += HistoryService_NewEntryAdded;
        }

        private void HistoryService_NewEntryAdded(object sender, HistoryEntry entry)
        {
            // Ensure we're on the UI thread
            Dispatcher.Invoke(() =>
            {
                string newEntry = $"[{entry.Timestamp:HH:mm:ss}] {entry.User}: {entry.Action} - {entry.Details}\n";

                // Prepend the new entry (most recent at top)
                RealTimeUpdates.Text = newEntry + RealTimeUpdates.Text;

                // Limit the number of displayed entries
                const int maxLines = 50;
                var lines = RealTimeUpdates.Text.Split('\n');
                if (lines.Length > maxLines)
                {
                    RealTimeUpdates.Text = string.Join("\n", lines.Take(maxLines));
                }
            });
        }

        private void LoadExistingHistory()
        {
            var recentEntries = _historyService.GetHistory()
                .OrderByDescending(h => h.Timestamp)
                .Take(20); // Show last 20 entries

            StringBuilder sb = new StringBuilder();
            foreach (var entry in recentEntries)
            {
                sb.Insert(0, $"[{entry.Timestamp:HH:mm:ss}] {entry.User}: {entry.Action} - {entry.Details}\n");
            }
            RealTimeUpdates.Text = sb.ToString();
        }

        private void SetupEventHandlers()
        {
            // Add College events
            _addCollegeControl.CollegeAdded += (s, e) =>
                _historyService.AddEntry(CurrentUser, "Added College", $"Added college: {e.College.Name} ({e.College.Code})");

            // Add Program events
            _addProgramControl.ProgramAdded += (s, e) =>
                _historyService.AddEntry(CurrentUser, "Added Program", $"Added program: {e.Program.Name} ({e.Program.Code})");

            // Add Student events
            _addStudentControl.StudentAdded += (s, e) =>
            {
                _historyService.AddEntry(CurrentUser, "Added Student", $"Added student: {e.Student.FirstName} {e.Student.LastName}");
                _viewStudentControl.LoadStudents();
            };

            // View College events
            _viewCollegesControl.CollegeDeleted += (s, e) =>
                _historyService.AddEntry(CurrentUser, "Deleted College", $"Deleted college: {e.College.Name}");
            _viewCollegesControl.CollegeUpdated += (s, e) =>
                _historyService.AddEntry(CurrentUser, "Updated College", $"Updated college: {e.College.Name}");
            _viewCollegesControl.CollegesCleared += (s, e) =>
                _historyService.AddEntry(CurrentUser, "Cleared Colleges", "All college data cleared");

            // View Program events
            _viewProgramsControl.ProgramDeleted += (s, e) =>
                _historyService.AddEntry(CurrentUser, "Deleted Program", $"Deleted program: {e.Program.Name}");
            _viewProgramsControl.ProgramUpdated += (s, e) =>
                _historyService.AddEntry(CurrentUser, "Updated Program", $"Updated program: {e.Program.Name}");
            _viewProgramsControl.ProgramsCleared += (s, e) =>
                _historyService.AddEntry(CurrentUser, "Cleared Programs", "All program data cleared");

            // View Student events
            _viewStudentControl.StudentDeleted += (s, e) =>
                _historyService.AddEntry(CurrentUser, "Deleted Student", $"Deleted student: {e.Student.FirstName} {e.Student.LastName}");
            _viewStudentControl.StudentUpdated += (s, e) =>
                _historyService.AddEntry(CurrentUser, "Updated Student", $"Updated student: {e.Student.FirstName} {e.Student.LastName}");
            _viewStudentControl.StudentsCleared += (s, e) =>
                _historyService.AddEntry(CurrentUser, "Cleared Students", "All student data cleared");
        }

        private void UpdateDirectory(string page)
        {
            DirectoryText.Text = $" | /{page}";
            // Remove "Navigation" and just pass the navigation details
            _historyService.AddEntry(CurrentUser, page, $"Navigated to {page}");
            // Or if you prefer just the destination without "Navigated to":
            // _historyService.AddEntry(CurrentUser, page, page);
        }

        // Navigation Event Handlers
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
            _addProgramControl.LoadCollegeCodes();
            MainContent.Content = _addProgramControl;
            UpdateDirectory("Add/Program");
        }

        private void NavigateAddOption3_Click(object sender, RoutedEventArgs e)
        {
            _addStudentControl.LoadProgramCodes();
            MainContent.Content = _addStudentControl;
            UpdateDirectory("Add/Student");
        }

        private void NavigateViewOption1_Click(object sender, RoutedEventArgs e)
        {
            _viewCollegesControl.LoadColleges();
            MainContent.Content = _viewCollegesControl;
            UpdateDirectory("View/Colleges");
        }

        private void NavigateViewOption2_Click(object sender, RoutedEventArgs e)
        {
            _viewProgramsControl.LoadPrograms();
            MainContent.Content = _viewProgramsControl;
            UpdateDirectory("View/Programs");
        }

        private void NavigateViewOption3_Click(object sender, RoutedEventArgs e)
        {
            _viewStudentControl.LoadStudents();
            MainContent.Content = _viewStudentControl;
            UpdateDirectory("View/Students");
        }

        private void NavigateHistory_Click(object sender, RoutedEventArgs e)
        {
            var historyView = new HistoryView(_historyService);
            MainContent.Content = historyView;
            UpdateDirectory("History");
        }

        private void NavigateAbout_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = _aboutView;
            UpdateDirectory("About");
        }

        // Social Media Links
        private void YouTube_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo("https://www.youtube.com/your-channel")
                { UseShellExecute = true });
                _historyService.AddEntry(CurrentUser, "External Link", "Opened YouTube channel");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening YouTube: {ex.Message}", "Error");
                _historyService.AddEntry(CurrentUser, "Error", "Failed to open YouTube channel");
            }
        }

        private void GitHub_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo("https://github.com/your-username")
                { UseShellExecute = true });
                _historyService.AddEntry(CurrentUser, "External Link", "Opened GitHub profile");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening GitHub: {ex.Message}", "Error");
                _historyService.AddEntry(CurrentUser, "Error", "Failed to open GitHub profile");
            }
        }

        private void LinkedIn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo("https://www.linkedin.com/in/your-profile")
                { UseShellExecute = true });
                _historyService.AddEntry(CurrentUser, "External Link", "Opened LinkedIn profile");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening LinkedIn: {ex.Message}", "Error");
                _historyService.AddEntry(CurrentUser, "Error", "Failed to open LinkedIn profile");
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Settings functionality coming soon!", "Settings");
            _historyService.AddEntry(CurrentUser, "Settings", "Attempted to access settings");
        }
    }
}