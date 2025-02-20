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
        private ProgramDataService _programDataService;
        private AddCollegeControl _addCollegeControl;
        private AddProgramControl _addProgramControl;
        private ViewCollegesControl _viewCollegesControl;
        private ViewProgramsControl _viewProgramsControl;
        private DashboardView _dashboardView;
        private StudentDataService _studentDataService;
        private AddStudentControl _addStudentControl;
        private ViewStudentControl _viewStudentControl;


        public string CurrentUser { get; set; } = "Admin";

        public MainWindow(string username)
        {
            InitializeComponent();

            CurrentUser = username; // Set the current user from login

            _collegeDataService = new CollegeDataService("colleges.csv") { CurrentUser = CurrentUser };
            _programDataService = new ProgramDataService("programs.csv") { CurrentUser = CurrentUser };

            _addCollegeControl = new AddCollegeControl(_collegeDataService);
            _addProgramControl = new AddProgramControl(_programDataService, _collegeDataService);
            _viewCollegesControl = new ViewCollegesControl(_collegeDataService);
            _viewProgramsControl = new ViewProgramsControl(_programDataService);
            _dashboardView = new DashboardView();
            _studentDataService = new StudentDataService("students.csv") { CurrentUser = CurrentUser };
            _addStudentControl = new AddStudentControl(_studentDataService, _collegeDataService, _programDataService);
            _viewStudentControl = new ViewStudentControl(_studentDataService);

            _addStudentControl.StudentAdded += (s, e) => _viewStudentControl.LoadStudents();

            LoginStatus.Text = CurrentUser;
            ProfileName.Text = CurrentUser;

            MainContent.Content = _dashboardView;
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
            _addStudentControl.LoadProgramCodes();
            MainContent.Content = _addStudentControl;
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
            _viewStudentControl.LoadStudents();
            MainContent.Content = _viewStudentControl;
            UpdateDirectory("View/Students");
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