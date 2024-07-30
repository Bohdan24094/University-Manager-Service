using DesktopApplication.Models;
using DesktopApplication.Services;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            // Navigate to GroupManagementPage
            MainFrame.Navigate(new GroupManagementPage(_groupManager));
        }

        // Add a method to navigate back to the main content
        public void NavigateToMainContent()
        {
            MainFrame.Navigate(new MainContent(_groupManager));
        }
    }
}