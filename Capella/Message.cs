using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json.Linq;
using Capella.Models;

namespace Capella.Models
{
    public class Message
    {
        public String twitterAccountToken;

        public NavController displayNavController;
        public String tootID = "";
        public String rawText = "";
        public String origuser_screen_name = "";
        public String origuser_name = "";
        public String userID = "";
        private String kuser_screen_name = "";
        private String kuser_name = "";
        private DateTime ktimeTooted = DateTime.Now;
        private String kclientString = "";
        public String clientLink;
        public Uri user_profilepicurl = null;
        public dynamic rawEntities = null;
        public dynamic rawExtendedEntities = null;
        public JArray orderedEntities = null;

        private bool mediaFound = false;
        private Uri mediaUri = null;
        private Uri mediaUri2 = null;
        private Uri mediaUri3 = null;
        private Uri mediaUri4 = null;

        private int mediaCount;

        private bool mediaIsNotImage = false;
        private Uri rawMediaUri = null;

        public bool hasQuotedToot = false;
        public Toot quotedToot;

        ~Message()
        {
            displayNavController = null;
            tootID = null;
            rawText = null;
            origuser_screen_name = null;
            origuser_name = null;
            userID = null;
            kuser_screen_name = null;
            kuser_name = null;
            user_profilepicurl = null;
            rawEntities = null;
            orderedEntities = null;
            mediaUri = null;
        }

        public Color firstBackgroundColor
        {
            get
            {
                if (!MastodonAPIWrapper.sharedApiWrapper.nightModeEnabled)
                {
                    return Color.FromArgb(255, 241, 241, 241);
                }
                else
                {
                    return Color.FromArgb(255, 33, 33, 33);
                }
            }
        }

        public Color secondBackgroundColor
        {
            get
            {
                if (!MastodonAPIWrapper.sharedApiWrapper.nightModeEnabled)
                {
                    return Color.FromArgb(255, 241, 241, 241);
                }
                else
                {
                    return Color.FromArgb(255, 33, 33, 33);
                }
            }
        }

        public Brush textColor
        {
            get
            {
                if (!MastodonAPIWrapper.sharedApiWrapper.nightModeEnabled)
                {
                    return new SolidColorBrush(Color.FromArgb(255, 40, 40, 40));
                    return new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                }
                else
                {
                    return new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                }
            }
        }

        public Brush lightTextColor
        {
            get
            {
                if (!MastodonAPIWrapper.sharedApiWrapper.nightModeEnabled)
                {
                    return new SolidColorBrush(Color.FromArgb(255, 135, 153, 166));
                }
                else
                {
                    return new SolidColorBrush(Color.FromArgb(255, 238, 238, 238));
                }
            }
        }

        public Brush nameColor
        {
            get
            {
                return this.lightTextColor;
            }
        }

        public Brush linkColor
        {
            get
            {
                if (!MastodonAPIWrapper.sharedApiWrapper.nightModeEnabled)
                {
                    return new SolidColorBrush(Color.FromArgb(255, 42, 135, 212));
                }
                else
                {
                    return Brushes.LightBlue;
                }
            }
        }

        public Brush quotedBackgroundColor
        {
            get
            {
                if (!MastodonAPIWrapper.sharedApiWrapper.nightModeEnabled)
                    return Brushes.Gainsboro;
                else
                    return new SolidColorBrush(Color.FromArgb(255, 52, 52, 52));
            }
        }

        public String displayorig_user_name
        {
            get
            {
                return "Retooted By " + origuser_name;
            }
        }

        public String clientString
        {
            get
            {
                return kclientString;
            }
            set
            {
                kclientString = value;
            }
        }

        public String user_name
        {
            get
            {
                return kuser_name;
            }
            set
            {
                kuser_name = value;
            }
        }

        public String user_screen_name
        {
            get
            {
                return kuser_screen_name;
            }
            set
            {
                kuser_screen_name = value;
            }
        }

