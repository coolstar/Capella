using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Newtonsoft.Json.Linq;
using Capella.Models;

namespace Capella
{
    /// <summary>
    /// Interaction logic for TootCell.xaml
    /// </summary>
    /// 

    public partial class TootCell : UserControl
    {
        public Account twitterAccount;

        public bool isShowingButtons, isComposingReply, isRetooted, isFavorited;
        public String tootID, screenName, userHandle, originalUserScreenName;
        public DateTime timeTooted;
        private Storyboard storyBoard;
        public double initialHeight;
        public NavController navController;
        public Image retootImg, favoriteImg;
        public dynamic entities;
        public TootCell()
        {
            InitializeComponent();
            isShowingButtons = false;
            this.initialHeight = 0;
            if (MastodonAPIWrapper.sharedApiWrapper.nightModeEnabled)
            {
                LinearGradientBrush background = new LinearGradientBrush();
                background.GradientStops.Add(new GradientStop(Color.FromArgb(255, 12, 12, 12), 0));
                background.GradientStops.Add(new GradientStop(Color.FromArgb(255, 12, 12, 12), 1));
                mainGrid.Background = background;

                this.tootText.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                this.nameHandleLabel.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            }
        }

        public void setIsRetooted()
        {
            this.retootedName.Visibility = Visibility.Visible;
            this.retootSymbol.Visibility = Visibility.Visible;
            this.profilePicFrame.Margin = new Thickness(this.profilePicFrame.Margin.Left, this.profilePicFrame.Margin.Top + 20, this.profilePicFrame.Margin.Right, this.profilePicFrame.Margin.Bottom);
            this.nameHandleLabel.Margin = new Thickness(this.nameHandleLabel.Margin.Left, this.nameHandleLabel.Margin.Top + 20, this.nameHandleLabel.Margin.Right, this.nameHandleLabel.Margin.Bottom);
            this.tootText.Margin = new Thickness(this.tootText.Margin.Left, this.tootText.Margin.Top + 20, this.tootText.Margin.Right, this.tootText.Margin.Bottom);
        }

        private void this_Click(object sender, MouseButtonEventArgs e)
        {
            if (isComposingReply == true)
                return;
        }

        public void highlightEntities()
        {
            JArray orderedEntities = new JArray();

            JArray userMentions = entities["user_mentions"];
            foreach (JObject mention in userMentions.Children()){
                if (mention["type"] != null)
                    mention.Remove("type");
                mention.Add("type", "user_mention");
                orderedEntities.Add(mention);
            }

            JArray links = entities["urls"];
            foreach (JObject link in links.Children())
            {
                int y = 0;
                int firstIndex = (int)link["indices"][0];
                foreach (dynamic entity in orderedEntities.Children())
                {
                    if ((int)entity["indices"][0] < firstIndex)
                        y++;
                    else
                        break;
                }
                if (link["type"] != null)
                    link.Remove("type");
                link.Add("type", "link");
                orderedEntities.Insert(y, link);
            }

            JArray hashtags = entities["hashtags"];
            if (hashtags != null)
            {
                foreach (JObject hashtag in hashtags.Children())
                {
                    int y = 0;
                    int firstIndex = (int)hashtag["indices"][0];
                    foreach (dynamic entity in orderedEntities.Children())
                    {
                        if ((int)entity["indices"][0] < firstIndex)
                            y++;
                        else
                            break;
                    }
                    if (hashtag["type"] != null)
                        hashtag.Remove("type");
                    hashtag.Add("type", "hashtag");
                    orderedEntities.Insert(y, hashtag);
                }
            }

            JArray medias = entities["media"];
            if (medias != null)
            {
                foreach (JObject media in medias.Children())
                {
                    int y = 0;
                    int firstIndex = (int)media["indices"][0];
                    foreach (dynamic entity in orderedEntities.Children())
                    {
                        if ((int)entity["indices"][0] < firstIndex)
                            y++;
                        else
                            break;
                    }
                    if (media["type"] != null)
                        media.Remove("type");
                    media.Add("type", "media");
                    orderedEntities.Insert(y, media);
                }
            }

            int x = 0;
            String text = tootText.Text;
            tootText.Inlines.Clear();

            Uri mediaUri = null;

            foreach (dynamic entity in orderedEntities.Children())
            {
                JArray indices = entity["indices"];
                String sub = text.Substring(x, ((int)indices[0]) - x);
                Run plainText = new Run(sub);
                tootText.Inlines.Add(plainText);
                x = (int)indices[1];

                if ((String)entity["type"] == "user_mention")
                {
                    int length = ((int)indices[1]) - ((int)indices[0]);
                    if ((int)indices[0] + length > tootText.Text.Length)
                        length = tootText.Text.Length - (int)indices[0];
                    if (length < 0)
                        length = 0;
                    sub = text.Substring((int)indices[0], length);
                    Run userMention = new Run(sub);
                    userMention.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 200, 255));
                    Hyperlink userMentionLink = new Hyperlink(userMention);
                    userMentionLink.TextDecorations = null;
                    userMentionLink.Cursor = Cursors.Arrow;
                    userMentionLink.NavigateUri = new Uri("http://www.example.com/");
                    tootText.Inlines.Add(userMentionLink);
                }

