using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Capella
{
    public class Profile
    {
        private String k_name = "";
        private String k_handle = "";
        public String accountID;
        public Uri profilePicUri = null;

        public String name
        {
            get
            {
                return k_name;
            }
            set
            {
                k_name = value;
            }
        }

        public String handle
        {
            get
            {
                return k_handle;
            }
            set
            {
                k_handle = value;
            }
        }

        public BitmapImage profilePic
        {
            get
            {
                return new BitmapImage(profilePicUri);
            }
        }

        public Color firstBackgroundColor
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

        public Color secondBackgroundColor
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

        public Brush textColor
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
