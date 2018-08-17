using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Capella.Models;
using Capella.Properties;
using System.Linq;

namespace Capella
{
    public struct MARGINS
    {
        public int Left;
        public int Right;
        public int Top;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DwmBlurbehind
    {
        public CoreNativeMethods.DwmBlurBehindDwFlags dwFlags;
        public bool Enabled;
        public IntPtr BlurRegion;
        public bool TransitionOnMaximized;
    }

    public static class CoreNativeMethods
    {
        public enum DwmBlurBehindDwFlags
        {
            DwmBbEnable = 1,
            DwmBbBlurRegion = 2,
            DwmBbTransitionOnMaximized = 4
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    internal enum WindowCompositionAttribute
    {
        // ...
        WCA_ACCENT_POLICY = 19
        // ...
    }

    internal enum AccentState
    {
        ACCENT_DISABLED = 0,
        ACCENT_ENABLE_GRADIENT = 1,
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_INVALID_STATE = 4
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AccentPolicy
    {
        public AccentState AccentState;
        public int AccentFlags;
        public int GradientColor;
        public int AnimationId;
    }


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern void DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern bool DwmIsCompositionEnabled();

        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern void DwmEnableBlurBehindWindow(IntPtr hwnd, ref DwmBlurbehind blurBehind);

        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);


        public MARGINS margins;

        [StructLayout(LayoutKind.Sequential)]
        public struct DWMCOLORIZATIONPARAMS
        {
            public uint ColorizationColor,
                ColorizationAfterglow,
                ColorizationColorBalance,
                ColorizationAfterglowBalance,
                ColorizationBlurBalance,
                ColorizationGlassReflectionIntensity,
                ColorizationOpaqueBlend;
        }

        [DllImport("dwmapi.dll", EntryPoint = "#127")]
        internal static extern void DwmGetColorizationParameters(ref DWMCOLORIZATIONPARAMS dwmparams);

        public MastodonAPIWrapper apiWrapper;
        public static MainWindow sharedMainWindow;
        private TabsController tabController;

        private List<AccountUIHandler> accountUIHandlers;

