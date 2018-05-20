using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Capella.Models
{
    public class Toot
    {
        public String twitterAccountToken;

        public NavController displayNavController;

        /// <summary>
        /// The ID of the status
        /// </summary>
        [JsonProperty("id")]
        public string tootID { get; set; }

        /// <summary>
        /// A Fediverse-unique resource ID
        /// </summary>
        [JsonProperty("uri")]
        public string tootURL { get; set; }

        /// <summary>
        /// Body of the status; this will contain HTML (remote HTML already sanitized)
        /// </summary>
        [JsonProperty("content")]
        public string Content { get; set; }

        public string rawText
        {
            get
            {
                var text = Content;
                text = text.Replace("<br>", "\n");
                text = text.Replace("<br/>", "\n");
                text = text.Replace("<br />", "\n");
                text = text.Replace("</p><p>", "\n\n");
                text = Regex.Replace(text, "<.*?>", String.Empty);
                text = HttpUtility.HtmlDecode(text);
                return text;
            }
        }

        /// <summary>
        /// The Account which posted the status
        /// </summary>
        [JsonProperty("account")]
        public Profile Account { get; set; }

        /// <summary>
        /// null or the reblogged Status
        /// </summary>
        [JsonProperty("reblog")]
        public Toot Reblog { get; set; }

        public String origuser_screen_name => Reblog?.user_screen_name;
        public String origuser_name => Reblog?.user_name;
        public String userID = "";
        private DateTime ktimeTooted = DateTime.Now;
        private String kclientString = "";
        public String clientLink;
        public bool isRetootedStatus => Reblog != null;
        public bool isStartToot = false;
        public Uri user_profilepicurl = null;
        public dynamic rawEntities = null;
        public dynamic rawExtendedEntities = null;
        public JArray orderedEntities = null;

        public String rawSpoilerText = "";

        private bool mediaFound = false;
        private Uri mediaUri = null;
        private Uri mediaUri2 = null;
        private Uri mediaUri3 = null;
        private Uri mediaUri4 = null;
        private Uri fullMediaUri = null;
        private Uri fullMediaUri2 = null;
        private Uri fullMediaUri3 = null;
        private Uri fullMediaUri4 = null;

        private int mediaCount;

        private bool mediaIsNotImage = false;
        private Uri rawMediaUri = null;

        public bool isRetooted, isFavorited = false;
        public bool isSensitive = false;
        public bool hasQuotedToot = false;
        public Toot quotedToot;
        public bool hasLocation;
        public String rawLocation;
        public String visibility;

        public int numRetoots = 0, numFavorites = 0;

        public Color firstBackgroundColor
        {
            get
            {
                if (!MastodonAPIWrapper.sharedApiWrapper.nightModeEnabled)
                {
                    if (!isStartToot)
                        return Color.FromArgb(255, 241, 241, 241);
                    return Color.FromArgb(255, 33, 33, 33);
                }
                else
                {
                    if (!isStartToot)
                        return Color.FromArgb(255, 33, 33, 33);
                    return Color.FromArgb(255, 241, 241, 241);
                }
            }
        }

        public Color secondBackgroundColor
        {
            get
            {
                if (!MastodonAPIWrapper.sharedApiWrapper.nightModeEnabled)
                {
                    if (!isStartToot)
                        return Color.FromArgb(255, 241, 241, 241);
                    return Color.FromArgb(255, 33, 33, 33);
                }
                else
                {
                    if (!isStartToot)
                        return Color.FromArgb(255, 33, 33, 33);
                    return Color.FromArgb(255, 241, 241, 241);
                }
            }
        }

        public Brush textColor
        {
            get
            {
                if (!MastodonAPIWrapper.sharedApiWrapper.nightModeEnabled)
                {
                    if (!isStartToot)
                        return new SolidColorBrush(Color.FromArgb(255, 40, 40, 40));
                    return new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                }
                else
                {
                    if (!isStartToot)
                        return new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                    return new SolidColorBrush(Color.FromArgb(255, 40, 40, 40));
                }
            }
        }

        public Brush lightTextColor
        {
            get
            {
                if (!MastodonAPIWrapper.sharedApiWrapper.nightModeEnabled)
                {
                    if (!isStartToot)
                        return new SolidColorBrush(Color.FromArgb(255, 135, 153, 166));
                    return new SolidColorBrush(Color.FromArgb(255, 238, 238, 238));
                }
                else
                {
                    if (!isStartToot)
                        return new SolidColorBrush(Color.FromArgb(255, 238, 238, 238));
                    return new SolidColorBrush(Color.FromArgb(255, 135, 153, 166));
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
                    if (!isStartToot)
                        return new SolidColorBrush(Color.FromArgb(255, 42, 135, 212));
                    else
                        return Brushes.LightBlue;
                }
                else
                {
                    if (!isStartToot)
                        return Brushes.LightBlue;
                    else
                        return new SolidColorBrush(Color.FromArgb(255, 42, 135, 212));
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
                return "Boosted By " + origuser_name;
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

        public Thickness tootOffset
        {
            get
            {
                if (isRetootedStatus)
                    return new Thickness(0, 0, 0, 0);
                return new Thickness(0);
            }
        }

        public Visibility retootedVisibility
        {
            get
            {
                return isRetootedStatus ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public String user_name { get; set; } = "";

        public String user_screen_name { get; set; } = "";

        public String user_display_screen_name
        {
            get
            {
                return "@" + user_screen_name;
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

        public BitmapImage locationImage
        {
            get
            {
                if (!MastodonAPIWrapper.sharedApiWrapper.nightModeEnabled)
                {
                    if (!isStartToot)
                        return new BitmapImage(new Uri("Resources/location.png", UriKind.Relative));
                    else
                        return new BitmapImage(new Uri("Resources/location-night.png", UriKind.Relative));
                }
                else
                {
                    if (isStartToot)
                        return new BitmapImage(new Uri("Resources/location.png", UriKind.Relative));
                    else
                        return new BitmapImage(new Uri("Resources/location-night.png", UriKind.Relative));
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

        public Visibility showMoreDetails
        {
            get
            {
                if (isStartToot)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        public Visibility showShareDetails
        {
            get
            {
                if (isStartToot && numRetoots > 0 && numFavorites > 0)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        public Visibility showLocation
        {
            get
            {
                if (isStartToot && hasLocation)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        public String location
        {
            get
            {
                return rawLocation;
            }
        }

        public void orderEntities()
        {
            orderedEntities = new JArray();

            JArray userMentions = rawEntities["user_mentions"];
            foreach (JObject mention in userMentions.Children())
            {
                int y = 0;
                int firstIndex = (int)mention["indices"][0];
                foreach (dynamic entity in orderedEntities.Children())
                {
                    if ((int)entity["indices"][0] < firstIndex)
                        y++;
                    else
                        break;
                }

                if (mention["type"] != null)
                    mention.Remove("type");
                mention.Add("type", "user_mention");
                orderedEntities.Insert(y, mention);
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
                    if (!isStartToot)
                        mediaUri = new Uri((String)media["preview_url"]);
                    else
                        mediaUri = new Uri((String)media["url"]);
                    fullMediaUri = new Uri((String)media["url"]);

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
                Uri uri = null;
                if (!isStartToot)
                    uri = new Uri((String)picture["preview_url"]);
                else
                    uri = new Uri((String)picture["url"]);
                Uri fullUri = new Uri((String)picture["url"]);
                switch (idx)
                {
                    case 0:
                        mediaUri = uri;
                        fullMediaUri = fullUri;
                        break;
                    case 1:
                        mediaUri2 = uri;
                        fullMediaUri2 = fullUri;
                        break;
                    case 2:
                        mediaUri3 = uri;
                        fullMediaUri3 = fullUri;
                        break;
                    case 3:
                        mediaUri4 = uri;
                        fullMediaUri4 = fullUri;
                        break;
                }
                idx++;
                mediaCount = idx;
            }
        }

        public String spoilerText
        {
            get
            {
                return rawSpoilerText;
            }
        }

        public Visibility spoilerVisibility
        {
            get
            {
                return (!isSensitive) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public Visibility richTextVisibility
        {
            get
            {
                return (!isSensitive) ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public Visibility mediaVisibility
        {
            get
            {
                if (mediaFound && !isSensitive)
                    return Visibility.Visible;
                return Visibility.Hidden;
            }
        }

        public Thickness richTextMargin
        {
            get
            {
                if (rawSpoilerText == null || rawSpoilerText == "")
                {
                    return new Thickness(0, 24, 0, 0);
                } else
                {
                    return new Thickness(0);
                }
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
                            panel.profileUserID = entity["id"];
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
                            panel.timelineType = "tag/" + sub.Substring(1);
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
                            mediaUri = (Uri)entity["url"];
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
                            Console.WriteLine($"loading {mediaUri}");
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
                if (isRetootedStatus)
                    Height = 0;
                return new Thickness(0, Height, 0, 0);
            }
        }

        public RepeatBehavior videoBehavior {
            get {
                // if mediatype is gifv repeat
                return RepeatBehavior.Forever;
            }
        }

        public Uri videoSource
        {
            get
            {
                return new Uri("http://hubblesource.stsci.edu/sources/video/clips/details/images/hst_1.mpg");
                //if (mediaFound && me

                //return new Uri("https://assets.octodon.social/media_attachments/files/001/627/769/original/63ec188b3c29c9bd.mp4");
                JObject media = rawEntities["media"];
                //Console.WriteLine((String)media);
                if (((String)media["type"]).Equals("gifv")) {
                    return new Uri((String)media["url"]);
                }
                return new Uri("");
            }
        }

        public BitmapImage mediaSource
        {
            get
            {
                if (mediaFound && mediaUri != null)
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

        public BitmapImage retootImage
        {
            get
            {
                if (MastodonAPIWrapper.sharedApiWrapper.nightModeEnabled && !isStartToot)
                    return new BitmapImage(new Uri("Resources/retooted.png", UriKind.Relative));
                else
                    return new BitmapImage(new Uri("Resources/retooted.png", UriKind.Relative));
            }
        }

        public bool retootEnabled
        {
            get
            {
                return (visibility == "public" || visibility == "unlisted");
            }
        }

        public BitmapImage retootStatusDisplay
        {
            get
            {
                if (isRetooted)
                {
                    return new BitmapImage(new Uri("Resources/retooted.png", UriKind.Relative));
                }
                else
                {
                    if (retootEnabled)
                    {
                        if (MastodonAPIWrapper.sharedApiWrapper.nightModeEnabled && !isStartToot)
                            return new BitmapImage(new Uri("Resources/retoot.png", UriKind.Relative));
                        else
                            return new BitmapImage(new Uri("Resources/retoot.png", UriKind.Relative));
                    }
                    else
                    {
                        return new BitmapImage(new Uri("Resources/protected_dark.png", UriKind.Relative));
                    }
                }
            }
        }

        public BitmapImage favoriteStatusDisplay
        {
            get
            {
                if (isFavorited)
                {
                    return new BitmapImage(new Uri("Resources/favorite-glow.png", UriKind.Relative));
                }
                else
                {
                    if (MastodonAPIWrapper.sharedApiWrapper.nightModeEnabled && !isStartToot)
                        return new BitmapImage(new Uri("Resources/favorite-night.png", UriKind.Relative));
                    else
                        return new BitmapImage(new Uri("Resources/favorite.png", UriKind.Relative));
                }
            }
        }

        public BitmapImage replyBtnImage
        {
            get
            {
                if (MastodonAPIWrapper.sharedApiWrapper.nightModeEnabled && !isStartToot)
                        return new BitmapImage(new Uri("Resources/reply-night.png", UriKind.Relative));
                else
                        return new BitmapImage(new Uri("Resources/reply.png", UriKind.Relative));
            }
        }

        public BitmapImage moreBtnImage
        {
            get
            {
                if (MastodonAPIWrapper.sharedApiWrapper.nightModeEnabled && !isStartToot)
                    return new BitmapImage(new Uri("Resources/more-night.png", UriKind.Relative));
                else
                    return new BitmapImage(new Uri("Resources/more.png", UriKind.Relative));
            }
        }

        public void delete_Click(object sender, RoutedEventArgs e)
        {
            Account twitterAccount = MastodonAPIWrapper.sharedApiWrapper.accountWithToken(twitterAccountToken);
            MastodonAPIWrapper.sharedApiWrapper.deleteToot(this.tootID, twitterAccount);
        }

        public void block_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Blocking Users hasn't been implemented yet, sorry.", "Feature Not Implemented Yet");
        }

        public void copy_Click(object sender, RoutedEventArgs e)
        {
            //string text = new TextRange(tootText.Document.ContentStart, tootText.Document.ContentEnd).Text;
            System.Windows.Forms.Clipboard.SetDataObject(rawText, true, 5, 200);
        }

        public void copyLink_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.Clipboard.SetDataObject(tootURL, true, 5, 200);
        }

        public void replyBtn_Click(object sender, RoutedEventArgs e)
        {
            Account twitterAccount = MastodonAPIWrapper.sharedApiWrapper.accountWithToken(twitterAccountToken);
            String myAccount = twitterAccount.accountID;

            String usersToReplyTo = this.user_display_screen_name;

            if (isRetootedStatus)
            {
                usersToReplyTo += " @" + origuser_screen_name;
            }

            String text = rawText;

            bool userReplacementDone = false;

            foreach (dynamic entity in orderedEntities.Children())
            {
                JArray indices = entity["indices"];

                if ((String)entity["type"] == "user_mention")
                {
                    int length = ((int)indices[1]) - ((int)indices[0]);
                    if ((int)indices[0] + length > rawText.Length)
                        length = rawText.Length - (int)indices[0];
                    if (length < 0)
                        length = 0;
                    String sub = text.Substring((int)indices[0], length);
                    String userMention = WebUtility.HtmlDecode(sub);

                    if (userMention.Equals("@" + twitterAccount.myHandle) && (!userReplacementDone))
                    {
                        userMention = this.user_display_screen_name;
                        userReplacementDone = true;
                    }

                    if (usersToReplyTo.Contains(userMention))
                        continue;

                    if (!usersToReplyTo.Equals(""))
                        usersToReplyTo += " ";
                    usersToReplyTo += userMention;
                }
            }

            TootWindow toot = new TootWindow(twitterAccount, this.tootID, usersToReplyTo);
            toot.replyPreviewCell.tootID = this.tootID;

            toot.Title = "Reply to " + this.user_display_screen_name;

            toot.replyPreviewCell.tootText.Text = this.rawText;
            toot.replyPreviewCell.entities = this.rawEntities;
            toot.replyPreviewCell.highlightEntities();
            toot.replyPreviewCell.isComposingReply = true;

            toot.replyPreviewCell.tootText.Arrange(new Rect(0, 0, toot.replyPreviewCell.tootText.Width, 1000));
            
            toot.replyPreviewCell.profilePic.Source = this.user_profilepic;

            toot.replyPreviewCell.nameHandleLabel.Text = "";

            Run nameRun = new Run(this.user_name);
            nameRun.FontWeight = FontWeights.SemiBold;
            toot.replyPreviewCell.nameHandleLabel.Inlines.Add(nameRun);

            toot.replyPreviewCell.nameHandleLabel.Inlines.Add(new Run(" "));

            Run handleRun = new Run(this.user_display_screen_name);
            handleRun.FontStyle = FontStyles.Italic;
            handleRun.FontSize = 12;
            handleRun.Foreground = new SolidColorBrush(Color.FromArgb(255, 156, 156, 156));
            toot.replyPreviewCell.nameHandleLabel.Inlines.Add(handleRun);

            toot.updateReply();

            /*if (retootSymbol.Visibility == Visibility.Visible)
            {
                toot.replyPreviewCell.setIsRetooted();
            }*/

            toot.Show();
        }

        public void details_Click(object sender, RoutedEventArgs e)
        {
            TimelinePanel conversationView = new TimelinePanel();
            conversationView.twitterAccountToken = twitterAccountToken;
            conversationView.setTitle("");
            conversationView.isConversation = true;
            conversationView.timelineType = "conversation";
            conversationView.conversationStartToot = this.tootID;
            conversationView.refreshTimeline();
            displayNavController.pushControl(conversationView);
        }

        public void quote_Click(object sender, RoutedEventArgs e)
        {
            TimelinePanel conversationView = new TimelinePanel();
            conversationView.twitterAccountToken = twitterAccountToken;
            conversationView.setTitle("");
            conversationView.isConversation = true;
            conversationView.timelineType = "conversation";
            conversationView.conversationStartToot = this.quotedToot.tootID;
            conversationView.refreshTimeline();
            displayNavController.pushControl(conversationView);
        }

        public void retootsClick(object sender, RoutedEventArgs e)
        {
            Account twitterAccount = MastodonAPIWrapper.sharedApiWrapper.accountWithToken(twitterAccountToken);

            ProfilesList profilesList = new ProfilesList();
            profilesList.setTitle("Boosts");
            profilesList.twitterAccountToken = twitterAccountToken;
            displayNavController.pushControl(profilesList);

            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += (sender2, e2) =>
            {
                profilesList.profiles = Capella.MastodonAPIWrapper.sharedApiWrapper.retootsList(twitterAccount, tootID, 100);
            };
            backgroundWorker.RunWorkerCompleted += (sender2, e2) =>
            {
                profilesList.renderList();
            };
            backgroundWorker.RunWorkerAsync();
        }

        public void favoritesClick(object sender, RoutedEventArgs e)
        {
            Account twitterAccount = MastodonAPIWrapper.sharedApiWrapper.accountWithToken(twitterAccountToken);

            ProfilesList profilesList = new ProfilesList();
            profilesList.setTitle("Favorites");
            profilesList.twitterAccountToken = twitterAccountToken;
            displayNavController.pushControl(profilesList);

            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += (sender2, e2) =>
            {
                dynamic favoritesList = MastodonAPIWrapper.sharedApiWrapper.favoritesList(twitterAccount, tootID, 100);
                profilesList.profiles = favoritesList;
            };
            backgroundWorker.RunWorkerCompleted += (sender2, e2) =>
            {
                profilesList.renderList();
            };
            backgroundWorker.RunWorkerAsync();
        }

        public void retootBtn_Click(object sender, RoutedEventArgs e)
        {
            Button buttonClicked = (Button)sender;
            performRetoot((Image)buttonClicked.Content);
        }

        private void performRetoot(Image retootImg)
        {
            /*if (isRetooted == true)
            {
                MessageBox.Show("Undoing Retoots hasn't been implemented yet, sorry.", "Feature Not Implemented Yet");
                return;
            }*/
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (sender, e) =>
            {
                Account twitterAccount = MastodonAPIWrapper.sharedApiWrapper.accountWithToken(twitterAccountToken);
                isRetooted = MastodonAPIWrapper.sharedApiWrapper.retootToot(this.tootID, isRetooted, twitterAccount);
            };
            worker.RunWorkerCompleted += (sender, e) =>
            {
                if (isRetooted)
                {
                    retootImg.Source = new BitmapImage(new Uri("Resources/retooted.png", UriKind.Relative));
                }
                else
                {
                    retootImg.Source = new BitmapImage(new Uri("Resources/retoot.png", UriKind.Relative));
                }
            };
            worker.RunWorkerAsync();
        }

        public void favoriteBtn_Click(object sender, RoutedEventArgs e)
        {
            Button buttonClicked = (Button)sender;
            performFavorite((Image)buttonClicked.Content);
        }

        private void performFavorite(Image favoriteImg)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (sender, e) =>
            {
                Account twitterAccount = MastodonAPIWrapper.sharedApiWrapper.accountWithToken(twitterAccountToken);
                isFavorited = MastodonAPIWrapper.sharedApiWrapper.favoriteToot(this.tootID, isFavorited, twitterAccount);
            };
            worker.RunWorkerCompleted += (sender, e) =>
            {
                if (isFavorited)
                {
                    favoriteImg.Source = new BitmapImage(new Uri("Resources/favorite-glow.png", UriKind.Relative));
                }
                else
                {
                    favoriteImg.Source = new BitmapImage(new Uri("Resources/favorite.png", UriKind.Relative));
                }
            };
            worker.RunWorkerAsync();
        }

        public String favoritesCount
        {
            get
            {
                if (numFavorites != 0)
                    return String.Format("{0} Favorites", numFavorites);
                return "";
            }
        }

        public String retootsCount
        {
            get
            {
                if (numRetoots != 0)
                    return String.Format("{0} Boosts", numRetoots);
                return "";
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

        public async void AsyncImageLoad(Uri uri)
        {
            PictureViewer viewer = new PictureViewer();
            viewer.Show();

            WebClient client = new WebClient();
            client.DownloadProgressChanged +=
                delegate (object sender, DownloadProgressChangedEventArgs e)
                {
                    double bytesIn = e.BytesReceived;
                    double totalBytes = e.TotalBytesToReceive;
                    double percentage = bytesIn / totalBytes * 100;
                    viewer.DownloadProgressBar.Value = percentage;
                };

            client.DownloadDataCompleted +=
                delegate (object sender, DownloadDataCompletedEventArgs e)
                {
                    viewer.DownloadProgressBar.Visibility = Visibility.Hidden;
                };
            var bytes = await client.DownloadDataTaskAsync(uri);
            BitmapImage rawImage = new BitmapImage();
            rawImage.BeginInit();
            rawImage.CacheOption = BitmapCacheOption.OnLoad;
            rawImage.StreamSource = new MemoryStream(bytes);
            rawImage.EndInit();

            var titleHeight = SystemParameters.WindowCaptionHeight + SystemParameters.ResizeFrameHorizontalBorderHeight;
            var verticalBorderWidth = SystemParameters.ResizeFrameVerticalBorderWidth;


            viewer.image.Source = rawImage;
            viewer.Width = rawImage.Width + (verticalBorderWidth * 2);
            viewer.Height = rawImage.Height + titleHeight + SystemParameters.ResizeFrameHorizontalBorderHeight;
        }

        public void mediaClick()
        {
            if (mediaIsNotImage)
            {
                Process.Start(rawMediaUri.OriginalString);
            }
            else {

                AsyncImageLoad(fullMediaUri);
            }
        }

        public void mediaClick2()
        {
            AsyncImageLoad(fullMediaUri2);
        }

        public void mediaClick3()
        {
            AsyncImageLoad(fullMediaUri3);
        }

        public void mediaClick4()
        {
            AsyncImageLoad(fullMediaUri4);
        }
    }
}
