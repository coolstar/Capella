using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Timers;
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
using System.Web;
using TaskDialogInterop;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace Capella
{
    /// <summary>
    /// Interaction logic for TimelinePanel.xaml
    /// </summary>
    ///
    public partial class TimelinePanel : UserControl
    {
        public String twitterAccountToken;

        public String timelineType = "home";
        public String profileID = "";
        public NavController navController;
        private bool subscribedToStream = false;
        private List<Toot> timeline;

        public bool isConversation = false;
        public String conversationStartToot;

        public bool isSearch = false;
        public String searchQuery = "";

        private bool isLoaded = false;

        public TimelinePanel()
        {
            InitializeComponent();
            /*Timer timer = new Timer();
            timer.Elapsed += new ElapsedEventHandler(refresh_Timeline);
            timer.Interval = 60000;
            timer.Enabled = true;*/

            if (MastodonAPIWrapper.sharedApiWrapper.nightModeEnabled)
            {
                this.Background = new SolidColorBrush(Color.FromArgb(255, 26, 26, 26));
                title.Foreground = Brushes.White;
                timelineBox.Background = new SolidColorBrush(Color.FromArgb(255, 50, 50, 50));
            }

            try
            {
                VirtualizingStackPanel.SetScrollUnit(timelineBox, ScrollUnit.Pixel);
                VirtualizingStackPanel.SetIsVirtualizing(timelineBox, true);
                VirtualizingStackPanel.SetVirtualizationMode(timelineBox, VirtualizationMode.Recycling);
                ScrollViewer.SetCanContentScroll(timelineBox, true);
            }
            catch (Exception)
            {
            }
        }

        public TimelinePanel createCopy()
        {
            TimelinePanel panelCopy = new TimelinePanel();
            panelCopy.twitterAccountToken = this.twitterAccountToken;
            panelCopy.timelineType = this.timelineType;
            panelCopy.profileID = this.profileID;
            panelCopy.isConversation = this.isConversation;
            panelCopy.conversationStartToot = this.conversationStartToot;
            panelCopy.isSearch = this.isSearch;
            panelCopy.searchQuery = this.searchQuery;
            panelCopy.setTitle(this.title.Text);
            return panelCopy;
        }

        public void hideTopBar()
        {
            timelineBox.Margin = new Thickness(0, 0, 0, 0);
            reloadBtn.Visibility = Visibility.Hidden;
            title.Visibility = Visibility.Hidden;
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

        public void refresh_Timeline(object sender, ElapsedEventArgs e)
        {
            this.refreshTimeline();
        }

        public void refreshTimeline()
        {
            isLoaded = true;

            var worker = new BackgroundWorker();
            dynamic rawTimeline = null;
            worker.DoWork += (sender, e) =>
                {
                    Account twitterAccount = MastodonAPIWrapper.sharedApiWrapper.accountWithToken(twitterAccountToken);

                    if (isConversation)
                        Console.WriteLine("Conversation Timeline...");
                    else if (isSearch)
                        Console.WriteLine("Search Timeline...");
                    else
                        Console.WriteLine("Loading Timeline...");

                    var watch = Stopwatch.StartNew();
                    if (isConversation)
                        rawTimeline = MastodonAPIWrapper.sharedApiWrapper.getConversation(twitterAccount, this.conversationStartToot);
                    /*else if (isSearch)
                    {
                        rawTimeline = TwitterAPIWrapper.sharedApiWrapper.searchToots(twitterAccount, this.searchQuery);
                        rawTimeline = rawTimeline["statuses"];
                    }*/
                    else
                        rawTimeline = MastodonAPIWrapper.sharedApiWrapper.getTimeline(twitterAccount, this.timelineType, this.profileID, null);
                    watch.Stop();
                    if (isConversation)
                        Console.WriteLine("Conversation took " + watch.ElapsedMilliseconds + "ms to load.");
                    else if (isSearch)
                        Console.WriteLine("Search took " + watch.ElapsedMilliseconds + "ms to load.");
                    else
                        Console.WriteLine("Timeline type " + timelineType + " took " + watch.ElapsedMilliseconds + "ms to load.");
                };
            worker.RunWorkerCompleted += (sender, e) =>
                {
                    Account twitterAccount = MastodonAPIWrapper.sharedApiWrapper.accountWithToken(twitterAccountToken);

                    Console.WriteLine("Timeline type loaded: " + timelineType);
                    if (timelineType != "home" && timelineType != "mentions")
                        this.renderTimeline(rawTimeline, twitterAccount);
                };
            if (!subscribedToStream && timelineType == "public")
            {
                MastodonAPIWrapper.sharedApiWrapper.publicTimelineChanged += this.renderTimelineFromStream;
                subscribedToStream = true;
            }
            if (!subscribedToStream && timelineType == "home")
            {
                MastodonAPIWrapper.sharedApiWrapper.homeTimelineChanged += this.renderTimelineFromStream;
                subscribedToStream = true;
            }
            if (!subscribedToStream && timelineType == "mentions")
            {
                MastodonAPIWrapper.sharedApiWrapper.mentionsTimelineChanged += this.renderTimelineFromStream;
                subscribedToStream = true;
            }
            if (subscribedToStream || isConversation || isSearch)
            {
                reloadBtn.Visibility = Visibility.Hidden;
            }
            worker.RunWorkerAsync();
        }

        private Toot renderToot(dynamic rawToot, int index, bool quoted)
        {
            Account twitterAccount = MastodonAPIWrapper.sharedApiWrapper.accountWithToken(twitterAccountToken);

            Toot toot = new Toot();
            toot.twitterAccountToken = twitterAccountToken;

            dynamic rawOrigToot = rawToot;
            dynamic rawUser = rawToot["account"];
            if (rawToot["reblog"] != null)
            {
                toot.isRetootedStatus = true;
                toot.origuser_name = rawUser["display_name"];
                toot.origuser_screen_name = rawUser["acct"];
                rawOrigToot = rawToot["reblog"];
                rawUser = rawOrigToot["account"];
            }
            if (""+rawOrigToot["id"] == conversationStartToot && quoted == false)
                toot.isStartToot = true;
            toot.userID = "" + rawUser["id"];
            toot.tootID = (String)rawOrigToot["id"];
            toot.user_screen_name = (String)rawUser["acct"];
            toot.tootURL = (String)rawOrigToot["url"];
            //toot.clientString = (String)rawOrigToot["source"];
            if (rawOrigToot["application"] != null && rawOrigToot["application"].Type == JTokenType.Object)
            {
                toot.clientString = rawOrigToot["application"]["name"];
                toot.clientLink = rawOrigToot["application"]["website"];
            } else
            {
                toot.clientString = "web";
                toot.clientLink = "https://mastodon.social";
            }

            if (twitterAccount.accountID.Equals(toot.userID))
                twitterAccount.myHandle = toot.user_screen_name;

            toot.user_name = (String)rawUser["display_name"];
            try
            {
                toot.user_profilepicurl = new Uri((String)rawUser["avatar"], UriKind.Absolute);
            } catch (Exception e)
            {
                //no profile pic ;-;
            }
            toot.rawText = (String)rawOrigToot["content"];
            toot.rawText = toot.rawText.Replace("<br>", "\n");
            toot.rawText = toot.rawText.Replace("<br/>", "\n");
            toot.rawText = toot.rawText.Replace("<br />", "\n");
            toot.rawText = toot.rawText.Replace("</p><p>", "\n\n");
            toot.rawText = Regex.Replace(toot.rawText, "<.*?>", String.Empty);

            foreach (String keyword in MastodonAPIWrapper.sharedApiWrapper.keywords)
            {
                if (toot.rawText.ToLower().Contains(keyword.ToLower()))
                {
                    return toot;
                }
            }
            //toot.rawEntities = rawOrigToot["entities"];
            //toot.rawExtendedEntities = rawOrigToot["extended_entities"];

            toot.isRetooted = false;
            toot.isFavorited = false;
            toot.numRetoots = 0;
            toot.numFavorites = 0;

            if (rawOrigToot["reblogged"] != null && rawOrigToot["reblogged"].Type == JTokenType.Boolean)
                toot.isRetooted = (bool)rawOrigToot["reblogged"];
            if (rawOrigToot["favourited"] != null && rawOrigToot["favourited"].Type == JTokenType.Boolean)
                toot.isFavorited = (bool)rawOrigToot["favourited"];
            if (rawOrigToot["reblogs_count"] != null && rawOrigToot["reblogs_count"].Type == JTokenType.Integer)
                toot.numRetoots = (int)rawOrigToot["reblogs_count"];
            if (rawOrigToot["favourites_count"] != null && rawOrigToot["favourites_count"].Type == JTokenType.Integer)
                toot.numFavorites = (int)rawOrigToot["favourites_count"];
            try
            {
                const string format = "MM/dd/yyyy HH:mm:ss";
                DateTime date = DateTime.ParseExact((String)rawOrigToot["created_at"], format, CultureInfo.InvariantCulture);
                toot.timeTooted = date.ToLocalTime();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                toot.timeTooted = DateTime.Now;
            }

            if (rawOrigToot["quoted_status_id_str"] != null && rawOrigToot["quoted_status"] != null)
            {
                toot.quotedToot = renderToot(rawOrigToot["quoted_status"], 0, true);
                toot.hasQuotedToot = true;
            }

            if (rawOrigToot["place"] != null)
            {
                toot.rawLocation = (String)rawOrigToot["place"]["full_name"];
                toot.hasLocation = true;
            }

            JObject rawEntities = new JObject();
            JObject extendedEntities = new JObject();

            JArray userMentions = new JArray();
            foreach (dynamic user in rawOrigToot["mentions"].Children())
            {
                JObject mention = new JObject();
                mention.Add("id", user["id"]);
                mention.Add("acct", user["acct"]);

                JArray indices = new JArray();

                String acct = user["acct"];

                if (acct.LastIndexOf("@") != -1)
                    acct = acct.Substring(0, acct.LastIndexOf("@"));

                int firstidx = toot.rawText.IndexOf("@" + acct);
                if (firstidx == -1)
                    continue;
                indices.Add(firstidx);
                indices.Add(firstidx + acct.Length + 1);

                mention.Add("indices", indices);
                userMentions.Add(mention);
            }

            rawEntities.Add("user_mentions", userMentions);

            JArray media = new JArray();
            foreach (JToken rawMedia in rawOrigToot["media_attachments"].Children())
            {
                if (((String)rawMedia["type"]).Equals("image"))
                {
                    JObject image = new JObject();
                    image.Add("id", rawMedia["id"]);
                    image.Add("preview_url", rawMedia["preview_url"]);
                    image.Add("url", rawMedia["url"]);

                    JArray indices = new JArray();

                    String mediaDisplay = (String)rawMedia["text_url"];
                    image.Add("display_url", mediaDisplay);

                    if (mediaDisplay == null || mediaDisplay.Length == 0)
                        continue;

                    int firstidx = toot.rawText.IndexOf(mediaDisplay);
                    if (firstidx == -1)
                        continue;
                    indices.Add(firstidx);
                    indices.Add(firstidx + mediaDisplay.Length);

                    image.Add("indices", indices);
                    media.Add(image);
                }
            }
            rawEntities.Add("media", media);
            extendedEntities.Add("media", media);

            JArray hashtags = new JArray();
            foreach (dynamic tag in rawOrigToot["tags"].Children())
            {
                JObject hashtag = new JObject();
                hashtag.Add("name", tag["name"]);

                JArray indices = new JArray();

                String tagName = tag["name"];

                int firstidx = toot.rawText.IndexOf("#" + tagName);
                if (firstidx == -1)
                    continue;
                indices.Add(firstidx);
                indices.Add(firstidx + tagName.Length + 1);

                hashtag.Add("indices", indices);
                hashtags.Add(hashtag);
            }
            rawEntities.Add("hashtags", hashtags);

            JArray urls = new JArray();
            String rawHtml = rawOrigToot["content"];
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(rawHtml);
            HtmlNodeCollection collection = doc.DocumentNode.SelectNodes("//a[@href and (@rel=\"nofollow noopener\" or @target=\"_blank\")]");
            if (collection != null)
            {
                foreach (HtmlNode link in collection)
                {
                    string hrefValue = link.GetAttributeValue("href", string.Empty);
                    String rawHTML = link.OuterHtml;

                    if (hrefValue.StartsWith("https://") || hrefValue.StartsWith("http://"))
                    {
                        String rawUrl = hrefValue;
                        String displayURL = rawUrl;
                        displayURL = displayURL.Replace("https://", "");
                        displayURL = displayURL.Replace("http://", "");

                        JObject url = new JObject();
                        url.Add("url", rawUrl);
                        url.Add("display_url", rawUrl);
                        url.Add("expanded_url", rawUrl);
                        JArray indices = new JArray();

                        String displayText = hrefValue;

                        int idx = toot.rawText.IndexOf(displayText);

                        if (idx == -1)
                            continue;

                        bool addItem = true;

                        foreach (JObject image in media)
                        {
                            if ((int)image["indices"][0] == idx)
                            {
                                addItem = false;
                                break;
                            }
                        }

                        if (addItem)
                        {
                            indices.Add(idx);
                            indices.Add(idx + displayText.Length);

                            url.Add("indices", indices);
                            urls.Add(url);
                        }
                    }
                }
            }
            rawEntities.Add("urls", urls);

            toot.rawEntities = rawEntities;
            toot.rawExtendedEntities = extendedEntities;

            toot.orderEntities();

            toot.parseExtendedEntities();
            toot.displayNavController = navController;
            if (!quoted)
            {
                if (index < 0)
                    timeline.Add(toot);
                else
                    timeline.Insert(index, toot);
            }
            return toot;
        }

        private void renderTimelineFromStream(object sender, String action, int index, Account account)
        {
            if (!account.accessToken.Equals(twitterAccountToken))
                return;
            Console.WriteLine("Type: " + timelineType + "; Action: " + action);
            if (action == "refresh")
            {
                dynamic rawTimeline = null;
                if (this.timelineType == "home")
                    rawTimeline = account.homeTimeline;
                if (this.timelineType == "mentions")
                    rawTimeline = account.mentionsTimeline;
                renderTimeline(rawTimeline, account);
            }
            if (action == "insert")
            {
                if (this.timelineType == "public")
                    this.renderToot(account.publicTimeline[0], 0, false);
                if (this.timelineType == "home")
                    this.renderToot(account.homeTimeline[0], 0, false);
                if (this.timelineType == "mentions")
                    this.renderToot(account.mentionsTimeline[0], 0, false);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    timelineBox.ItemsSource = null;
                    timelineBox.ItemsSource = timeline;
                }));
            }
            if (action == "delete")
            {
                if (index < 0 || index >= timeline.Count)
                    return;
                timeline.RemoveAt(index);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    timelineBox.ItemsSource = null;
                    timelineBox.ItemsSource = timeline;
                }));
            }
        }

        private void renderTimeline(dynamic rawTimeline, Account account)
        {
            timeline = new List<Toot>();
            if (rawTimeline == null)
                return;
            try {
                if (rawTimeline["error"] != null)
                {
                    errorDisplay.Visibility = Visibility.Visible;
                    errorReason.Content = "Error loading toots: " + (String)rawTimeline["error"];
                    return;
                }
            } catch (Exception e)
            {

            }
            foreach (dynamic rawToot in rawTimeline.Children())
            {
                renderToot(rawToot, -1, false);
            }
            Dispatcher.BeginInvoke(new Action(() =>
            {
                timelineBox.ItemsSource = timeline;
            }));
        }

        private void reloadImg_MouseEnter(object sender, MouseEventArgs e)
        {
            /*DoubleAnimation anim1 = new DoubleAnimation();
            anim1.From = 1;
            anim1.To = 0;
            Storyboard.SetTarget(anim1, reloadImg);
            Storyboard.SetTargetProperty(anim1, new PropertyPath(Control.OpacityProperty));

            DoubleAnimation anim2 = new DoubleAnimation();
            anim1.From = 0;
            anim1.To = 1;
            Storyboard.SetTarget(anim2, reloadImgSelected);
            Storyboard.SetTargetProperty(anim2, new PropertyPath(Control.OpacityProperty));

            Storyboard storyboard = new Storyboard();
            storyboard.SpeedRatio *= 2;
            storyboard.Children.Add(anim1);
            storyboard.Children.Add(anim2);
            storyboard.Begin();*/

            reloadImg.Opacity = 0;
            reloadImgSelected.Opacity = 1;
        }

        private void reloadImg_MouseLeave(object sender, MouseEventArgs e)
        {
            reloadImg.Opacity = 1;
            reloadImgSelected.Opacity = 0;
        }

        private void reloadBtn_Click(object sender, RoutedEventArgs e)
        {
            refreshTimeline();
        }

        private void tootBtn_Click(object sender, RoutedEventArgs e)
        {
            TootWindow tootWindow = new TootWindow(MastodonAPIWrapper.sharedApiWrapper.selectedAccount);
            if (this.profileID != "")
            {
                tootWindow.tootText.Text = "@"+this.title.Text+" ";
            }
            tootWindow.ShowDialog();
        }

        /*private void tootImg_MouseEnter(object sender, MouseEventArgs e)
        {
            tootImg.Opacity = 0;
            tootImgSelected.Opacity = 1;
        }

        private void tootImg_MouseLeave(object sender, MouseEventArgs e)
        {
            tootImg.Opacity = 1;
            tootImgSelected.Opacity = 0;
        }*/

        private void backBtn_Click(object sender, RoutedEventArgs e)
        {
            navController.popControl();
        }

        private void nameHandleLabel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TextBlock nameHandleLabel = (TextBlock)sender;
            Toot toot = (Toot)nameHandleLabel.DataContext;
            ProfilePanel panel = new ProfilePanel();
            panel.twitterAccountToken = twitterAccountToken;
            panel.profileUserID = toot.userID;
            panel.refreshProfile();
            navController.pushControl(panel);
        }

        private void timelineBoxItem_Click(object sender, MouseButtonEventArgs e)
        {
            Grid listItemGrid = (Grid)sender;
            Toot rawToot = (Toot)listItemGrid.DataContext;
            if (e.ClickCount == 2)
            {
                rawToot.details_Click(sender, e);
                return;
            }

            Grid listItemItems = (Grid)listItemGrid.Children[0];
            StackPanel tootContentsContainer = (StackPanel)listItemItems.Children[4];
            StackPanel tootContents = (StackPanel)tootContentsContainer.Children[0];
            Border tootPadding = (Border)tootContentsContainer.Children[2];
            Border toolbarBorder = (Border)listItemItems.Children[5];
            Thickness margin = toolbarBorder.Margin;
            Thickness padding = tootPadding.Padding;

            ThicknessAnimation paddingAnimation = new ThicknessAnimation();
            paddingAnimation.From = padding;

            ThicknessAnimation marginAnimation = new ThicknessAnimation();
            marginAnimation.From = margin;

            if (MastodonAPIWrapper.sharedApiWrapper.nightModeEnabled && !rawToot.isStartToot)
            {
                toolbarBorder.Background = new SolidColorBrush(Color.FromArgb(255, 61, 61, 61));
            }
            else
            {
                toolbarBorder.Background = new SolidColorBrush(Color.FromArgb(255, 194, 194, 194));
            }

            if (padding.Bottom == 0)
            {
                padding.Bottom = 35;
                margin.Bottom = 0;
                toolbarBorder.Visibility = Visibility.Visible;
            }
            else if (padding.Bottom == 35)
            {
                padding.Bottom = 0;
                margin.Bottom = -35;
                marginAnimation.Completed += (sender2, e2) =>
                    {
                        toolbarBorder.Visibility = Visibility.Collapsed;
                    };
            }
            else
            {
                return;
            }

            paddingAnimation.To = padding;
            marginAnimation.To = margin;

            Storyboard.SetTarget(paddingAnimation, tootPadding);
            Storyboard.SetTargetProperty(paddingAnimation, new PropertyPath(Border.PaddingProperty));

            Storyboard.SetTarget(marginAnimation, toolbarBorder);
            Storyboard.SetTargetProperty(marginAnimation, new PropertyPath(Border.MarginProperty));

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(paddingAnimation);
            storyboard.Children.Add(marginAnimation);
            storyboard.SpeedRatio *= 3.5;
            storyboard.Begin();
        }

        private void quote_Click(object sender, RoutedEventArgs e)
        {
            Grid senderElement = (Grid)sender;
            Toot boundData = (Toot)senderElement.DataContext;
            boundData.quote_Click(sender, e);
        }

        private void replyBtn_Click(object sender, RoutedEventArgs e)
        {
            Control senderElement = (Control)sender;
            Toot boundData = (Toot)senderElement.DataContext;
            boundData.replyBtn_Click(sender, e);
        }

        private void retootBtn_Click(object sender, RoutedEventArgs e)
        {
            Control senderElement = (Control)sender;
            Toot boundData = (Toot)senderElement.DataContext;
            boundData.retootBtn_Click(sender, e);
        }

        private void favoriteBtn_Click(object sender, RoutedEventArgs e)
        {
            Control senderElement = (Control)sender;
            Toot boundData = (Toot)senderElement.DataContext;
            boundData.favoriteBtn_Click(sender, e);
        }

        private void moreBtn_Click(object sender, RoutedEventArgs e)
        {
            Account twitterAccount = MastodonAPIWrapper.sharedApiWrapper.accountWithToken(twitterAccountToken);

            Control senderElement = (Control)sender;
            Toot boundData = (Toot)senderElement.DataContext;
            ContextMenu moreMenu = new ContextMenu();

            MenuItem detailsItem = new MenuItem();
            detailsItem.Header = "Post Details";
            detailsItem.Click += new RoutedEventHandler(boundData.details_Click);
            moreMenu.Items.Add(detailsItem);

            moreMenu.Items.Add(new Separator());

            if (boundData.userID == twitterAccount.accountID)
            {
                MenuItem deleteItem = new MenuItem();
                deleteItem.Header = "Delete Post";
                deleteItem.Click += new RoutedEventHandler(boundData.delete_Click);
                moreMenu.Items.Add(deleteItem);
            }
            else
            {
                MenuItem blockItem = new MenuItem();
                blockItem.Header = "Block " + boundData.user_display_screen_name;
                blockItem.Click += new RoutedEventHandler(boundData.block_Click);
                moreMenu.Items.Add(blockItem);
            }

            moreMenu.Items.Add(new Separator());

            MenuItem copyItem = new MenuItem();
            copyItem.Header = "Copy Post";
            copyItem.Click += new RoutedEventHandler(boundData.copy_Click);
            moreMenu.Items.Add(copyItem);

            MenuItem copyLinkItem = new MenuItem();
            copyLinkItem.Header = "Copy Link to Post";
            copyLinkItem.Click += new RoutedEventHandler(boundData.copyLink_Click);
            moreMenu.Items.Add(copyLinkItem);

            moreMenu.Margin = new Thickness(0);
            moreMenu.PlacementTarget = (UIElement)sender;
            moreMenu.IsOpen = true;
        }

        private void ClientLabel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Control senderElement = (Control)sender;
            Toot boundData = (Toot)senderElement.DataContext;
            if (boundData.clientLink.StartsWith("http://") || boundData.clientLink.StartsWith("https://"))
                Process.Start(boundData.clientLink);
        }

        private void RetootLabel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Control senderElement = (Control)sender;
            Toot boundData = (Toot)senderElement.DataContext;
            boundData.retootsClick(sender, e);
        }

        private void FavoriteLabel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Control senderElement = (Control)sender;
            Toot boundData = (Toot)senderElement.DataContext;
            boundData.favoritesClick(sender, e);
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Image rawControl = (Image)sender;
            Toot boundData = (Toot)rawControl.DataContext;
            boundData.mediaClick();
        }

        private void Image2_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Image rawControl = (Image)sender;
            Toot boundData = (Toot)rawControl.DataContext;
            boundData.mediaClick2();
        }

        private void Image3_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Image rawControl = (Image)sender;
            Toot boundData = (Toot)rawControl.DataContext;
            boundData.mediaClick3();
        }

        private void Image4_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Image rawControl = (Image)sender;
            Toot boundData = (Toot)rawControl.DataContext;
            boundData.mediaClick4();
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

        public void WillDisplay()
        {
            if (!isLoaded)
            {
                Account twitterAccount = MastodonAPIWrapper.sharedApiWrapper.accountWithToken(twitterAccountToken);

                MastodonAPIWrapper.sharedApiWrapper.getProfileAvatar(twitterAccount, accountImg);
            }

            if (!isLoaded)
                this.refreshTimeline();
        }

        private void Grid_ContextMenu(object sender, MouseButtonEventArgs e)
        {

        }

        private void timelineBoxItem_Enter(object sender, MouseEventArgs e)
        {
            return;
            Grid listItemGrid = (Grid)sender;
            Grid listItemItems = (Grid)listItemGrid.Children[0];
            StackPanel tootContentsContainer = (StackPanel)listItemItems.Children[4];
            StackPanel tootContents = (StackPanel)tootContentsContainer.Children[0];
            Border tootPadding = (Border)tootContentsContainer.Children[2];
            Border toolbarBorder = (Border)listItemItems.Children[5];
            Thickness margin = toolbarBorder.Margin;
            Thickness padding = tootPadding.Padding;
            Toot rawToot;
            try
            {
                rawToot = (Toot)listItemGrid.DataContext;
            }
            catch (Exception)
            {
                return;
            }

            ThicknessAnimation paddingAnimation = new ThicknessAnimation();
            paddingAnimation.From = padding;

            ThicknessAnimation marginAnimation = new ThicknessAnimation();
            marginAnimation.From = margin;

            if (MastodonAPIWrapper.sharedApiWrapper.nightModeEnabled && !rawToot.isStartToot)
            {
                toolbarBorder.Background = new SolidColorBrush(Color.FromArgb(255, 61, 61, 61));
            }
            else
            {
                toolbarBorder.Background = new SolidColorBrush(Color.FromArgb(255, 194, 194, 194));
            }

            if (padding.Bottom != 35)
            {
                padding.Bottom = 35;
                margin.Bottom = 0;
                toolbarBorder.Visibility = Visibility.Visible;
            }
            else
            {
                return;
            }

            paddingAnimation.To = padding;
            marginAnimation.To = margin;

            Storyboard.SetTarget(paddingAnimation, tootPadding);
            Storyboard.SetTargetProperty(paddingAnimation, new PropertyPath(Border.PaddingProperty));

            Storyboard.SetTarget(marginAnimation, toolbarBorder);
            Storyboard.SetTargetProperty(marginAnimation, new PropertyPath(Border.MarginProperty));

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(paddingAnimation);
            storyboard.Children.Add(marginAnimation);
            storyboard.SpeedRatio *= 3.5;
            storyboard.Begin();
            /*Grid listItemGrid = (Grid)sender;
            Toot boundData = (Toot)listItemGrid.DataContext;
            Grid listItemItems = (Grid)listItemGrid.Children[0];
            StackPanel tootContentsContainer = (StackPanel)listItemItems.Children[4];
            StackPanel tootContents = (StackPanel)tootContentsContainer.Children[0];
            StackPanel shareDetails = (StackPanel)tootContents.Children[5];
            if (boundData.numFavorites > 0 || boundData.numRetoots > 0)
                shareDetails.Visibility = Visibility.Visible;*/
        }

        private void timelineBoxItem_Leave(object sender, MouseEventArgs e)
        {
            return;
            Grid listItemGrid = (Grid)sender;
            Grid listItemItems = (Grid)listItemGrid.Children[0];
            StackPanel tootContentsContainer = (StackPanel)listItemItems.Children[4];
            StackPanel tootContents = (StackPanel)tootContentsContainer.Children[0];
            Border tootPadding = (Border)tootContentsContainer.Children[2];
            Border toolbarBorder = (Border)listItemItems.Children[5];
            Thickness margin = toolbarBorder.Margin;
            Thickness padding = tootPadding.Padding;
            Toot rawToot;
            try
            {
                rawToot = (Toot)listItemGrid.DataContext;
            } catch (Exception)
            {
                return;
            }

            ThicknessAnimation paddingAnimation = new ThicknessAnimation();
            paddingAnimation.From = padding;

            ThicknessAnimation marginAnimation = new ThicknessAnimation();
            marginAnimation.From = margin;

            if (MastodonAPIWrapper.sharedApiWrapper.nightModeEnabled && !rawToot.isStartToot)
            {
                toolbarBorder.Background = new SolidColorBrush(Color.FromArgb(255, 61, 61, 61));
            }
            else
            {
                toolbarBorder.Background = new SolidColorBrush(Color.FromArgb(255, 194, 194, 194));
            }

            if (padding.Bottom != 0)
            {
                padding.Bottom = 0;
                margin.Bottom = -35;
                marginAnimation.Completed += (sender2, e2) =>
                {
                    toolbarBorder.Visibility = Visibility.Collapsed;
                };
            }
            else
            {
                return;
            }

            paddingAnimation.To = padding;
            marginAnimation.To = margin;

            Storyboard.SetTarget(paddingAnimation, tootPadding);
            Storyboard.SetTargetProperty(paddingAnimation, new PropertyPath(Border.PaddingProperty));

            Storyboard.SetTarget(marginAnimation, toolbarBorder);
            Storyboard.SetTargetProperty(marginAnimation, new PropertyPath(Border.MarginProperty));

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(paddingAnimation);
            storyboard.Children.Add(marginAnimation);
            storyboard.SpeedRatio *= 3.5;
            storyboard.Begin();
            /*Grid listItemGrid = (Grid)sender;
            Toot boundData = (Toot)listItemGrid.DataContext;
            Grid listItemItems = (Grid)listItemGrid.Children[0];
            StackPanel tootContentsContainer = (StackPanel)listItemItems.Children[4];
            StackPanel tootContents = (StackPanel)tootContentsContainer.Children[0];
            StackPanel shareDetails = (StackPanel)tootContents.Children[5];
            shareDetails.Visibility = boundData.showShareDetails;*/
        }
    }
}