        public MainWindow()
        {
            InitializeComponent();

            // controlled by settings :)
            if (Settings.Default.startMinimized)
            {
                Application.Current.MainWindow.WindowState = WindowState.Minimized;
            }

            if (Environment.OSVersion.Version.Minor == 0 || Environment.OSVersion.Version.Minor == 1) //Vista and Win7
            {
                //roundCornersHandler.RadiusX = 10;
                //roundCornersHandler.RadiusY = 10;
                mainGrid.Background = new SolidColorBrush(Color.FromArgb(64, 0, 0, 0));
            }

            this.apiWrapper = MastodonAPIWrapper.sharedApiWrapper ?? new MastodonAPIWrapper();

            new NotificationsHandler();
            sharedMainWindow = this;

            closeBtn_MouseDown(null, null);
            minimizeBtn_MouseDown(null, null);

            this.Show();

            tabController = new TabsController();
            tabController.parentGrid = this.mainGrid;
            tabController.parentWindow = this;
            tabController.ClipToBounds = true;
            tabController.Margin = new Thickness(77, 1, 1, 1);
            tabController.VerticalAlignment = VerticalAlignment.Stretch;
            tabController.HorizontalAlignment = HorizontalAlignment.Left;
            tabController.Background = Brushes.Transparent;
            tabController.Width = Math.Max(0, this.RenderSize.Width - 77);

            tabController.windowWidth = this.RenderSize.Width;

            //tabController.Width = this.RenderSize.Width - 97;
            //tabController.Height = this.RenderSize.Height - 20;
            this.mainGrid.Children.Add(tabController);

            
            TimelinePanel timelinePanel;

            NavController navController;

            int topMargin = 43;

            accountUIHandlers = new List<AccountUIHandler>();

            if (!MastodonAPIWrapper.sharedApiWrapper.accounts.Any())
            {
                new WelcomeWindow().ShowDialog();
                apiWrapper = new MastodonAPIWrapper();
            }

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

                #region Home
                Button homeBtn = new Button();
                homeBtn.HorizontalAlignment = HorizontalAlignment.Left;
                homeBtn.VerticalAlignment = VerticalAlignment.Top;
                homeBtn.Style = Resources["FlatTab"] as Style;
                homeBtn.Height = 40;
                homeBtn.Margin = new Thickness(4, topMargin, 0, 0);
                homeBtn.Cursor = Cursors.Hand;
                this.sidebarGrid.Children.Add(homeBtn);
                if (twitterAccount.accessToken.Equals(MastodonAPIWrapper.sharedApiWrapper.selectedAccount.accessToken))
                {
                    topMargin += 50;
                } else
                {
                    homeBtn.Opacity = 0;
                }
                accountUIHandler.homeBtn = homeBtn;

                TabImage tabImage = new TabImage();
                tabImage.Height = 30;
                tabImage.Width = 38;
                tabImage.Source = new BitmapImage(new Uri("Resources/sidebar_home.png", UriKind.Relative));
                tabImage.VerticalAlignment = VerticalAlignment.Center;
                tabImage.HorizontalAlignment = HorizontalAlignment.Center;
                tabImage.Margin = new Thickness(2, 1, 20, 1);
                homeBtn.Content = tabImage;

                timelinePanel = new TimelinePanel();
                timelinePanel.twitterAccountToken = twitterAccount.accessToken;
                //timelinePanel.refreshTimeline();
                navController = new NavController(timelinePanel);

                navController.Margin = new Thickness(0);
                tabController.addControl(navController, homeBtn);

                #endregion
                #region Mentions
                Button mentionsBtn = new Button();
                mentionsBtn.HorizontalAlignment = HorizontalAlignment.Left;
                mentionsBtn.VerticalAlignment = VerticalAlignment.Top;
                mentionsBtn.Style = Resources["FlatTab"] as Style;
                mentionsBtn.Height = 40;
                mentionsBtn.Margin = new Thickness(4, topMargin, 0, 0);
                mentionsBtn.Cursor = Cursors.Hand;
                this.sidebarGrid.Children.Add(mentionsBtn);
                if (twitterAccount.accessToken.Equals(MastodonAPIWrapper.sharedApiWrapper.selectedAccount.accessToken))
                {
                    topMargin += 50;
                }
                else
                {
                    mentionsBtn.Opacity = 0;
                }
                accountUIHandler.mentionsBtn = mentionsBtn;

                tabImage = new TabImage();
                tabImage.Height = 30;
                tabImage.Width = 38;
                tabImage.Source = new BitmapImage(new Uri("Resources/sidebar_notifications.png", UriKind.Relative));
                tabImage.VerticalAlignment = VerticalAlignment.Center;
                tabImage.HorizontalAlignment = HorizontalAlignment.Center;
                tabImage.Margin = new Thickness(2, 1, 20, 1);
                mentionsBtn.Content = tabImage;

                timelinePanel = new TimelinePanel();
                timelinePanel.twitterAccountToken = twitterAccount.accessToken;
                timelinePanel.setTitle(Strings.Mentions);
                timelinePanel.timelineType = "mentions";
                //timelinePanel.refreshTimeline();

                navController = new NavController(timelinePanel);

                navController.Margin = new Thickness(0);
                tabController.addControl(navController, mentionsBtn);

                #endregion
                #region Public
                Button publicBtn = new Button();
                publicBtn.HorizontalAlignment = HorizontalAlignment.Left;
                publicBtn.VerticalAlignment = VerticalAlignment.Top;
                publicBtn.Style = Resources["FlatTab"] as Style;
                publicBtn.Height = 40;
                publicBtn.Margin = new Thickness(4, topMargin, 0, 0);
                publicBtn.Cursor = Cursors.Hand;
                this.sidebarGrid.Children.Add(publicBtn);
                if (twitterAccount.accessToken.Equals(MastodonAPIWrapper.sharedApiWrapper.selectedAccount.accessToken))
                {
                    topMargin += 50;
                }
                else
                {
                    publicBtn.Opacity = 0;
                }
                accountUIHandler.publicBtn = publicBtn;

                tabImage = new TabImage();
                tabImage.Height = 30;
                tabImage.Width = 38;
                tabImage.Source = new BitmapImage(new Uri("Resources/sidebar_public.png", UriKind.Relative));
                tabImage.VerticalAlignment = VerticalAlignment.Center;
                tabImage.HorizontalAlignment = HorizontalAlignment.Center;
                tabImage.Margin = new Thickness(2, 1, 20, 1);
                publicBtn.Content = tabImage;

                timelinePanel = new TimelinePanel();
                timelinePanel.twitterAccountToken = twitterAccount.accessToken;
                timelinePanel.setTitle(Strings.PublicTimeline);
                timelinePanel.timelineType = "public";
                //timelinePanel.refreshTimeline();
                navController = new NavController(timelinePanel);

                navController.Margin = new Thickness(0);
                tabController.addControl(navController, publicBtn);

                #endregion
                #region Direct
                Button directBtn = new Button();
                directBtn.HorizontalAlignment = HorizontalAlignment.Left;
                directBtn.VerticalAlignment = VerticalAlignment.Top;
                directBtn.Style = Resources["FlatTab"] as Style;
                directBtn.Height = 40;
                directBtn.Margin = new Thickness(4, topMargin, 0, 0);
                directBtn.Cursor = Cursors.Hand;
                this.sidebarGrid.Children.Add(directBtn);
                if (twitterAccount.accessToken.Equals(MastodonAPIWrapper.sharedApiWrapper.selectedAccount.accessToken))
                {
                    topMargin += 50;
                }
                else
                {
                    directBtn.Opacity = 0;
                }
                accountUIHandler.directBtn = directBtn;

                tabImage = new TabImage();
                tabImage.Height = 34;
                tabImage.Width = 38;
                tabImage.Source = new BitmapImage(new Uri("Resources/sidebar_messages.png", UriKind.Relative));
                tabImage.VerticalAlignment = VerticalAlignment.Center;
                tabImage.HorizontalAlignment = HorizontalAlignment.Center;
                tabImage.Margin = new Thickness(2, 1, 20, 1);
                directBtn.Content = tabImage;

                timelinePanel = new TimelinePanel();
                timelinePanel.twitterAccountToken = twitterAccount.accessToken;
                timelinePanel.setTitle(Strings.DirectTimeline);
                timelinePanel.timelineType = "direct";
                //timelinePanel.refreshTimeline();
                navController = new NavController(timelinePanel);

                navController.Margin = new Thickness(0);
                tabController.addControl(navController, directBtn);
                #endregion
                #region Account
                Button userBtn = new Button();
                userBtn.HorizontalAlignment = HorizontalAlignment.Left;
                userBtn.VerticalAlignment = VerticalAlignment.Top;
                userBtn.Style = Resources["FlatTab"] as Style;
                userBtn.Height = 40;
                userBtn.Margin = new Thickness(4, topMargin, 0, 0);
                userBtn.Cursor = Cursors.Hand;
                this.sidebarGrid.Children.Add(userBtn);
                if (twitterAccount.accessToken.Equals(MastodonAPIWrapper.sharedApiWrapper.selectedAccount.accessToken))
                {
                    topMargin += 50;
                }
                else
                {
                    userBtn.Opacity = 0;
                }

                tabImage = new TabImage();
                tabImage.Height = 30;
                tabImage.Width = 38;
                tabImage.Source = new BitmapImage(new Uri("Resources/sidebar_profile.png", UriKind.Relative));
                tabImage.VerticalAlignment = VerticalAlignment.Center;
                tabImage.HorizontalAlignment = HorizontalAlignment.Center;
                tabImage.Margin = new Thickness(2, 1, 20, 1);
                userBtn.Content = tabImage;
                accountUIHandler.userButton = userBtn;

                ProfilePanel profilePanel = new ProfilePanel();
                profilePanel.twitterAccountToken = twitterAccount.accessToken;
                //profilePanel.refreshProfile();

                navController = new NavController(profilePanel);

                navController.Margin = new Thickness(0);
                tabController.addControl(navController, userBtn);

                #endregion

                #region Search
                Button searchBtn = new Button();
                searchBtn.HorizontalAlignment = HorizontalAlignment.Left;
                searchBtn.VerticalAlignment = VerticalAlignment.Top;
                searchBtn.Style = Resources["FlatTab"] as Style;
                searchBtn.Height = 40;
                searchBtn.Margin = new Thickness(4, topMargin, 0, 0);
                searchBtn.Cursor = Cursors.Hand;
                this.sidebarGrid.Children.Add(searchBtn);
                if (twitterAccount.accessToken.Equals(MastodonAPIWrapper.sharedApiWrapper.selectedAccount.accessToken))
                {
                    topMargin += 50;
                }
                else
                {
                    searchBtn.Opacity = 0;
                }
                accountUIHandler.searchBtn = searchBtn;

                tabImage = new TabImage();
                tabImage.Height = 30;
                tabImage.Width = 38;
                tabImage.Source = new BitmapImage(new Uri("Resources/sidebar_search.png", UriKind.Relative));
                tabImage.VerticalAlignment = VerticalAlignment.Center;
                tabImage.HorizontalAlignment = HorizontalAlignment.Center;
                tabImage.Margin = new Thickness(2, 1, 20, 1);
                searchBtn.Content = tabImage;

                SearchPanel searchPanel = new SearchPanel();
                searchPanel.twitterAccountToken = twitterAccount.accessToken;
                navController = new NavController(searchPanel);

                navController.Margin = new Thickness(0);
                tabController.addControl(navController, searchBtn);
                #endregion

                accountUIHandlers.Add(accountUIHandler);
            }
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
            tabController.setSelectedControl(tabController.buttons.IndexOf(accountUIHandler.homeBtn));

