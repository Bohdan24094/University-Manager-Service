using DesktopApplication.Services;
using System.Windows;
using System.Windows.Controls;
using DesktopApplication.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DesktopApplication
{
    /// <summary>
    /// Interaction logic for CourseManagementPage.xaml
    /// </summary>
    public partial class CourseManagementPage : Page, INotifyPropertyChanged
    {
        private readonly CourseManager _courseManager;
        public ObservableCollection<Course> Courses { get; set; }

        private Course _selectedCourse;
        public Course SelectedCourse
        {
            get => _selectedCourse;
            set
            {
                _selectedCourse = value;
                OnPropertyChanged();
                if (_selectedCourse != null)
                {
                    NameTextBox.Text = _selectedCourse.Name;
                    DescriptionTextBox.Text = _selectedCourse.Description;
                }
            }
        }

        public CourseManagementPage(CourseManager courseManager)
        {
            InitializeComponent();
            _courseManager = courseManager;
            Courses = new ObservableCollection<Course>();
            DataContext = this;
            LoadData();
        }

        private async void LoadData()
        {
            var courses = await _courseManager.GetAllCoursesAsync();
            Courses.Clear();
            foreach (var course in courses)
            {
                Courses.Add(course);
            }
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