        public String user_display_screen_name
        {
            get
            {
                return "@"+kuser_screen_name;
            }
        }

        public BitmapImage user_profilepic
        {
            get
            {
                try
                {
                    return new BitmapImage(user_profilepicurl);
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }

        public DateTime timeTooted
        {
            get
            {
                return ktimeTooted;
            }
            set
            {
                ktimeTooted = value;
            }
        }

        public String timeTootedString
        {
            get
            {
                return String.Format("{0:MM/dd/yy} at {0:h:mm tt}", ktimeTooted);
            }
        }

        public void orderEntities()
        {
            orderedEntities = new JArray();

            JArray userMentions = rawEntities["user_mentions"];
            foreach (JObject mention in userMentions.Children())
            {
                if (mention["type"] != null)
                    mention.Remove("type");
                mention.Add("type", "user_mention");
                orderedEntities.Add(mention);
            }

            JArray links = rawEntities["urls"];
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

                String fullURL = (String)link["expanded_url"];
                Uri fullUri = (Uri)link["expanded_url"];
                if (!mediaFound) {
                    if (fullURL.StartsWith("http://d.pr/i/"))
                    {
                        mediaFound = true;
                        mediaUri = new Uri(fullURL + "+", UriKind.Absolute);
                    }
                    else if ((fullURL.StartsWith("http://gyazo.com/") || fullURL.StartsWith("https://gyazo.com/")) && (fullURL.EndsWith(".png") || fullURL.EndsWith(".bmp") || fullURL.EndsWith(".jpg") || fullURL.EndsWith(".jpeg")))
                    {
                        mediaFound = true;
                        if (fullURL.StartsWith("http://"))
                        {
                            String httpsURL = "https://" + fullURL.Remove(0, 7);
                            mediaUri = new Uri(httpsURL, UriKind.Absolute);
                        }
                        else
                            mediaUri = new Uri(fullURL, UriKind.Absolute);
                    }
                    else if (fullURL.StartsWith("http://cl.ly") && (fullURL.EndsWith(".png") || fullURL.EndsWith(".bmp") || fullURL.EndsWith(".jpg") || fullURL.EndsWith(".jpeg")))
                    {
                        mediaFound = true;
                        mediaUri = new Uri(fullURL, UriKind.Absolute);
                    } else if (fullUri.Host.EndsWith("youtube.com") && fullURL.Contains("watch"))
                    {
                        string watchParam = HttpUtility.ParseQueryString(fullUri.Query).Get("v");
                        if (watchParam.Length > 0)
                        {
                            mediaFound = true;
                            mediaUri = new Uri("https://img.youtube.com/vi/" + watchParam + "/mqdefault.jpg", UriKind.Absolute);
                            mediaIsNotImage = true;
                            rawMediaUri = fullUri;
                        }
                    }
                    else if (fullUri.Host.EndsWith("youtu.be"))
                    {
                        mediaFound = true;
                        mediaUri = new Uri("https://img.youtube.com/vi" + fullUri.LocalPath + "/mqdefault.jpg", UriKind.Absolute);
                        mediaIsNotImage = true;
                        rawMediaUri = fullUri;
                    }
                }
                if (link["type"] != null)
                    link.Remove("type");
                link.Add("type", "link");
                orderedEntities.Insert(y, link);
            }

            JArray hashtags = rawEntities["hashtags"];
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

            JArray medias = rawEntities["media"];
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

                    mediaFound = true;
                    mediaUri = new Uri((String)media["media_url_https"] + ":large");

                    if (media["type"] != null)
                        media.Remove("type");
                    media.Add("type", "media");
                    orderedEntities.Insert(y, media);
                }
            }
        }

