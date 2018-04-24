using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Text.RegularExpressions;

namespace Capella
{
    /// <summary>
    /// Interaction logic for ProfilePanel.xaml
    /// </summary>
    public partial class ProfilePanel : UserControl
    {
        public NavController navController;
        public String profileScreenName;
        public String profileUserID = null;
        public String twitterAccountToken;
        int actionBtnType = 0;

        private bool isLoaded = false;

        public ProfilePanel()
        {
            InitializeComponent();
            if (MastodonAPIWrapper.sharedApiWrapper.nightModeEnabled){
                backgroundColorGrid.Background = new SolidColorBrush(Color.FromArgb(255, 24, 24, 24));
                this.Background = new SolidColorBrush(Color.FromArgb(255, 26, 26, 26));
            }
        }

        public ProfilePanel createCopy()
        {
            ProfilePanel panelCopy = new ProfilePanel();
            panelCopy.twitterAccountToken = this.twitterAccountToken;
            panelCopy.profileScreenName = this.profileScreenName;
            panelCopy.profileUserID = this.profileUserID;
            return panelCopy;
        }

        public void refreshProfile()
        {
            //isLoaded = true;

            dynamic profile = null;
            dynamic relationship = null;

            bool isFollowedBy = false;

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (sender, e) =>
                {
                    Account twitterAccount = MastodonAPIWrapper.sharedApiWrapper.accountWithToken(twitterAccountToken);

                    if (this.profileUserID == null || this.profileUserID == "")
                        this.profileUserID = twitterAccount.accountID;
                    profile = MastodonAPIWrapper.sharedApiWrapper.getProfile(this.profileUserID, twitterAccount);
                    relationship = MastodonAPIWrapper.sharedApiWrapper.getRelationship(this.profileUserID, twitterAccount);

                    if (relationship != null)
                        relationship = relationship[0];

                    isFollowedBy = (bool)relationship["followed_by"];
                };
            worker.RunWorkerCompleted += (sender, e) =>
                {
                    this.renderProfile(profile, relationship);
                    if (isFollowedBy)
                        follows.Visibility = Visibility.Visible;
                };
            worker.RunWorkerAsync();
        }

        private void renderProfile(dynamic profile, dynamic relationship)
        {
            Account twitterAccount = MastodonAPIWrapper.sharedApiWrapper.accountWithToken(twitterAccountToken);

            if (profile == null)
            {

                MessageBox.Show("Error ??: Unknown Error.", "Error loading Profile", MessageBoxButton.OK);
                return;
            }
            if (profile["errors"] != null)
            {
                MessageBox.Show("Error "+profile["errors"][0]["code"]+": "+profile["errors"][0]["message"]+".", "Error loading Profile", MessageBoxButton.OK);
                return;
            }
            backgroundImg.Source = new BitmapImage((Uri)profile["header"]);

            profileUserID = (String)profile["id"];

            try
            {
                accountImg.Source = new BitmapImage(new Uri((String)profile["avatar"], UriKind.Absolute));
            }
            catch (Exception e)
            {

            }
            nameLabel.Content = profile["display_name"];
            handleLabel.Content = "@" + profile["acct"];

            if ((bool)profile["locked"] == true)
            {
                tootsProtected.Visibility = Visibility.Visible;
            }

            /*if ((bool)profile["verified"] == true)
            {
                verified.Visibility = Visibility.Visible;
            }*/

            int toots_count = (int)profile["statuses_count"];
            String tootsCountStr = String.Format("{0:n0}", toots_count);
            if (toots_count > 9999999)
            {
                tootsCountStr = String.Format("{0:n1} M", (double)toots_count / 1000000.0);
            }
            else if (toots_count > 9999)
            {
                tootsCountStr = String.Format("{0:n1} K", (double)toots_count / 1000.0);
            }
            tootsCount.Content = tootsCountStr;

            int following_count = (int)profile["following_count"];
            String followingCountStr = String.Format("{0:n0}", following_count);
            if (following_count > 999999)
            {
                followingCountStr = String.Format("{0:n1} M", (double)following_count / 1000000.0);
            }
            else if (following_count > 9999)
            {
                followingCountStr = String.Format("{0:n1} K", (double)following_count / 1000.0);
            }
            followingCount.Content = followingCountStr;

            int followers_count = (int)profile["followers_count"];
            String followersCountStr = String.Format("{0:n0}", followers_count);
            if (followers_count > 999999)
            {
                followersCountStr = String.Format("{0:n1} M", (double)followers_count / 1000000.0);
            }
            else if (followers_count > 9999)
            {
                followersCountStr = String.Format("{0:n1} K", (double)followers_count / 1000.0);
            }
            followersCount.Content = followersCountStr;

            if (profileUserID.Equals(twitterAccount.accountID))
            {
                profileActionBtn.Content = "Edit";
                actionBtnType = 0;
            } else
            {
                bool isFollowing = (bool)relationship["following"];
                bool requested = (bool)relationship["requested"];
                if (isFollowing)
                {
                    profileActionBtn.Content = "Unfollow";
                    actionBtnType = 1;
                } else
                {
                    if (requested)
                    {
                        profileActionBtn.Content = "Follow Requested";
                    }
                    else
                    {
                        if ((bool)profile["locked"] == true)
                        {
                            profileActionBtn.Content = "Request Follow";
                        }
                        else
                        {
                            profileActionBtn.Content = "Follow";
                        }
                    }
                    
                    actionBtnType = 2;
                }
            }

            TimelinePanel tootsPanel = new TimelinePanel();
            tootsPanel.twitterAccountToken = twitterAccountToken;
            tootsPanel.navController = navController;
            tootsPanel.timelineType = "user";
            tootsPanel.profileID = ""+(String)profile.id;
            tootsPanel.Height = contents.Height;
            tootsPanel.hideTopBar();
            tootsPanel.refreshTimeline();
            contents.Children.Add(tootsPanel);

            profileBio.Text = (String)profile.note;
            profileBio.Text = Regex.Replace(profileBio.Text, "<.*?>", String.Empty);
            if (profile["location"] != null && (string)profile["location"] != "")
            {
                location.Content = (String)profile["location"];
            }

            if (profile["url"] != null && (string)profile["url"] != "")
            {
                //website.Content = (String)profile["entities"]["url"]["urls"][0]["display_url"];
            }
        }

