using DesktopApplication.Models;
using System.Windows;
using System.Windows.Controls;
using DesktopApplication.Services;
namespace DesktopApplication
{
    /// <summary>
    /// Interaction logic for MainContent.xaml
    /// </summary>
    public partial class MainContent : UserControl
    {
        private GroupManager _groupManager;

        public MainContent(GroupManager groupManager)
        {
            InitializeComponent();
            _groupManager = groupManager;
            LoadData();
        }

        private async void LoadData()
        {
            var courses = await _groupManager.GetAllCoursesAsync(); 
            CourseTreeView.ItemsSource = courses;
        }

        private void CourseTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is Group selectedGroup)
            {
                StudentListView.ItemsSource = selectedGroup.Students;
            }
            else
            {
                StudentListView.ItemsSource = null;
            }
        }
    }
}
