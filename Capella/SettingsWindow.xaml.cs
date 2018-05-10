using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Capella.Models;
using Capella.Properties;
using Microsoft.Win32;

namespace Capella
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public MastodonAPIWrapper apiWrapper;
        private List<AccountUIHandler> accountUIHandlers;

        public SettingsWindow()
        {
            InitializeComponent();

            this.apiWrapper = MastodonAPIWrapper.sharedApiWrapper;

            int topMargin = 43;

            accountUIHandlers = new List<AccountUIHandler>();

            foreach (Account twitterAccount in MastodonAPIWrapper.sharedApiWrapper.accounts)
            {
                AccountUIHandler accountUIHandler = new AccountUIHandler();
                accountUIHandler.twitterAccountToken = twitterAccount.accessToken;

                Button accountButton = new Button();
                accountButton.ClipToBounds = false;
                accountButton.HorizontalAlignment = HorizontalAlignment.Left;
                accountButton.VerticalAlignment = VerticalAlignment.Top;
                accountButton.Height = 50;
                accountButton.Margin = new Thickness(13, topMargin, 0, 0);
                accountButton.Style = Resources["FlatButton"] as Style;
                accountButton.Click += Account_Click;
                accountButton.Cursor = Cursors.Hand;
                this.sidebarGrid.Children.Add(accountButton);
                accountUIHandler.accountButton = accountButton;
                topMargin += 60;

                Image accountImage = new Image();
                accountImage.Clip = new RectangleGeometry(new Rect(0, 0, 50, 50), 4, 4);
                accountButton.Content = accountImage;
                accountUIHandler.accountImage = accountImage;

                accountImage.Opacity = 0;
                this.apiWrapper.getProfileAvatar(twitterAccount, accountImage);

                accountUIHandlers.Add(accountUIHandler);
            }

            Button homeBtn = new Button();
            homeBtn.HorizontalAlignment = HorizontalAlignment.Left;
            homeBtn.VerticalAlignment = VerticalAlignment.Top;
            homeBtn.Style = Resources["FlatTab"] as Style;
            homeBtn.Height = 40;
            homeBtn.Margin = new Thickness(4, topMargin, 0, 0);
            homeBtn.Cursor = Cursors.Hand;
            this.sidebarGrid.Children.Add(homeBtn);

            TabImage tabImage = new TabImage();
            tabImage.Height = 30;
            tabImage.Width = 38;
            tabImage.Source = new BitmapImage(new Uri("Resources/add.png", UriKind.Relative));
            tabImage.VerticalAlignment = VerticalAlignment.Center;
            tabImage.HorizontalAlignment = HorizontalAlignment.Center;
            tabImage.Margin = new Thickness(2, 1, 20, 1);
            homeBtn.Content = tabImage;
        }

        private void AddAccount_Click(object sender, RoutedEventArgs e)
        {
            WelcomeWindow welcomeWindow = new WelcomeWindow();
            welcomeWindow.Show();
        }

        private void Account_Click(object sender, RoutedEventArgs e)
        {
            Button accountButton = (Button)sender;

            AccountUIHandler accountUIHandler = null;
            foreach (AccountUIHandler uiHandler in accountUIHandlers)
            {
                if (uiHandler.accountButton == accountButton)
                {
                    accountUIHandler = uiHandler;
                    break;
                }
            }
            if (accountUIHandler.twitterAccount.accessToken.Equals(MastodonAPIWrapper.sharedApiWrapper.selectedAccount.accessToken))
                return;

            MastodonAPIWrapper.sharedApiWrapper.selectedAccount = accountUIHandler.twitterAccount;

            foreach (AccountUIHandler accountUI in accountUIHandlers)
            {
                Account twitterAccount = accountUI.twitterAccount;

                DoubleAnimation accountAnimOpacity = new DoubleAnimation();
                Storyboard.SetTarget(accountAnimOpacity, accountUI.accountImage);
                Storyboard.SetTargetProperty(accountAnimOpacity, new PropertyPath(UserControl.OpacityProperty));
                accountAnimOpacity.From = accountUI.accountImage.Opacity;
                if (twitterAccount.accessToken.Equals(MastodonAPIWrapper.sharedApiWrapper.selectedAccount.accessToken))
                {
                    accountAnimOpacity.To = 1.0;
                }
                else
                {
                    accountAnimOpacity.To = 0.5;
                }

                Storyboard storyboard = new Storyboard();
                //storyboard.Children.Add(accountAnim);
                storyboard.Children.Add(accountAnimOpacity);
                storyboard.SpeedRatio *= 5;
                storyboard.Begin();
            }
        }

        private void SettingChanged(object sender, RoutedEventArgs e)
        {
            SetStartup();
            Settings.Default.Save();
        }

        /// <summary>
        /// Sets the application on startup depending on whether the setting is enabled
        /// </summary>
        private void SetStartup()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (Settings.Default.startup)
            {
                rk.SetValue(Process.GetCurrentProcess().ProcessName, System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
            else { 
                rk.DeleteValue(Process.GetCurrentProcess().ProcessName, false);
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
