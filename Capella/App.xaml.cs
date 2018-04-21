using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows;
using Squirrel;
using DesktopNotifications;
using CrashReporterDotNET;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Text;

namespace Capella
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern bool DwmIsCompositionEnabled();

        public static bool isDebugEnabled = false;

        public App()
        {
            ServicePointManager.DefaultConnectionLimit = 5000;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            if (isDebugEnabled)
                AllocConsole();
            Console.WriteLine("Capella Debug Console Started");

            Console.WriteLine("Initializing Capella...");

            if (accountExists())
                this.StartupUri = new Uri("MainWindow.xaml", UriKind.Relative);
            else
                this.StartupUri = new Uri("WelcomeWindow.xaml", UriKind.Relative);

            if (Environment.OSVersion.Version.Major >= 6.0)
                if (!DwmIsCompositionEnabled())
                    MessageBox.Show("Capella looks best with Aero Enabled. It is recommended you turn on Aero when using Capella.");
            // Register COM server and activator type
            DesktopNotificationManagerCompat.RegisterActivator<NotificationsActivator>();

            update();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            Application.Current.DispatcherUnhandledException += DispatcherOnUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
        }

        private void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs unobservedTaskExceptionEventArgs)
        {
            ReportCrash(unobservedTaskExceptionEventArgs.Exception);
            Environment.Exit(0);
        }

        private void DispatcherOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs dispatcherUnhandledExceptionEventArgs)
        {
            ReportCrash(dispatcherUnhandledExceptionEventArgs.Exception);
            Environment.Exit(0);
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            ReportCrash((Exception)unhandledExceptionEventArgs.ExceptionObject);
            Environment.Exit(0);
        }

        public static void ReportCrash(Exception exception, string developerMessage = "")
        {

            byte[] data = Convert.FromBase64String("aGFjaGlkb3JpaUBpY2xvdWQuY29t");
            string decodedString = Encoding.UTF8.GetString(data);
            Console.WriteLine(decodedString);
            var reportCrash = new ReportCrash(decodedString)
            {
                DeveloperMessage = developerMessage
            };
            reportCrash.CaptureScreen = false;
            reportCrash.DoctorDumpSettings = new DoctorDumpSettings
            {
                ApplicationID = new Guid("b0013426-a191-460a-a199-879f2610e69e"),
            };
            reportCrash.Send(exception);
        }

        public bool accountExists()
        {
            new MastodonAPIWrapper();

            if (MastodonAPIWrapper.sharedApiWrapper.accounts == null)
                return false;
            if (MastodonAPIWrapper.sharedApiWrapper.accounts.Count > 0)
                return true;
            return false;
        }
        async static void update()
        {
            using (var mgr = UpdateManager.GitHubUpdateManager("https://github.com/cybercatgurrl/Capella"))
            {
                try
                {
                    await mgr.Result.UpdateApp();
                }
                catch
                {
                    Console.WriteLine("couldn't find update channel");
                }
            }
        }
    }
}
