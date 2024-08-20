using System.Windows;
using System.Windows.Controls;
using DesktopApplication.Models;
using DesktopApplication.Services;
using DesktopApplication.ViewModels;

namespace DesktopApplication
{
    /// <summary>
    /// Interaction logic for StudentManagementPage.xaml
    /// </summary>
    public partial class StudentManagementPage : Page
    {
        private readonly StudentManager _studentManager;
        private readonly GroupManager _groupManager;

        public StudentManagementPage(StudentManager studentManager, GroupManager groupManager)
        {
            InitializeComponent();
            _studentManager = studentManager;
            _groupManager = groupManager;
            LoadData();
        }

        private async void LoadData()
        {
            var students = await _studentManager.GetAllStudentsAsync();
            StudentListBox.ItemsSource = students;

            var groups = await _groupManager.GetAllGroupsAsync();
            GroupComboBox.ItemsSource = groups;
        }

        private void StudentListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StudentListBox.SelectedItem is Student selectedStudent)
            {
                FirstNameTextBox.Text = selectedStudent.FirstName;
                LastNameTextBox.Text = selectedStudent.LastName;
                GroupComboBox.SelectedValue = selectedStudent.Group.GroupId;
            }
        }

        private async void AddStudent_Click(object sender, RoutedEventArgs e)
        {
            if (GroupComboBox.SelectedValue != null)
            {
                try
                {
                    var studentRecord = new PersonRecord
                    {
                        FirstName = FirstNameTextBox.Text,
                        LastName = LastNameTextBox.Text
                    };

                    await _studentManager.AddStudentAsync(studentRecord, (int)GroupComboBox.SelectedValue);
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding student: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show($"Please select group for student");
            }

        }

        private async void UpdateStudent_Click(object sender, RoutedEventArgs e)
        {
            if (StudentListBox.SelectedItem is Student selectedStudent)
            {
                try
                {
                    var studentRecord = new PersonRecord
                    {
                        FirstName = FirstNameTextBox.Text,
                        LastName = LastNameTextBox.Text
                    };

                    await _studentManager.UpdateStudentAsync(selectedStudent.StudentId, studentRecord);
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating student: {ex.Message}");
                }
            }
        }

        private async void DeleteStudent_Click(object sender, RoutedEventArgs e)
        {
            if (StudentListBox.SelectedItem is Student selectedStudent)
            {
                try
                {
                    await _studentManager.DeleteStudentAsync(selectedStudent.StudentId);
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting student: {ex.Message}");
                }
            }
        }
    }
}
