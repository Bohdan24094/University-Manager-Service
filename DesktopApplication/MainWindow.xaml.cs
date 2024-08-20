using DesktopApplication.Models;
using DesktopApplication.Services;
using Serilog;
using System.Windows;

namespace DesktopApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly UniversityContext _context;
        private readonly GroupManager _groupManager;
        private readonly StudentManager _studentManager;
        private readonly TeacherManager _teacherManager;
        private readonly CourseManager _courseManager;
        private readonly ILogger _logger;
        public MainWindow()
        {
            InitializeComponent();
            _context = new UniversityContext();
            // Initialize Serilog logger
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("DesktopApplication\\logs\\app_log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            _logger = Log.Logger;
            // Create an instance of GroupManager with the logger
            _groupManager = new GroupManager(_context,_logger);
            _studentManager = new StudentManager(_context, _logger);
            _teacherManager = new TeacherManager(_context, _logger);
            _courseManager = new CourseManager(_context, _logger);
        }

        private void ClearNavigationHistory()
        {
            var entry = MainFrame.RemoveBackEntry();
            while (entry != null)
            {
                entry = MainFrame.RemoveBackEntry();
            }
        }

        private void ManageGroups_Click(object sender, RoutedEventArgs e)
        {
            ClearNavigationHistory();
            MainFrame.Navigate(new GroupManagementPage(_groupManager));
        }

        private void ManageStudents_Click(object sender, RoutedEventArgs e)
        {
            ClearNavigationHistory();
            MainFrame.Navigate(new StudentManagementPage(_studentManager, _groupManager));
        }

        private void ManageTeachers_Click(object sender, RoutedEventArgs e)
        {
            ClearNavigationHistory();
            MainFrame.Navigate(new TeacherManagementPage(_teacherManager));
        }

        private void ManageCourses_Click(object sender, RoutedEventArgs e)
        {
            ClearNavigationHistory();
            MainFrame.Navigate(new CourseManagementPage(_courseManager));
        }

        public void NavigateToMainContent()
        {
            ClearNavigationHistory();
            MainFrame.Navigate(new MainContent(_groupManager));
        }
    }
}