using Capella.Models;
using System;
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
        public Button directBtn;
        public Button searchBtn;
    }
}
