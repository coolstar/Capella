using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Capella
{
    public class AccountUIHandler
    {
        public String twitterAccountToken;

        public Account twitterAccount
        {
            get
            {
                return MastodonAPIWrapper.sharedApiWrapper.accountWithToken(twitterAccountToken);
            }
        }

        public Button accountButton;
        public Image accountImage;
        public Button homeBtn;
        public Button mentionsBtn;
        public Button publicBtn;
        public Button userButton;
        public Button searchBtn;
    }
}