            int topMargin = 43;
            foreach (AccountUIHandler accountUI in accountUIHandlers)
            {
                Account twitterAccount = accountUI.twitterAccount;

                ThicknessAnimation accountAnim = new ThicknessAnimation();
                Storyboard.SetTarget(accountAnim, accountUI.accountButton);
                Storyboard.SetTargetProperty(accountAnim, new PropertyPath(UserControl.MarginProperty));
                accountAnim.From = accountUI.accountButton.Margin;
                accountAnim.To = new Thickness(13, topMargin, 0, 0);

                DoubleAnimation accountAnimOpacity = new DoubleAnimation();
                Storyboard.SetTarget(accountAnimOpacity, accountUI.accountImage);
                Storyboard.SetTargetProperty(accountAnimOpacity, new PropertyPath(UserControl.OpacityProperty));
                accountAnimOpacity.From = accountUI.accountImage.Opacity;
                if (twitterAccount.accessToken.Equals(MastodonAPIWrapper.sharedApiWrapper.selectedAccount.accessToken))
                {
                    accountAnimOpacity.To = 1.0;
                } else
                {
                    accountAnimOpacity.To = 0.5;
                }
                topMargin += 60;

                ThicknessAnimation homeButtonAnim = new ThicknessAnimation();
                Storyboard.SetTarget(homeButtonAnim, accountUI.homeBtn);
                Storyboard.SetTargetProperty(homeButtonAnim, new PropertyPath(UserControl.MarginProperty));
                homeButtonAnim.From = accountUI.homeBtn.Margin;
                homeButtonAnim.To = new Thickness(4, topMargin, 0, 0);

                DoubleAnimation homeButtonAnimOpacity = new DoubleAnimation();
                Storyboard.SetTarget(homeButtonAnimOpacity, accountUI.homeBtn);
                Storyboard.SetTargetProperty(homeButtonAnimOpacity, new PropertyPath(UserControl.OpacityProperty));
                homeButtonAnimOpacity.From = accountUI.homeBtn.Opacity;

                if (twitterAccount.accessToken.Equals(MastodonAPIWrapper.sharedApiWrapper.selectedAccount.accessToken))
                {
                    topMargin += 50;
                    homeButtonAnimOpacity.To = 1;
                }
                else
                {
                    homeButtonAnimOpacity.To = 0;
                }

                ThicknessAnimation mentionsBtnAnim = new ThicknessAnimation();
                Storyboard.SetTarget(mentionsBtnAnim, accountUI.mentionsBtn);
                Storyboard.SetTargetProperty(mentionsBtnAnim, new PropertyPath(UserControl.MarginProperty));
                mentionsBtnAnim.From = accountUI.mentionsBtn.Margin;
                mentionsBtnAnim.To = new Thickness(4, topMargin, 0, 0);

                DoubleAnimation mentionsBtnAnimOpacity = new DoubleAnimation();
                Storyboard.SetTarget(mentionsBtnAnimOpacity, accountUI.mentionsBtn);
                Storyboard.SetTargetProperty(mentionsBtnAnimOpacity, new PropertyPath(UserControl.OpacityProperty));
                mentionsBtnAnimOpacity.From = accountUI.mentionsBtn.Opacity;

                if (twitterAccount.accessToken.Equals(MastodonAPIWrapper.sharedApiWrapper.selectedAccount.accessToken))
                {
                    topMargin += 50;
                    mentionsBtnAnimOpacity.To = 1;
                }
                else
                {
                    mentionsBtnAnimOpacity.To = 0;
                }

                ThicknessAnimation directBtnAnim = new ThicknessAnimation();
                Storyboard.SetTarget(directBtnAnim, accountUI.directBtn);
                Storyboard.SetTargetProperty(directBtnAnim, new PropertyPath(UserControl.MarginProperty));
                directBtnAnim.From = accountUI.directBtn.Margin;
                directBtnAnim.To = new Thickness(4, topMargin, 0, 0);

                DoubleAnimation directBtnAnimOpacity = new DoubleAnimation();
                Storyboard.SetTarget(directBtnAnimOpacity, accountUI.directBtn);
                Storyboard.SetTargetProperty(directBtnAnimOpacity, new PropertyPath(UserControl.OpacityProperty));
                directBtnAnimOpacity.From = accountUI.directBtn.Opacity;

                if (twitterAccount.accessToken.Equals(MastodonAPIWrapper.sharedApiWrapper.selectedAccount.accessToken))
                {
                    topMargin += 50;
                    directBtnAnimOpacity.To = 1;
                }
                else
                {
                    directBtnAnimOpacity.To = 0;
                }

                ThicknessAnimation messagesBtnAnim = new ThicknessAnimation();
                Storyboard.SetTarget(messagesBtnAnim, accountUI.publicBtn);
                Storyboard.SetTargetProperty(messagesBtnAnim, new PropertyPath(UserControl.MarginProperty));
                messagesBtnAnim.From = accountUI.publicBtn.Margin;
                messagesBtnAnim.To = new Thickness(4, topMargin, 0, 0);

                DoubleAnimation messagesBtnAnimOpacity = new DoubleAnimation();
                Storyboard.SetTarget(messagesBtnAnimOpacity, accountUI.publicBtn);
                Storyboard.SetTargetProperty(messagesBtnAnimOpacity, new PropertyPath(UserControl.OpacityProperty));
                messagesBtnAnimOpacity.From = accountUI.publicBtn.Opacity;

                if (twitterAccount.accessToken.Equals(MastodonAPIWrapper.sharedApiWrapper.selectedAccount.accessToken))
                {
                    topMargin += 50;
                    messagesBtnAnimOpacity.To = 1;
                }
                else
                {
                    messagesBtnAnimOpacity.To = 0;
                }

                ThicknessAnimation userButtonAnim = new ThicknessAnimation();
                Storyboard.SetTarget(userButtonAnim, accountUI.userButton);
                Storyboard.SetTargetProperty(userButtonAnim, new PropertyPath(UserControl.MarginProperty));
                userButtonAnim.From = accountUI.userButton.Margin;
                userButtonAnim.To = new Thickness(4, topMargin, 0, 0);

                DoubleAnimation userButtonAnimOpacity = new DoubleAnimation();
                Storyboard.SetTarget(userButtonAnimOpacity, accountUI.userButton);
                Storyboard.SetTargetProperty(userButtonAnimOpacity, new PropertyPath(UserControl.OpacityProperty));
                userButtonAnimOpacity.From = accountUI.userButton.Opacity;

                if (twitterAccount.accessToken.Equals(MastodonAPIWrapper.sharedApiWrapper.selectedAccount.accessToken))
                {
                    topMargin += 50;
                    userButtonAnimOpacity.To = 1;
                }
                else
                {
                    userButtonAnimOpacity.To = 0;
                }

                ThicknessAnimation searchBtnAnim = new ThicknessAnimation();
                Storyboard.SetTarget(searchBtnAnim, accountUI.searchBtn);
                Storyboard.SetTargetProperty(searchBtnAnim, new PropertyPath(UserControl.MarginProperty));
                searchBtnAnim.From = accountUI.searchBtn.Margin;
                searchBtnAnim.To = new Thickness(4, topMargin, 0, 0);

                DoubleAnimation searchBtnAnimOpacity = new DoubleAnimation();
                Storyboard.SetTarget(searchBtnAnimOpacity, accountUI.searchBtn);
                Storyboard.SetTargetProperty(searchBtnAnimOpacity, new PropertyPath(UserControl.OpacityProperty));
                searchBtnAnimOpacity.From = accountUI.searchBtn.Opacity;

                if (twitterAccount.accessToken.Equals(MastodonAPIWrapper.sharedApiWrapper.selectedAccount.accessToken))
                {
                    topMargin += 50;
                    searchBtnAnimOpacity.To = 1;
                }
                else
                {
                    searchBtnAnimOpacity.To = 0;
                }

                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(accountAnim);
                storyboard.Children.Add(accountAnimOpacity);

                storyboard.Children.Add(homeButtonAnim);
                storyboard.Children.Add(homeButtonAnimOpacity);

                storyboard.Children.Add(mentionsBtnAnim);
                storyboard.Children.Add(mentionsBtnAnimOpacity);

                storyboard.Children.Add(messagesBtnAnim);
                storyboard.Children.Add(messagesBtnAnimOpacity);

                storyboard.Children.Add(directBtnAnim);
                storyboard.Children.Add(directBtnAnimOpacity);

                storyboard.Children.Add(userButtonAnim);
                storyboard.Children.Add(userButtonAnimOpacity);

                storyboard.Children.Add(searchBtnAnim);
                storyboard.Children.Add(searchBtnAnimOpacity);
                storyboard.SpeedRatio *= 5;
                storyboard.Begin();
            }
        }

