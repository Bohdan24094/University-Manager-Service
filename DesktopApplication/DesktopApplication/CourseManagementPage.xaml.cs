using DesktopApplication.Services;
using DesktopApplication.ViewModels;
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
using DesktopApplication.Models;

namespace DesktopApplication
{
    /// <summary>
    /// Interaction logic for CourseManagementPage.xaml
    /// </summary>
    public partial class CourseManagementPage : Page
    {
        private readonly CourseManager _courseManager;
        public CourseManagementPage(CourseManager courseManager)
        {
            InitializeComponent();
            _courseManager = courseManager;
            LoadData();
        }
        private async void LoadData()
        {
            var courses = await _courseManager.GetAllTeachersAsync();
            CourseListBox.ItemsSource = courses;
        }

        private async void AddCourse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _courseManager.AddCourseAsync((string)NameTextBox.Text, (string)DescriptionTextBox.Text);
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding course: {ex.Message}");
            }
        }

        private async void UpdateCourse_Click(object sender, RoutedEventArgs e)
        {
            var selectedCourse = CourseListBox.SelectedItem as Course;
            if (selectedCourse != null) {
                await _courseManager.UpdateCourseAsync(selectedCourse.CourseId, (string)NameTextBox.Text, (string)DescriptionTextBox.Text);
                LoadData();
            }
        }

        private async void DeleteCourse_Click(Object sender, RoutedEventArgs e)
        {
            if (CourseListBox.SelectedItem is Course selectedCourse)
            {
                try
                {
                    await _courseManager.DeleteCourseAsync(selectedCourse.CourseId);
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting course: {ex.Message}");
                }
            }
        }

        private void CourseListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedCourse = CourseListBox.SelectedItem as Course;
            if (selectedCourse != null)
            {
                NameTextBox.Text = selectedCourse.Name;
                DescriptionTextBox.Text = selectedCourse.Description;
            }
        }
        private void ReturnToMainWindow_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.NavigateToMainContent();
            }
        }
    }
}
