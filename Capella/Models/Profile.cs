using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Capella.Models
{
    public class Profile
    {
        /// <summary>
        /// The ID of the account
        /// </summary>
        [JsonProperty("id")]
        public String accountID { get; set; }

        /// <summary>
        /// The username of the account
        /// </summary>
        [JsonProperty("username")]
        public string Username { get; set; }

        /// <summary>
        /// Equals username for local users, includes @domain for remote ones
        /// </summary>
        [JsonProperty("acct")]
        public String Handle { get; set; }

        /// <summary>
        /// The account's display name
        /// </summary>
        [JsonProperty("display_name")]
        public String DisplayName { get; set; }

        /// <summary>
        /// Boolean for when the account cannot be followed without waiting for approval first
        /// </summary>
        [JsonProperty("locked")]
        public bool Locked;

        /// <summary>
        /// The time the account was created
        /// </summary>
        public string created_at;
        
        /// <summary>
        /// The number of followers for the account
        /// </summary>
        public int followers_count;
        
        /// <summary>
        /// The number of accounts the given account is following
        /// </summary>
        public int following_count;
        
        /// <summary>
        /// The number of statuses the account has made
        /// </summary>
        public int statuses_count;
        
        /// <summary>
        /// Biography of user
        /// </summary>
        public string note;

        /// <summary>
        /// URL of the user's profile page (can be remote)
        /// </summary>
        public string url;

        /// <summary>
        /// URL to the avatar image
        /// </summary>
        [JsonProperty("avatar")]
        public Uri Avatar = null;
        
        /// <summary>
        /// URL to the avatar static image (gif)
        /// </summary>
        public string avatar_static;
        
        /// <summary>
        /// URL to the header image
        /// </summary>
        public string header;

        /// <summary>
        /// URL to the header static image (gif)
        /// </summary>
        public string header_static;
        
        /// <summary>
        /// If the owner decided to switch accounts, new account is in this attribute
        /// </summary>
        public JObject moved;


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
