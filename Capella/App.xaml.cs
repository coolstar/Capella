using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows;

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
    }
}
