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
        private UniversityContext _context;
        private GroupManager _groupManager;
        private StudentManager _studentManager;
        private TeacherManager _teacherManager;
        private ILogger _logger;
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


        }

        private void ManageGroups_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new GroupManagementPage(_groupManager));
        }

        private void ManageStudents_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new StudentManagementPage(_studentManager, _groupManager));
        }
        private void ManageTeachers_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new TeacherManagementPage(_teacherManager));
        }

        public void NavigateToMainContent()
        {
            MainFrame.Navigate(new MainContent(_groupManager));
        }
    }
}