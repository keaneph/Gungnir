using System.Collections.Generic;
using System.Windows.Controls;

namespace glaive.Views
{
    public partial class ViewCollegesControl : UserControl
    {
        private CollegeDataService _collegeDataService;

        public ViewCollegesControl(CollegeDataService collegeDataService)
        {
            InitializeComponent();
            _collegeDataService = collegeDataService;
            LoadColleges();
        }

        public void LoadColleges()
        {
            List<College> colleges = _collegeDataService.GetAllColleges();
            CollegeListView.ItemsSource = colleges; // Directly set the ItemsSource
        }
    }
}