                if ((String)entity["type"] == "hashtag")
                {
                    int length = ((int)indices[1]) - ((int)indices[0]);
                    if ((int)indices[0] + length > tootText.Text.Length)
                        length = tootText.Text.Length - (int)indices[0];
                    if (length < 0)
                        length = 0;
                    sub = text.Substring((int)indices[0], length);
                    Run hashtag = new Run(sub);
                    hashtag.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 192, 0));
                    Hyperlink hashtagLink = new Hyperlink(hashtag);
                    hashtagLink.TextDecorations = null;
                    hashtagLink.Cursor = Cursors.Hand;
                    hashtagLink.NavigateUri = new Uri("http://www.example.com/");
                    tootText.Inlines.Add(hashtagLink);
                }

                if ((String)entity["type"] == "link")
                {
                    int length = ((int)indices[1]) - ((int)indices[0]);
                    if ((int)indices[0] + length > tootText.Text.Length)
                        length = tootText.Text.Length - (int)indices[0];
                    if (length < 0)
                        length = 0;
                    sub = text.Substring((int)indices[0], length);

                    String fullURL = (String)entity["expanded_url"];
                    if (fullURL.StartsWith("http://d.pr/i/"))
                    {
                        mediaUri = new Uri(fullURL + "+", UriKind.Absolute);
                    }
                    if ((fullURL.StartsWith("http://gyazo.com/") || fullURL.StartsWith("https://gyazo.com/")) && fullURL.EndsWith(".png"))
                    {
                        if (fullURL.StartsWith("http://"))
                        {
                            String httpsURL = "https://"+fullURL.Remove(0, 7);
                            mediaUri = new Uri(httpsURL, UriKind.Absolute);
                        }
                        else
                            mediaUri = new Uri(fullURL, UriKind.Absolute);
                    }

                    Run linkText = new Run((String)entity["display_url"]);
                    linkText.Foreground = Brushes.DarkBlue;
                    Hyperlink link = new Hyperlink(linkText);
                    link.TextDecorations = null;
                    link.Cursor = Cursors.Arrow;
                    link.NavigateUri = (Uri)entity["url"];
                    tootText.Inlines.Add(link);
                }

                if ((String)entity["type"] == "media")
                {
                    int length = ((int)indices[1]) - ((int)indices[0]);
                    if ((int)indices[0] + length > tootText.Text.Length)
                        length = tootText.Text.Length - (int)indices[0];
                    if (length < 0)
                        length = 0;
                    sub = text.Substring((int)indices[0], length);

                    mediaUri = (Uri)entity["media_url_https"];

                    Run linkText = new Run((String)entity["display_url"]);
                    linkText.Foreground = Brushes.DarkBlue;
                    Hyperlink link = new Hyperlink(linkText);
                    link.TextDecorations = null;
                    link.Cursor = Cursors.Arrow;
                    link.NavigateUri = (Uri)entity["url"];
                    tootText.Inlines.Add(link);
                }
            }
            if (x < text.Length)
            {
                String sub = text.Substring(x, text.Length - x);
                Run plainText = new Run(sub);
                tootText.Inlines.Add(plainText);
            }
        }

        private void userClick(object sender, MouseButtonEventArgs e)
        {
            if (navController == null)
                return;
            ProfilePanel profile = new ProfilePanel();
            profile.profileScreenName = this.userHandle;
            navController.pushControl(profile);
            profile.refreshProfile();
        }

        private void retootedName_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (navController == null)
                return;
            ProfilePanel profile = new ProfilePanel();
            profile.profileScreenName = originalUserScreenName;
            navController.pushControl(profile);
            profile.refreshProfile();
        }
    }
}