        public void parseExtendedEntities()
        {
            if (rawExtendedEntities == null)
                return;
            if (rawExtendedEntities["media"] == null)
                return;

            mediaFound = true;

            int idx = 0;

            dynamic media = rawExtendedEntities["media"];
            foreach (dynamic picture in media.Children())
            {
                Uri uri = new Uri((String)picture["media_url_https"] + ":large");
                switch (idx)
                {
                    case 0:
                        mediaUri = uri;
                        break;
                    case 1:
                        mediaUri2 = uri;
                        break;
                    case 2:
                        mediaUri3 = uri;
                        break;
                    case 3:
                        mediaUri4 = uri;
                        break;
                }
                idx++;
                mediaCount = idx;
            }
        }

        public InlineCollection richText
        {
            get
            {
                Paragraph tootText = new Paragraph();

                int x = 0;
                String text = rawText;
                tootText.Inlines.Clear();

                foreach (dynamic entity in orderedEntities.Children())
                {
                    JArray indices = entity["indices"];
                    String sub = text.Substring(x, ((int)indices[0]) - x);
                    Run plainText = new Run(WebUtility.HtmlDecode(sub));
                    plainText.Foreground = this.textColor;
                    tootText.Inlines.Add(plainText);
                    x = (int)indices[1];

                    if ((String)entity["type"] == "user_mention")
                    {
                        int length = ((int)indices[1]) - ((int)indices[0]);
                        if ((int)indices[0] + length > rawText.Length)
                            length = rawText.Length - (int)indices[0];
                        if (length < 0)
                            length = 0;
                        sub = text.Substring((int)indices[0], length);
                        Run userMention = new Run(WebUtility.HtmlDecode(sub));
                        userMention.Foreground = this.linkColor;
                        Hyperlink userMentionLink = new Hyperlink(userMention);
                        userMentionLink.TextDecorations = null;
                        userMentionLink.Cursor = Cursors.Hand;
                        userMentionLink.NavigateUri = new Uri("http://www.example.com/");
                        userMentionLink.RequestNavigate += (sender, e) =>
                        {
                            ProfilePanel panel = new ProfilePanel();
                            panel.twitterAccountToken = twitterAccountToken;
                            panel.navController = displayNavController;
                            panel.profileScreenName = WebUtility.HtmlDecode(sub).Replace("@", "");
                            panel.refreshProfile();
                            displayNavController.pushControl(panel);
                        };
                        tootText.Inlines.Add(userMentionLink);
                    }

                    if ((String)entity["type"] == "hashtag")
                    {
                        int length = ((int)indices[1]) - ((int)indices[0]);
                        if ((int)indices[0] + length > rawText.Length)
                            length = rawText.Length - (int)indices[0];
                        if (length < 0)
                            length = 0;
                        sub = text.Substring((int)indices[0], length);
                        Run hashtag = new Run(WebUtility.HtmlDecode(sub));
                        hashtag.Foreground = this.linkColor;
                        Hyperlink hashtagLink = new Hyperlink(hashtag);
                        hashtagLink.TextDecorations = null;
                        hashtagLink.Cursor = Cursors.Hand;
                        hashtagLink.NavigateUri = new Uri("http://www.example.com/");
                        hashtagLink.RequestNavigate += (sender, e) =>
                        {
                            TimelinePanel panel = new TimelinePanel();
                            panel.twitterAccountToken = twitterAccountToken;
                            panel.timelineType = "search";
                            panel.isSearch = true;
                            panel.setTitle("\"" + sub + "\"");
                            panel.searchQuery = WebUtility.HtmlDecode(sub);
                            panel.refreshTimeline();
                            displayNavController.pushControl(panel);
                        };
                        tootText.Inlines.Add(hashtagLink);
                    }

                    if ((String)entity["type"] == "link")
                    {
                        int length = ((int)indices[1]) - ((int)indices[0]);
                        if ((int)indices[0] + length > rawText.Length)
                            length = rawText.Length - (int)indices[0];
                        if (length < 0)
                            length = 0;
                        sub = WebUtility.HtmlDecode(text.Substring((int)indices[0], length));

                        bool mediaURL = false;

                        if (mediaUri == null)
                        {
                            String fullURL = (String)entity["expanded_url"];
                            if (fullURL.EndsWith(".png") || fullURL.EndsWith(".bmp") || fullURL.EndsWith(".jpg") || fullURL.EndsWith(".jpeg"))
                            {
                                mediaFound = true;
                                mediaURL = true;
                                mediaUri = new Uri(fullURL, UriKind.Absolute);
                            }
                            else
                            if (fullURL.StartsWith("http://d.pr/i/"))
                            {
                                mediaFound = true;
                                mediaURL = true;
                                mediaUri = new Uri(fullURL + "+", UriKind.Absolute);
                            }
                            else if ((fullURL.StartsWith("http://gyazo.com/") || fullURL.StartsWith("https://gyazo.com/")) && (fullURL.EndsWith(".png") || fullURL.EndsWith(".bmp") || fullURL.EndsWith(".jpg") || fullURL.EndsWith(".jpeg")))
                            {
                                mediaFound = true;
                                mediaURL = true;
                                if (fullURL.StartsWith("http://"))
                                {
                                    String httpsURL = "https://" + fullURL.Remove(0, 7);
                                    mediaUri = new Uri(httpsURL, UriKind.Absolute);
                                }
                                else
                                    mediaUri = new Uri(fullURL, UriKind.Absolute);
                            }
                            else if (fullURL.StartsWith("http://twitpic.com/") || fullURL.StartsWith("https://twitpic.com/"))
                            {
                                mediaFound = true;
                                mediaURL = true;
                                Uri uri = new Uri(fullURL);
                                String id = uri.Segments.Last();
                                if (id.EndsWith("/"))
                                    id = id.Substring(0, id.Length - 1);
                                mediaUri = new Uri("https://twitpic.com/show/full/" + id, UriKind.Absolute);
                            }
                            else if (fullURL.StartsWith("http://cl.ly") && (fullURL.EndsWith(".png") || fullURL.EndsWith(".bmp") || fullURL.EndsWith(".jpg") || fullURL.EndsWith(".jpeg")))
                            {
                                mediaFound = true;
                                mediaURL = true;
                                mediaUri = new Uri(fullURL, UriKind.Absolute);
                            }
                            else if (fullURL.StartsWith("http://imgur.com") || fullURL.StartsWith("https://imgur.com") || fullURL.StartsWith("http://m.imgur.com") || fullURL.StartsWith("https:/m.imgur.com"))
                            {
                                if (!fullURL.EndsWith(".gifv"))
                                {
                                    mediaFound = true;
                                    mediaURL = true;
                                    Uri uri = new Uri(fullURL);
                                    String id = uri.Segments.Last();
                                    if (id.EndsWith("/"))
                                        id = id.Substring(0, id.Length - 1);
                                    mediaUri = new Uri("https://i.imgur.com/" + id + ".jpg", UriKind.Absolute);
                                }
                            }
                            else if (fullURL.StartsWith("http://i.imgur.com") || fullURL.StartsWith("https://i.imgur.com"))
                            {
                                if (!fullURL.EndsWith(".gifv"))
                                {
                                    mediaFound = true;
                                    mediaURL = true;
                                    mediaUri = new Uri(fullURL);
                                }
                            }
                            else if (fullURL.StartsWith("http://instagr.am/p/") || fullURL.StartsWith("http://instagram.com/p/") || fullURL.StartsWith("https://instagram.com/p/"))
                            {
                                mediaFound = true;
                                mediaURL = true;
                                Uri uri = new Uri(fullURL);
                                String id = uri.Segments.Last();
                                if (id.EndsWith("/"))
                                    id = id.Substring(0, id.Length - 1);
                                mediaUri = new Uri("http://instagram.com/p/" + id + "/media", UriKind.Absolute);
                            }
                        }

                        Run linkText = new Run((String)entity["display_url"]);
                        linkText.Foreground = this.linkColor;
                        Hyperlink link = new Hyperlink(linkText);
                        link.TextDecorations = null;
                        link.Cursor = Cursors.Hand;
                        link.NavigateUri = (Uri)entity["url"];
                        link.RequestNavigate += (sender, e) =>
                        {
                            if (mediaURL)
                            {
                                PictureViewer viewer = new PictureViewer();
                                viewer.image.Source = new BitmapImage(mediaUri);
                                viewer.Show();
                            }
                            else
                            {
                                String rawURL = (String)entity["expanded_url"];
                                if (rawURL.StartsWith("http://twitter.com") || rawURL.StartsWith("https://twitter.com"))
                                {
                                    Uri myUri = new Uri(rawURL);
                                    String[] urlComponents = myUri.LocalPath.Split('/');
                                    if (urlComponents.Count() >= 4)
                                    {
                                        if (urlComponents[2].ToLower().Equals("status"))
                                        {
                                            TimelinePanel linkedTimeline = new TimelinePanel();
                                            linkedTimeline.twitterAccountToken = twitterAccountToken;
                                            linkedTimeline.setTitle("");
                                            linkedTimeline.timelineType = "conversation";
                                            linkedTimeline.isConversation = true;
                                            linkedTimeline.conversationStartToot = urlComponents[3];
                                            linkedTimeline.refreshTimeline();
                                            this.displayNavController.pushControl(linkedTimeline);
                                            return;
                                        }
                                    }
                                    if (urlComponents.Count() == 2)
                                    {
                                        ProfilePanel linkedProfile = new ProfilePanel();
                                        linkedProfile.twitterAccountToken = twitterAccountToken;
                                        linkedProfile.profileScreenName = urlComponents[1];
                                        linkedProfile.refreshProfile();
                                        this.displayNavController.pushControl(linkedProfile);
                                        return;
                                    }
                                }
                                Process.Start((String)entity["url"]);
                            }
                        };
                        tootText.Inlines.Add(link);
                    }

                    if ((String)entity["type"] == "media")
                    {
                        int length = ((int)indices[1]) - ((int)indices[0]);
                        if ((int)indices[0] + length > rawText.Length)
                            length = rawText.Length - (int)indices[0];
                        if (length < 0)
                            length = 0;
                        sub = text.Substring((int)indices[0], length);

                        mediaFound = true;
                        if (mediaUri == null)
                        {
                            mediaUri = (Uri)entity["media_url_https"];
                        }

                        Run linkText = new Run((String)entity["display_url"]);
                        linkText.Foreground = this.linkColor;
                        Hyperlink link = new Hyperlink(linkText);
                        link.TextDecorations = null;
                        link.Cursor = Cursors.Hand;
                        link.NavigateUri = (Uri)entity["url"];
                        link.RequestNavigate += (sender, e) =>
                        {
                            PictureViewer viewer = new PictureViewer();
                            viewer.image.Source = new BitmapImage(mediaUri);
                            viewer.Show();
                        };
                        tootText.Inlines.Add(link);
                    }
                }
                if (x < text.Length)
                {
                    String sub = text.Substring(x, text.Length - x);
                    Run plainText = new Run(WebUtility.HtmlDecode(sub));
                    plainText.Foreground = this.textColor;
                    tootText.Inlines.Add(plainText);
                }

                return tootText.Inlines;
            }
        }

