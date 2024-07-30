using Serilog;
using Serilog.Events;
using Serilog.Sinks;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Windows;

namespace DesktopApplication
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // Initialize Serilog logger
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logFiles\\app_log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            Log.Information("Application starting up");

        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Log.Information("Application Starting Up");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            Log.Information("Application Shutting Down");
            Log.CloseAndFlush();
        }
    }

}
