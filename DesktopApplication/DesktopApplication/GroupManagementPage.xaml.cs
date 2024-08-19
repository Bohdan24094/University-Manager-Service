using DesktopApplication.Models;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using DesktopApplication.Services;

namespace DesktopApplication
{
    /// <summary>
    /// Interaction logic for GroupManagementPage.xaml
    /// </summary>
    public partial class GroupManagementPage : Page
    {
        private readonly GroupManager _groupManager;
        public GroupManagementPage(GroupManager groupManager)
        {
            InitializeComponent();
            _groupManager = groupManager;
            LoadData();
        }
        private async void LoadData()
        {
            var groups =await _groupManager.GetAllGroupsAsync();
            GroupListBox.ItemsSource = groups;

            var teachers = await _groupManager.GetAllTeachersAsync();
            TeacherComboBox.ItemsSource = teachers;

            var courses = await _groupManager.GetAllCoursesAsync();
            CourseComboBox.ItemsSource = courses;
        }

        private async void CreateGroup_Click(object sender, RoutedEventArgs e)
        {
            if ( CourseComboBox.SelectedValue!= null && TeacherComboBox.SelectedValue!=null)
            {
                await _groupManager.CreateGroupAsync(GroupNameTextBox.Text, (int)CourseComboBox.SelectedValue, (int)TeacherComboBox.SelectedValue);
                LoadData();
            }
            else
            {
                MessageBox.Show("Please choose the existing course and teacher to attach");

            }
        }

        private async void DeleteGroup_Click(object sender, RoutedEventArgs e)
        {
            var selectedGroup = GroupListBox.SelectedItem as Group;
            if (selectedGroup != null)
            {
                await _groupManager.DeleteGroupAsync(selectedGroup.GroupId);
                LoadData();
            }
        }

        private async void UpdateGroup_Click(object sender, RoutedEventArgs e)
        {
            var selectedGroup = GroupListBox.SelectedItem as Group;
            if (selectedGroup != null)
            {
                await _groupManager.UpdateGroupAsync(selectedGroup.GroupId, GroupNameTextBox.Text, (int)CourseComboBox.SelectedValue, (int)TeacherComboBox.SelectedValue);
               LoadData();
            }
        }

        private void GroupListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedGroup = GroupListBox.SelectedItem as Group;
            if (selectedGroup != null)
            {
                GroupNameTextBox.Text = selectedGroup.Name;
                TeacherComboBox.SelectedValue = selectedGroup.Teacher.TeacherId;
                CourseComboBox.SelectedValue = selectedGroup.CourseId;
            }
        }

        private void ExportStudents_Click(object sender, RoutedEventArgs e)
        {
            var selectedGroup = GroupListBox.SelectedItem as Group;
            if (selectedGroup == null) return;

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV file (*.csv)|*.csv"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                _groupManager.ExportStudents(selectedGroup.GroupId, saveFileDialog.FileName);
            }
        }

        private async void ImportStudents_Click(object sender, RoutedEventArgs e)
        {
            var selectedGroup = GroupListBox.SelectedItem as Group;
            if (selectedGroup == null) return;

            await _groupManager.ClearGroupAsync(selectedGroup.GroupId);

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "CSV file (*.csv)|*.csv"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                await _groupManager.ImportStudentsAsync(selectedGroup.GroupId, openFileDialog.FileName);
                LoadData();
            }
        }

        private void GenerateDocx_Click(object sender, RoutedEventArgs e)
        {
            var selectedGroup = GroupListBox.SelectedItem as Group;
            if (selectedGroup == null) return;
            
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Word Document (*.docx)|*.docx"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                _groupManager.GenerateDocx(selectedGroup.GroupId, saveFileDialog.FileName);
            }
        }

        private void GeneratePdf_Click(object sender, RoutedEventArgs e)
        {
            var selectedGroup = GroupListBox.SelectedItem as Group;
            if (selectedGroup == null) return;
        
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Document (*.pdf)|*.pdf"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                _groupManager.GeneratePdf(selectedGroup.GroupId, saveFileDialog.FileName);
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
