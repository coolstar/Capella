using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TaskDialogInterop;

namespace Capella
{
    /// <summary>
    /// Interaction logic for ProfilesList.xaml
    /// </summary>
    /// 
    public partial class ProfilesList : UserControl
    {
        public NavController navController;
        public dynamic list = null;
        public List<Profile> profiles = new List<Profile>();
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

        public void convertList()
        {
            foreach (dynamic rawUser in list.Children())
            {
                Profile profileItem = new Profile();
                profileItem.name = (String)rawUser["display_name"];
                profileItem.handle = "@" + (String)rawUser["acct"];
                profileItem.profilePicUri = (Uri)rawUser["avatar"];
                profileItem.accountID = rawUser["id"];
                profiles.Add(profileItem);

                
                /*listItem.MouseLeftButtonUp += (sender, e) =>
                {
                    ProfilePanel panel = new ProfilePanel();
                    panel.profileScreenName = rawUser.screen_name;
                    panel.refreshProfile();
                    navController.pushControl(panel);
                };
                listItem.Width = profilesList.Width;*/
            }
        }

        public void renderList()
        {
            try
            {
                if (list == null)
                {
                    dynamic error = list.errors[0];

                    TaskDialogOptions config = new TaskDialogOptions();
                    config.Owner = MainWindow.sharedMainWindow;
                    config.Title = "Error Loading Timeline";
                    config.MainInstruction = "Please try again at a later time.";
                    config.Content = "The Twitter API returned \"" + "Error " + error.code + ": " + error.message + "\".";
                    config.ExpandedInfo = "You may try logging out and back in to twitter and see if that fixes it. If not, please wait at least 5 minutes for Twitter's (horrible) API.";
                    config.MainIcon = VistaTaskDialogIcon.Error;
                    config.ExpandToFooter = false;
                    TaskDialog.Show(config);
                    return;
                }
                profilesList.ItemsSource = profiles;
            }
            catch (Exception)
            {
            }
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
            ProfilePanel panel = new ProfilePanel();
            panel.profileScreenName = profile.handle;
            panel.profileUserID = profile.accountID;
            panel.twitterAccountToken = twitterAccountToken;
            panel.refreshProfile();
            navController.pushControl(panel);
        }
    }
}
