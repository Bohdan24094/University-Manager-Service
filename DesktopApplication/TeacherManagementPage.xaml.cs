using DesktopApplication.Models;
using DesktopApplication.Services;
using System.Windows;
using System.Windows.Controls;
using DesktopApplication.ViewModels;

namespace DesktopApplication
{
    /// <summary>
    /// Interaction logic for TeacherManagementPage.xaml
    /// </summary>
    public partial class TeacherManagementPage : Page
    {
        private readonly TeacherManager _teacherManager;
        public TeacherManagementPage(TeacherManager teacherManager)
        {
            InitializeComponent();
            _teacherManager = teacherManager;
            LoadData();
        }

        private async void LoadData()
        {
            var teachers = await _teacherManager.GetAllTeachersAsync();
            TeacherListBox.ItemsSource = teachers;
        }

        private void TeacherListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TeacherListBox.SelectedItem is Teacher selectedTeacher)
            {
                FirstNameTextBox.Text = selectedTeacher.FirstName;
                LastNameTextBox.Text = selectedTeacher.LastName;
            }
        }

        private async void AddTeacher_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var teacherRecord = new PersonRecord
                {
                    FirstName = FirstNameTextBox.Text,
                    LastName = LastNameTextBox.Text
                };

                await _teacherManager.AddTeacherAsync(teacherRecord);
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding teacher: {ex.Message}");
            }
        }

        private async void UpdateTeacher_Click(object sender, RoutedEventArgs e)
        {
            if (TeacherListBox.SelectedItem is not Teacher selectedTeacher)
            {
                MessageBox.Show("No teacher selected. Please select a teacher before updating.");
            }
            else
            {
                try
                {
                    var teacherRecord = new PersonRecord
                    {
                        FirstName = FirstNameTextBox.Text,
                        LastName = LastNameTextBox.Text
                    };

                    await _teacherManager.UpdateTeacherAsync(selectedTeacher.TeacherId, teacherRecord);
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating teacher: {ex.Message}");
                }
            }
        }

        private async void DeleteTeacher_Click(object sender, RoutedEventArgs e)
        {
            if (TeacherListBox.SelectedItem is not Teacher selectedTeacher)
            {
                MessageBox.Show("No teacher selected. Please select a teacher before deleting.");
            }
            else
            {
                try
                {
                    await _teacherManager.DeleteTeacherAsync(selectedTeacher.TeacherId);
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting teacher: {ex.Message}");
                }
            }
        }
    }
}
