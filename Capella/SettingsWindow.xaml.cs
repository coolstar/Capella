using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Capella.Models;

namespace Capella
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public MastodonAPIWrapper apiWrapper;
        private List<AccountUIHandler> accountUIHandlers;

        public SettingsWindow()
        {
            InitializeComponent();

            this.apiWrapper = MastodonAPIWrapper.sharedApiWrapper;

            int topMargin = 43;

            accountUIHandlers = new List<AccountUIHandler>();

            foreach (Account twitterAccount in MastodonAPIWrapper.sharedApiWrapper.accounts)
            {
                AccountUIHandler accountUIHandler = new AccountUIHandler();
                accountUIHandler.twitterAccountToken = twitterAccount.accessToken;

                Button accountButton = new Button();
                accountButton.ClipToBounds = false;
                accountButton.HorizontalAlignment = HorizontalAlignment.Left;
                accountButton.VerticalAlignment = VerticalAlignment.Top;
                accountButton.Height = 50;
                accountButton.Margin = new Thickness(13, topMargin, 0, 0);
                accountButton.Style = Resources["FlatButton"] as Style;
                //accountButton.Click += Account_Click;
                accountButton.Cursor = Cursors.Hand;
                this.sidebarGrid.Children.Add(accountButton);
                accountUIHandler.accountButton = accountButton;
                topMargin += 60;

                Image accountImage = new Image();
                accountImage.Clip = new RectangleGeometry(new Rect(0, 0, 50, 50), 4, 4);
                accountButton.Content = accountImage;
                accountUIHandler.accountImage = accountImage;

                accountImage.Opacity = 0;
                this.apiWrapper.getProfileAvatar(twitterAccount, accountImage);

                accountUIHandlers.Add(accountUIHandler);
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
