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

        public MainWindow()
        {
            InitializeComponent();
            _context = new UniversityContext();

            // Initialize Serilog logger
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logFiles\\app_log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            // Create an instance of GroupManager with the logger
            _groupManager = new GroupManager(_context,Log.Logger);


        }

        private void ManageGroups_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new GroupManagementPage(_groupManager));
        }

        public void NavigateToMainContent()
        {
            MainFrame.Navigate(new MainContent(_groupManager));
        }
    }
}