        const int WM_DWMCOLORIZATIONCOLORCHANGED = 0x320;

        private IntPtr hwnd;
        private HwndSource hsource;

        private static Color GetWindowColorizationColor(bool opaque)
        {
            DWMCOLORIZATIONPARAMS dwmparams = new DWMCOLORIZATIONPARAMS();
            DwmGetColorizationParameters(ref dwmparams);

            return Color.FromArgb(
                (byte)(opaque ? 255 : 156 /*dwmparams.ColorizationColor >> 24*/), 
        (byte)(dwmparams.ColorizationColor >> 16), 
        (byte)(dwmparams.ColorizationColor >> 8), 
        (byte)dwmparams.ColorizationColor
            );
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_DWMCOLORIZATIONCOLORCHANGED:

                    /* 
                     * Update gradient brushes with new color information from
                     * NativeMethods.DwmGetColorizationParams() or the registry.
                     */
                    this.Background = new SolidColorBrush(GetWindowColorizationColor(false));
                    return IntPtr.Zero;

                default:
                    return IntPtr.Zero;
            }
        }

        private void OnContentRendered(object sender, EventArgs e)
        {
            try
            {
                // Obtain the window handle for WPF application
                IntPtr mainWindowPtr = new WindowInteropHelper(this).Handle;
                HwndSource mainWindowSrc = HwndSource.FromHwnd(mainWindowPtr);
                mainWindowSrc.CompositionTarget.BackgroundColor = Color.FromArgb(0, 0, 0, 0);

                // Get System Dpi
                System.Drawing.Graphics desktop = System.Drawing.Graphics.FromHwnd(mainWindowPtr);
                float DesktopDpiX = desktop.DpiX;
                float DesktopDpiY = desktop.DpiY;

                // Set Margins
                MARGINS margins = new MARGINS();

                margins.Left = margins.Top = margins.Right = margins.Bottom = -1;

                DwmExtendFrameIntoClientArea(mainWindowSrc.Handle, ref margins);

                const int dwmwaNcrenderingPolicy = 2;
                var dwmncrpDisabled = 2;

                DwmSetWindowAttribute(mainWindowSrc.Handle, dwmwaNcrenderingPolicy, ref dwmncrpDisabled, sizeof(int));

                const int dwmDisallowPeekPolicy = 12;
                var dwmPeekDisabled = 0;

                DwmSetWindowAttribute(mainWindowSrc.Handle, dwmDisallowPeekPolicy, ref dwmPeekDisabled, sizeof(int));
            }
            // If not Vista, paint background white. 
            catch (Exception)
            {
                //SetResourceReference(Control.BackgroundProperty, SystemColors.ControlBrushKey);
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if ((hwnd = new WindowInteropHelper(this).Handle) == IntPtr.Zero)
            {
                throw new InvalidOperationException("Could not get window handle.");
            }

            hsource = HwndSource.FromHwnd(hwnd);
            hsource.AddHook(WndProc);
            this.Background = new SolidColorBrush(GetWindowColorizationColor(false));
            /*try
            {
                var windowHelper = new WindowInteropHelper(this);
                var accent = new AccentPolicy();
                var accentStructSize = Marshal.SizeOf(accent);
                accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;

                var accentPtr = Marshal.AllocHGlobal(accentStructSize);
                Marshal.StructureToPtr(accent, accentPtr, false);

                var data = new WindowCompositionAttributeData();
                data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
                data.SizeOfData = accentStructSize;
                data.Data = accentPtr;

                SetWindowCompositionAttribute(windowHelper.Handle, ref data);

                Marshal.FreeHGlobal(accentPtr);

                this.Background = new SolidColorBrush(Colors.Transparent);
            }
            catch (Exception)
            {

            }*/

            this.OnContentRendered(sender, e);
            foreach (Account mastodonAccount in MastodonAPIWrapper.sharedApiWrapper.accounts) {
                Console.WriteLine("Start Streaming Thread for {0}...", mastodonAccount.accessToken);
                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += (sender2, e2) =>
                {
                    Console.WriteLine("Start Streaming for {0}...", mastodonAccount.accessToken);
                    /*OAuthUtils utils = new OAuthUtils();

                    MastodonAPIWrapper.sharedApiWrapper.getTimeline(twitterAccount, "mentions", "", "");

                    while (true)
                    {
                        Console.WriteLine("Updating...");
                        Console.WriteLine("Sleeping for 10 seconds...");

                        MastodonAPIWrapper.sharedApiWrapper.updateAccount(twitterAccount);

                        Thread.Sleep(10 * 1000);
                    }*/

                    MastodonAPIWrapper.sharedApiWrapper.startStreaming(mastodonAccount);
                    //utils.StreamGet("https://userstream.twitter.com/1.1/user.json", "stringify_friend_ids=true", TwitterAPIWrapper.sharedApiWrapper.handleStreamInput, twitterAccount);
                };
                worker.RunWorkerAsync();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown(0);
        }

        private void button_Enter(object sender, MouseEventArgs e)
        {
            Button btn = (Button)sender;
            TabImage img = (TabImage)btn.Content;
            img.Opacity = img.SelectedOpacity;
        }

        private void button_Leave(object sender, MouseEventArgs e)
        {
            Button btn = (Button)sender;
            TabImage img = (TabImage)btn.Content;
            if (!tabController.isButtonSelected(btn))
            {
                img.Opacity = img.DefaultOpacity;
            }
        }

        private void tootEvent(object sender, EventArgs e)
        {
            TootWindow toot = new TootWindow(MastodonAPIWrapper.sharedApiWrapper.selectedAccount);
            //toot.account1.Source = account1.Source;
            toot.Show();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.BrowserBack)
            {
                int idx = tabController.getSelectedControl();
                NavController controller = tabController.getControl(idx);
                controller.popControl();
            }
        }

        private void mainGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
                    this.DragMove();
            }
            catch (Exception e2)
            {

            }
        }