        private void backBtn_Click(object sender, RoutedEventArgs e)
        {
            navController.popControl();
        }

        private System.Drawing.Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new System.Drawing.Bitmap(bitmap);
            }
        }

        private System.Drawing.Color CalculateAverageColor(System.Drawing.Bitmap bm)
        {
            int width = bm.Width;
            int height = bm.Height;
            int red = 0;
            int green = 0;
            int blue = 0;
            int minDiversion = 15; // drop pixels that do not differ by at least minDiversion between color values (white, gray or black)
            int dropped = 0; // keep track of dropped pixels
            long[] totals = new long[] { 0, 0, 0 };
            int bppModifier = bm.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb ? 3 : 4; // cutting corners, will fail on anything else but 32 and 24 bit images

            System.Drawing.Imaging.BitmapData srcData = bm.LockBits(new System.Drawing.Rectangle(0, 0, bm.Width, bm.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bm.PixelFormat);
            int stride = srcData.Stride;
            IntPtr Scan0 = srcData.Scan0;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int idx = (y * stride) + x * bppModifier;
                        red = p[idx + 2];
                        green = p[idx + 1];
                        blue = p[idx];
                        if (Math.Abs(red - green) > minDiversion || Math.Abs(red - blue) > minDiversion || Math.Abs(green - blue) > minDiversion)
                        {
                            totals[2] += red;
                            totals[1] += green;
                            totals[0] += blue;
                        }
                        else
                        {
                            dropped++;
                        }
                    }
                }
            }

            int count = width * height - dropped;
            if (count == 0)
                return System.Drawing.Color.Black;
            byte avgR = (byte)(totals[2] / count);
            byte avgG = (byte)(totals[1] / count);
            byte avgB = (byte)(totals[0] / count);

            return System.Drawing.Color.FromArgb(255, avgR, avgG, avgB);
        }

        private void showMenu(object sender, RoutedEventArgs e)
        {
            Account twitterAccount = MastodonAPIWrapper.sharedApiWrapper.accountWithToken(twitterAccountToken);

            ContextMenu menu = new ContextMenu();

            MenuItem blockItem = new MenuItem();
            if (twitterAccount.blockedIDs.Contains(profileUserID))
                blockItem.Header = "Unblock " + handleLabel.Content;
            else
                blockItem.Header = "Block " + handleLabel.Content;
            blockItem.Click += new RoutedEventHandler(block_Click);
            menu.Items.Add(blockItem);

            menu.Items.Add(new Separator());

            MenuItem muteItem = new MenuItem();
            muteItem.Header = "Mute";
            //muteItem.Click += new RoutedEventHandler(mute_Click);
            menu.Items.Add(muteItem);

            MenuItem followItem = new MenuItem();
            followItem.Header = "Follow";
            //followItem.Click += new RoutedEventHandler(follow_Click);
            menu.Items.Add(followItem);

            menu.Margin = new Thickness(0);
            menu.PlacementTarget = (UIElement)sender;
            menu.IsOpen = true;
        }

        private void favstar_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://favstar.fm/users/" + MastodonAPIWrapper.sharedApiWrapper.sharedOAuthUtils.UrlEncode(this.profileScreenName));
        }

        private void block_Click(object sender, RoutedEventArgs e)
        {
            Account twitterAccount = MastodonAPIWrapper.sharedApiWrapper.accountWithToken(twitterAccountToken);

            BackgroundWorker worker = new BackgroundWorker();
            bool success = false;
            bool unblocking = twitterAccount.blockedIDs.Contains(profileUserID);
            worker.DoWork += (sender2, e2) =>
                {
                    if (twitterAccount.blockedIDs.Contains(profileUserID))
                        success = (MastodonAPIWrapper.sharedApiWrapper.blockUser(profileUserID, true, twitterAccount) == false);
                    else
                        success = MastodonAPIWrapper.sharedApiWrapper.blockUser(profileUserID, false, twitterAccount);
                };
            worker.RunWorkerCompleted += (sender2, e2) =>
            {
                if (success == true)
                {
                    /*notification.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7F0074FF"));
                    if (unblocking)
                        notificationText.Content = "You have unblocked @" + profileScreenName + "!";
                    else
                        notificationText.Content = "You have blocked @" + profileScreenName + "!";
                    this.showNotification();*/
                }
            };
            worker.RunWorkerAsync();
        }

        private void showNotification()
        {
            /*notification.Margin = new Thickness(0, -40, 0, 0);

            ThicknessAnimation anim = new ThicknessAnimation();
            anim.From = new Thickness(0, -40, 0, 0);
            anim.To = new Thickness(0);
            Storyboard.SetTarget(anim, notification);
            Storyboard.SetTargetProperty(anim, new PropertyPath(UserControl.MarginProperty));

            Storyboard storyboard = new Storyboard();
            storyboard.SpeedRatio *= 2;
            storyboard.Children.Add(anim);
            storyboard.Begin();*/
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                MainWindow.sharedMainWindow.DragMove();
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

        private void page1_Click(object sender, RoutedEventArgs e)
        {
            ScrollViewerOffsetMediator mediator = new ScrollViewerOffsetMediator();
            mediator.ScrollViewer = profileScroller;

            DoubleAnimation scrollAnimation = new DoubleAnimation();
            scrollAnimation.From = profileScroller.HorizontalOffset;
            scrollAnimation.To = 0.0;

            Storyboard.SetTarget(scrollAnimation, mediator);
            Storyboard.SetTargetProperty(scrollAnimation, new PropertyPath(ScrollViewerOffsetMediator.HorizontalOffsetProperty));

            DoubleAnimation page1Animation = new DoubleAnimation();
            page1Animation.From = page1.Opacity;
            page1Animation.To = 0.8;

            Storyboard.SetTarget(page1Animation, page1);
            Storyboard.SetTargetProperty(page1Animation, new PropertyPath(Button.OpacityProperty));

            DoubleAnimation page2Animation = new DoubleAnimation();
            page2Animation.From = page2.Opacity;
            page2Animation.To = 0.4;

            Storyboard.SetTarget(page2Animation, page2);
            Storyboard.SetTargetProperty(page2Animation, new PropertyPath(Button.OpacityProperty));

            DoubleAnimation darkenAnimation = new DoubleAnimation();
            darkenAnimation.From = profileBackgroundDarken.Opacity;
            darkenAnimation.To = 0.25;

            Storyboard.SetTarget(darkenAnimation, profileBackgroundDarken);
            Storyboard.SetTargetProperty(darkenAnimation, new PropertyPath(Border.OpacityProperty));

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(scrollAnimation);
            storyboard.Children.Add(page1Animation);
            storyboard.Children.Add(page2Animation);
            storyboard.Children.Add(darkenAnimation);
            storyboard.SpeedRatio *= 3.5;
            storyboard.Begin();
        }

        private void page2_Click(object sender, RoutedEventArgs e)
        {
            ScrollViewerOffsetMediator mediator = new ScrollViewerOffsetMediator();
            mediator.ScrollViewer = profileScroller;

            DoubleAnimation scrollAnimation = new DoubleAnimation();
            scrollAnimation.From = profileScroller.HorizontalOffset;
            scrollAnimation.To = profileScroller.ViewportWidth;

            Storyboard.SetTarget(scrollAnimation, mediator);
            Storyboard.SetTargetProperty(scrollAnimation, new PropertyPath(ScrollViewerOffsetMediator.HorizontalOffsetProperty));

            DoubleAnimation page1Animation = new DoubleAnimation();
            page1Animation.From = page1.Opacity;
            page1Animation.To = 0.4;

            Storyboard.SetTarget(page1Animation, page1);
            Storyboard.SetTargetProperty(page1Animation, new PropertyPath(Button.OpacityProperty));

            DoubleAnimation page2Animation = new DoubleAnimation();
            page2Animation.From = page2.Opacity;
            page2Animation.To = 0.8;

            Storyboard.SetTarget(page2Animation, page2);
            Storyboard.SetTargetProperty(page2Animation, new PropertyPath(Button.OpacityProperty));

            DoubleAnimation darkenAnimation = new DoubleAnimation();
            darkenAnimation.From = profileBackgroundDarken.Opacity;
            darkenAnimation.To = 0.5;

            Storyboard.SetTarget(darkenAnimation, profileBackgroundDarken);
            Storyboard.SetTargetProperty(darkenAnimation, new PropertyPath(Border.OpacityProperty));

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(scrollAnimation);
            storyboard.Children.Add(page1Animation);
            storyboard.Children.Add(page2Animation);
            storyboard.Children.Add(darkenAnimation);
            storyboard.SpeedRatio *= 3.5;
            storyboard.Begin();
        }

        public void WillDisplay()
        {
            this.refreshProfile();
        }

        private void profileActionBtn_Click(object sender, RoutedEventArgs e)
        {
            if (actionBtnType == 0)
            {
                MessageBox.Show("Editing Profiles hasn't been implemented Yet. Sorry :(");
            } else if (actionBtnType == 1 || actionBtnType == 2)
            {
                Account twitterAccount = MastodonAPIWrapper.sharedApiWrapper.accountWithToken(twitterAccountToken);
                dynamic profile = null;
                dynamic relationship = null;
                bool isFollowing = false;
                BackgroundWorker followAction = new BackgroundWorker();
                profileActionBtn.IsEnabled = false;
                followAction.DoWork += (sender2, e2) =>
                {
                    MastodonAPIWrapper.sharedApiWrapper.followAccount(profileUserID, twitterAccount, actionBtnType == 1 ? false : true);

                    if (this.profileUserID == null || this.profileUserID == "")
                        this.profileUserID = twitterAccount.accountID;
                    profile = MastodonAPIWrapper.sharedApiWrapper.getProfile(this.profileUserID, twitterAccount);
                    relationship = MastodonAPIWrapper.sharedApiWrapper.getRelationship(this.profileUserID, twitterAccount);

                    if (relationship != null)
                        relationship = relationship[0];

                    isFollowing = MastodonAPIWrapper.sharedApiWrapper.followAccount(profileUserID, twitterAccount, actionBtnType == 1 ? false : true);
                    if (isFollowing)
                        actionBtnType = 1;
                    else
                        actionBtnType = 2;
                    Thread.Sleep(300);
                };
                followAction.RunWorkerCompleted += (sender2, e2) =>
                {
                    bool requested = (bool)relationship["requested"];
                    if (actionBtnType == 1)
                    {
                        // these may be reversed
                        if ((bool)profile["locked"] == true)
                        {
                            profileActionBtn.Content = "Request Follow";
                        }
                        else
                        {
                            profileActionBtn.Content = "Unfollow";
                        }
                    }
                    else if (actionBtnType == 2)
                    {
                        if (requested)
                        {
                            profileActionBtn.Content = "Follow Requested";
                        }
                        else
                        {
                            if ((bool)profile["locked"] == true)
                            {
                                profileActionBtn.Content = "Request Follow";
                            }
                            else
                            {
                                profileActionBtn.Content = "Follow";
                            }
                        }
                    }
                    profileActionBtn.IsEnabled = true;

                };
                followAction.RunWorkerAsync();
            }
        }

        private void Followers_Click(object sender, MouseButtonEventArgs e)
        {
            ProfilesList profilesList = new ProfilesList();
            profilesList.twitterAccountToken = twitterAccountToken;
            profilesList.setTitle("Followers");
            navController.pushControl(profilesList);

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (sender2, e2) =>
            {
                Account twitterAccount = MastodonAPIWrapper.sharedApiWrapper.accountWithToken(twitterAccountToken);
                dynamic followers = MastodonAPIWrapper.sharedApiWrapper.followersList(twitterAccount, profileUserID, 200);
                profilesList.list = followers;
                profilesList.convertList();
            };
            worker.RunWorkerCompleted += (sender2, e2) =>
            {
                profilesList.renderList();
            };
            worker.RunWorkerAsync();
        }

        private void Following_Click(object sender, MouseButtonEventArgs e)
        {
            ProfilesList profilesList = new ProfilesList();
            profilesList.twitterAccountToken = twitterAccountToken;
            profilesList.setTitle("Following");
            navController.pushControl(profilesList);

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (sender2, e2) =>
            {
                Account twitterAccount = MastodonAPIWrapper.sharedApiWrapper.accountWithToken(twitterAccountToken);
                dynamic followers = MastodonAPIWrapper.sharedApiWrapper.followingList(twitterAccount, profileUserID, 200);
                profilesList.list = followers;
                profilesList.convertList();
            };
            worker.RunWorkerCompleted += (sender2, e2) =>
            {
                profilesList.renderList();
            };
            worker.RunWorkerAsync();
        }
    }
}