        /*public double richTextWidth {
            get
            {
                if (mediaFound == true)
                    return 136;
                return 186;
            }
        }*/

        public Thickness bottomPadding
        {
            get
            {
                int Height = 0;
                if (mediaFound == true || hasQuotedToot == true)
                    Height += 10;
                return new Thickness(0, Height, 0, 0);
            }
        }

        public BitmapImage mediaSource
        {
            get
            {
                if (mediaFound)
                    return new BitmapImage(mediaUri);
                return null;
            }
        }

        public BitmapImage mediaSource2
        {
            get
            {
                if (mediaCount > 1)
                    return new BitmapImage(mediaUri2);
                return null;
            }
        }

        public BitmapImage mediaSource3
        {
            get
            {
                if (mediaCount > 2)
                    return new BitmapImage(mediaUri3);
                return null;
            }
        }

        public BitmapImage mediaSource4
        {
            get
            {
                if (mediaCount > 3)
                    return new BitmapImage(mediaUri4);
                return null;
            }
        }

        public Visibility mediaVisibility
        {
            get
            {
                if (mediaFound)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public Visibility showQuotedToot
        {
            get
            {
                if (hasQuotedToot)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public InlineCollection quotedRichText
        {
            get
            {
                if (hasQuotedToot)
                    return quotedToot.richText;
                return null;
            }
        }

        public String quotedUser_name
        {
            get
            {
                if (hasQuotedToot)
                    return quotedToot.user_name;
                return null;
            }
            set
            {
                if (hasQuotedToot)
                    quotedToot.user_name = value;
            }
        }

        public String quotedUser_display_screen_name
        {
            get
            {
                if (hasQuotedToot)
                    return quotedToot.user_display_screen_name;
                return null;
            }
        }

        public int imageWidth
        {
            get
            {
                if (mediaCount > 2)
                    return 120;
                else
                    return 240;
            }
        }

        public void mediaClick()
        {
            if (mediaIsNotImage)
            {
                Process.Start(rawMediaUri.OriginalString);
            }
            else {
                ImageSource rawImage = this.mediaSource;

                var titleHeight = SystemParameters.WindowCaptionHeight + SystemParameters.ResizeFrameHorizontalBorderHeight;
                var verticalBorderWidth = SystemParameters.ResizeFrameVerticalBorderWidth;

                PictureViewer viewer = new PictureViewer();
                viewer.image.Source = rawImage;
                viewer.Width = rawImage.Width + (verticalBorderWidth * 2);
                viewer.Height = rawImage.Height + titleHeight + SystemParameters.ResizeFrameHorizontalBorderHeight;
                viewer.Show();
            }
        }

        public void mediaClick2()
        {
            ImageSource rawImage = this.mediaSource2;

            var titleHeight = SystemParameters.WindowCaptionHeight + SystemParameters.ResizeFrameHorizontalBorderHeight;
            var verticalBorderWidth = SystemParameters.ResizeFrameVerticalBorderWidth;

            PictureViewer viewer = new PictureViewer();
            viewer.image.Source = rawImage;
            viewer.Width = rawImage.Width + (verticalBorderWidth * 2);
            viewer.Height = rawImage.Height + titleHeight + SystemParameters.ResizeFrameHorizontalBorderHeight;
            viewer.Show();
        }

        public void mediaClick3()
        {
            ImageSource rawImage = this.mediaSource3;

            var titleHeight = SystemParameters.WindowCaptionHeight + SystemParameters.ResizeFrameHorizontalBorderHeight;
            var verticalBorderWidth = SystemParameters.ResizeFrameVerticalBorderWidth;

            PictureViewer viewer = new PictureViewer();
            viewer.image.Source = rawImage;
            viewer.Width = rawImage.Width + (verticalBorderWidth * 2);
            viewer.Height = rawImage.Height + titleHeight + SystemParameters.ResizeFrameHorizontalBorderHeight;
            viewer.Show();
        }

        public void mediaClick4()
        {
            ImageSource rawImage = this.mediaSource4;

            var titleHeight = SystemParameters.WindowCaptionHeight + SystemParameters.ResizeFrameHorizontalBorderHeight;
            var verticalBorderWidth = SystemParameters.ResizeFrameVerticalBorderWidth;

            PictureViewer viewer = new PictureViewer();
            viewer.image.Source = rawImage;
            viewer.Width = rawImage.Width + (verticalBorderWidth * 2);
            viewer.Height = rawImage.Height + titleHeight + SystemParameters.ResizeFrameHorizontalBorderHeight;
            viewer.Show();
        }
    }
}
