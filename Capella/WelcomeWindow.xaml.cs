using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Capella
{
    /// <summary>
    /// Interaction logic for WelcomeWindow.xaml
    /// </summary>
    public partial class WelcomeWindow : Window
    {
        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern void DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern bool DwmIsCompositionEnabled();

        public bool authenticated;
        public String accountToken;

        public WelcomeWindow()
        {
            InitializeComponent();
            authenticated = false;
            accountToken = "";
        }

        private void OnContentRendered(object sender, EventArgs e)
        {
            try
            {
                // Obtain the window handle for WPF application
                IntPtr mainWindowPtr = new WindowInteropHelper(this).Handle;
                HwndSource mainWindowSrc = HwndSource.FromHwnd(mainWindowPtr);
                mainWindowSrc.CompositionTarget.BackgroundColor = Color.FromArgb(0, 0, 0, 0);

                // Set Margins
                MARGINS margins = new MARGINS();

                // Extend glass frame into client area 
                // Note that the default desktop Dpi is 96dpi. The  margins are 
                // adjusted for the system Dpi.
                margins.Top = -1;
                margins.Left = -1;
                margins.Right = -1;
                margins.Bottom = -1;

                DwmExtendFrameIntoClientArea(mainWindowSrc.Handle, ref margins);

                this.Background = new SolidColorBrush(Color.FromArgb(128, 255, 255, 255));
            }
            // If not Vista, paint background white. 
            catch (Exception)
            {
            }
        }

        private void serialNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (serialNumber.Text != "")
            {
                signInBtn.IsEnabled = true;
            }
            else
            {
                signInBtn.IsEnabled = false;
            }
        }

        private void signInBtn_Click(object sender, RoutedEventArgs e)
        {
            signIn();
        }

        void signIn()
        {
            new MastodonAPIWrapper();
            OAuthUtils oAuthUtils = MastodonAPIWrapper.sharedApiWrapper.sharedOAuthUtils;

            SignInWindow signInWindow = new SignInWindow();
            signInWindow.callbackDelegate = this;
            signInWindow.ShowDialog();

            if (this.authenticated == true)
            {
                BackgroundWorker worker2 = new BackgroundWorker();
                worker2.DoWork += (sender2, e2) =>
                {
                    if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Capella"))
                    {
                        Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Capella");
                    }
                    JObject json = new JObject();

                    JArray accounts = new JArray();

                    if (MastodonAPIWrapper.sharedApiWrapper.accounts != null)
                    {
                        foreach (Account twitterAccount in MastodonAPIWrapper.sharedApiWrapper.accounts)
                        {
                            JObject rawAccount = new JObject();
                            if (twitterAccount.accessToken.Equals(accountToken))
                                return;
                            rawAccount.Add("token", twitterAccount.accessToken);
                            accounts.Add(rawAccount);
                        }
                    }

                    JObject account = new JObject();
                    account.Add("token", accountToken);
                    accounts.Add(account);

                    json.Add("accounts", accounts);

                    String output = json.ToString();
                    File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Capella\\settings.json", output);
                };
                worker2.RunWorkerCompleted += (sender2, e2) =>
                {
                    System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                    Application.Current.Shutdown();
                };
                worker2.RunWorkerAsync();
            }
        }
    }
}
