using Newtonsoft.Json;
using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Capella.Models
{
    public class Profile
    {
        [JsonProperty("id")]
        public String accountID { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("acct")]
        public String Handle { get; set; }

        [JsonProperty("display_name")]
        public String DisplayName { get; set; }

        [JsonProperty("locked")]
        public bool Locked;

        public string created_at;
        public int followers_count;
        public int following_count;
        public int statuses_count;
        public string note;
        public string url;
        public string avatar_static;
        public string header;
        public string header_static;
        public string moved;

        [JsonProperty("avatar")]
        public Uri Avatar = null;

        public BitmapImage ProfilePic
        {
            get
            {
                return new BitmapImage(Avatar);
            }
        }

        public Color FirstBackgroundColor
        {
            get
            {
                if (MastodonAPIWrapper.sharedApiWrapper.nightModeEnabled)
                {
                    return Color.FromArgb(255, 29, 29, 29);
                }
                else
                {
                    return Color.FromArgb(255, 246, 246, 246);
                }
            }
        }

        public Color SecondBackgroundColor
        {
            get
            {
                if (MastodonAPIWrapper.sharedApiWrapper.nightModeEnabled)
                {
                    return Color.FromArgb(255, 1, 1, 1);
                }
                else
                {
                    return Color.FromArgb(255, 254, 254, 254);
                }
            }
        }

        public Brush TextColor
        {
            get
            {
                if (MastodonAPIWrapper.sharedApiWrapper.nightModeEnabled)
                {
                    return new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                }
                else
                {
                    return new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                }
            }
        }
    }
}