        private void WindowResize(object sender, MouseButtonEventArgs e) //PreviewMousLeftButtonDown
        {
            HwndSource hwndSource = PresentationSource.FromVisual((Visual)sender) as HwndSource;
            SendMessage(hwndSource.Handle, 0x112, (IntPtr)61448, IntPtr.Zero);
        }

        /*protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }*/

        // minimize to system tray when applicaiton is closed
        protected override void OnClosing(CancelEventArgs e)
        {
            // setting cancel to true will cancel the close request
            // so the application is not closed
            if (Settings.Default.minimizeToTray)
            {
                e.Cancel = true;

                this.Hide();

                Application.Current.MainWindow.WindowState = WindowState.Minimized;
            }

            base.OnClosing(e);
        }

        private void OnActivated(object sender, EventArgs e)
        {
            if (WindowStyle != WindowStyle.None)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (DispatcherOperationCallback)delegate(object unused)
                {
                    WindowStyle = WindowStyle.None;
                    return null;
                }
                , null);
            }
        }

        public void minimize()
        {
            this.WindowStyle = WindowStyle.SingleBorderWindow;
            this.WindowState = WindowState.Minimized;
        }

        public void animateClose()
        {
            this.WindowStyle = WindowStyle.SingleBorderWindow;
            this.Close();
        }

        private void closeBtn_Click(object sender, RoutedEventArgs e)
        {
           this.animateClose();
        }

        private void closeBtn_MouseDown(object sender, MouseEventArgs e)
        {
            if (MastodonAPIWrapper.sharedApiWrapper.nightModeEnabled)
                closeBtnImage.Source = new BitmapImage(new Uri("Resources/close-dark.png", UriKind.Relative));
            else
                closeBtnImage.Source = new BitmapImage(new Uri("Resources/close-dark-highlighted.png", UriKind.Relative));
        }

        private void closeBtn_MouseUp(object sender, MouseEventArgs e)
        {
            if (MastodonAPIWrapper.sharedApiWrapper.nightModeEnabled)
                closeBtnImage.Source = new BitmapImage(new Uri("Resources/close-dark-highlighted.png", UriKind.Relative));
            else
                closeBtnImage.Source = new BitmapImage(new Uri("Resources/close-dark.png", UriKind.Relative));
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                MainWindow.sharedMainWindow.DragMove();
        }

        private void minimizeBtn_MouseDown(object sender, MouseEventArgs e)
        {
            if (MastodonAPIWrapper.sharedApiWrapper.nightModeEnabled)
                minimizeBtnImage.Source = new BitmapImage(new Uri("Resources/minimize-dark.png", UriKind.Relative));
            else
                minimizeBtnImage.Source = new BitmapImage(new Uri("Resources/minimize-light.png", UriKind.Relative));
        }

        private void minimizeBtn_MouseUp(object sender, MouseEventArgs e)
        {
            if (MastodonAPIWrapper.sharedApiWrapper.nightModeEnabled)
                minimizeBtnImage.Source = new BitmapImage(new Uri("Resources/minimize-dark-highlighted.png", UriKind.Relative));
            else
                minimizeBtnImage.Source = new BitmapImage(new Uri("Resources/minimize-light-highlighted.png", UriKind.Relative));
        }


        private void minimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            this.minimize();
        }

        private void toot_Click(object sender, RoutedEventArgs e)
        {
            TootWindow tootWindow = new TootWindow(MastodonAPIWrapper.sharedApiWrapper.selectedAccount);
            tootWindow.Show();
        }

        private void settings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();
        }
    }
}
