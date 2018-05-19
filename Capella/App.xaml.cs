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
using System.IO;
using Hardcodet.Wpf.TaskbarNotification;
using System.Threading;
using System.Diagnostics;

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

        [DllImport("user32.dll")]
        static extern bool AllowSetForegroundWindow(int dwProcessId);

        [DllImport("user32.dll")]
        private static extern int OpenIcon(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr FindWindow(string cls, string win);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private TaskbarIcon trayIcon;

        public static bool isDebugEnabled = false;
        private bool createdNew;
        private Mutex mutex;

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

            string updateExeLocation = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(typeof(App).Assembly.Location)));
            if (File.Exists(updateExeLocation))
            {
                // Don't try to update with Squirrel if we weren't deployed by Squirrel.
                update(); 
            }

            // Set the current user interface culture to the specific culture French
            //System.Threading.Thread.CurrentThread.CurrentUICulture =
            //            new System.Globalization.CultureInfo("fr");
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // there is a chance the user tries to click on the icon repeatedly
            // and the process cannot be discovered yet
            mutex = new Mutex(true, "Capella",
               out createdNew);  // must be a variable, though it is unused - 
            // we just need a bit of time until the process shows up
            if (!createdNew)
            {
                ActivateOtherWindow();
                Current.Shutdown();
                return;
            }

            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            Application.Current.DispatcherUnhandledException += DispatcherOnUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
            trayIcon = (TaskbarIcon)FindResource("TrayIcon");
        }

        private static void ActivateOtherWindow()
        {
            var other = FindWindow(null, "Capella");
            if (other != IntPtr.Zero)
            {
                // use SW_RESTORE to restore
                ShowWindow(other, 9);
                SetForegroundWindow(other);
                if (IsIconic(other))
                    OpenIcon(other);
            }
        }

        public static Process PriorProcess()
        // Returns a System.Diagnostics.Process pointing to
        // a pre-existing process with the same name as the
        // current one, if any; or null if the current process
        // is unique.
        {
            Process curr = Process.GetCurrentProcess();
            Process[] procs = Process.GetProcessesByName(curr.ProcessName);
            foreach (Process p in procs)
            {
                if ((p.Id != curr.Id) &&
                    (p.MainModule.FileName == curr.MainModule.FileName))
                    return p;
            }
            return null;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            trayIcon.Dispose();
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
                await mgr.Result.UpdateApp();
            }
        }
    }

    /// <summary>
    /// Enumeration of the different ways of showing a window.</summary>
    internal enum WindowShowStyle : uint
    {
        /// <summary>Hides the window and activates another window.</summary>
        /// <remarks>See SW_HIDE</remarks>
        Hide = 0,
        /// <summary>Activates and displays a window. If the window ..
        /// or maximized, the system restores it to its original size and
        /// position. An application should specify this flag when displaying
        /// the window for the first time.</summary>
        /// <remarks>See SW_SHOWNORMAL</remarks>
        ShowNormal = 1,
        /// <summary>Activates the window and displays it ..</summary>
        /// <remarks>See SW_SHOWMINIMIZED</remarks>
        ShowMinimized = 2,
        /// <summary>Activates the window and displays it ..</summary>
        /// <remarks>See SW_SHOWMAXIMIZED</remarks>
        ShowMaximized = 3,
        /// <summary>Maximizes the specified window.</summary>
        /// <remarks>See SW_MAXIMIZE</remarks>
        Maximize = 3,
        /// <summary>Displays a window in its most recent size and position.
        /// This value is similar to "ShowNormal", except the window is not
        /// actived.</summary>
        /// <remarks>See SW_SHOWNOACTIVATE</remarks>
        ShowNormalNoActivate = 4,
        /// <summary>Activates the window and displays it in its current size
        /// and position.</summary>
        /// <remarks>See SW_SHOW</remarks>
        Show = 5,
        /// <summary>Minimizes the specified window and activates the next
        /// top-level window in the Z order.</summary>
        /// <remarks>See SW_MINIMIZE</remarks>
        Minimize = 6,
        /// <summary>Displays the window as a minimized window. This value is
        /// similar to "ShowMinimized", except the window ..</summary>
        /// <remarks>See SW_SHOWMINNOACTIVE</remarks>
        ShowMinNoActivate = 7,
        /// <summary>Displays the window in its current size and position. This
        /// value is similar to "Show", except the window ..</summary>
        /// <remarks>See SW_SHOWNA</remarks>
        ShowNoActivate = 8,
        /// <summary>Activates and displays the window. If the window is
        /// minimized or maximized, the system restores it to its original size
        /// and position. An application should specify this flag ..
        /// a minimized window.</summary>
        /// <remarks>See SW_RESTORE</remarks>
        Restore = 9,
        /// <summary>Sets the show state based on the SW_ value specified ..
        /// STARTUPINFO structure passed to the CreateProcess function by the
        /// program that started the application.</summary>
        /// <remarks>See SW_SHOWDEFAULT</remarks>
        ShowDefault = 10,
        /// <summary>Windows 2000/XP: Minimizes a window, even if the thread
        /// that owns the window is hung. This flag should only be used when
        /// minimizing windows from a different thread.</summary>
        /// <remarks>See SW_FORCEMINIMIZE</remarks>
        ForceMinimized = 11
    }
}