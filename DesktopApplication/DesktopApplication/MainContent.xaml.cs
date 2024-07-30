using DesktopApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.EntityFrameworkCore;
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
                // Display selected group's students in the ListView
                StudentListView.ItemsSource = selectedGroup.Students;
            }
            else
            {
                StudentListView.ItemsSource = null;
            }
        }
    }
}
