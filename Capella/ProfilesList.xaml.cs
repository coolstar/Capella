using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using TaskDialogInterop;
using Capella.Models;
using Newtonsoft.Json;

namespace Capella
{
    /// <summary>
    /// Interaction logic for ProfilesList.xaml
    /// </summary>
    /// 
    public partial class ProfilesList : UserControl
    {
        public NavController navController;
        public Profile[] profiles;
        public String twitterAccountToken;
        public ProfilesList()
        {
            InitializeComponent();
            try
            {
                VirtualizingStackPanel.SetScrollUnit(profilesList, ScrollUnit.Pixel);
                VirtualizingStackPanel.SetIsVirtualizing(profilesList, true);
                VirtualizingStackPanel.SetVirtualizationMode(profilesList, VirtualizationMode.Recycling);
                ScrollViewer.SetCanContentScroll(profilesList, true);
            }
            catch (Exception)
            {
            }

            if (MastodonAPIWrapper.sharedApiWrapper.nightModeEnabled)
            {
                profilesList.Background = new SolidColorBrush(Color.FromArgb(255, 50, 50, 50));
            }
        }

        public void setTitle(String newTitle)
        {
            int length = newTitle.Length;
            int fontSize = 18;
            if (length >= 14)
                fontSize = 16;
            if (length > 15)
                fontSize = 16;
            if (length > 18)
                fontSize = 14;
            if (length > 23)
                fontSize = 12;

            title.Text = newTitle;
            title.FontSize = fontSize;
        }

        public void renderList()
        {
            // Katelyn: This code never worked, even before I deleted `list`.
            //try
            //{
            //    if (list == null)
            //    {
            //        dynamic error = list.errors[0]; // Like seriously, this would have thrown a NullReferenceException 100% of the time.

            //        TaskDialogOptions config = new TaskDialogOptions();
            //        config.Owner = MainWindow.sharedMainWindow;
            //        config.Title = "Error Loading Timeline";
            //        config.MainInstruction = "Please try again at a later time.";
            //        config.Content = "The Twitter API returned \"" + "Error " + error.code + ": " + error.message + "\".";
            //        config.ExpandedInfo = "You may try logging out and back in to twitter and see if that fixes it. If not, please wait at least 5 minutes for Twitter's (horrible) API.";
            //        config.MainIcon = VistaTaskDialogIcon.Error;
            //        config.ExpandToFooter = false;
            //        TaskDialog.Show(config);
            //        return;
            //    }
            //    profilesList.ItemsSource = profiles;
            //}
            //catch (Exception)
            //{
            //}
        }

        private void backBtn_Click(object sender, RoutedEventArgs e)
        {
            navController.popControl();
        }

        private void backBtn_MouseDown(object sender, MouseEventArgs e)
        {

            DoubleAnimation opacityAnimation = new DoubleAnimation();
            opacityAnimation.From = 1.0;
            opacityAnimation.To = 0.0;

            Storyboard.SetTarget(opacityAnimation, backBtnImageLight);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(Image.OpacityProperty));

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(opacityAnimation);
            storyboard.SpeedRatio *= 3.5;
            storyboard.Begin();
        }

        private void backBtn_MouseUp(object sender, MouseEventArgs e)
        {
            DoubleAnimation opacityAnimation = new DoubleAnimation();
            opacityAnimation.From = 0.0;
            opacityAnimation.To = 1.0;

            Storyboard.SetTarget(opacityAnimation, backBtnImageLight);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(Image.OpacityProperty));

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(opacityAnimation);
            storyboard.SpeedRatio *= 3.5;
            storyboard.Begin();
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            Grid ctrl = (Grid)sender;
            Profile profile = (Profile)ctrl.DataContext;
            ProfilePanel panel = new ProfilePanel
            {
                profileScreenName = profile.Handle,
                profileUserID = profile.accountID,
                twitterAccountToken = twitterAccountToken
            };
            panel.refreshProfile();
            navController.pushControl(panel);
        }
    }
}
