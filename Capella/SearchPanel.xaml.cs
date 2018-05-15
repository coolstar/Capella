using Capella.Models;
using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Capella
{
    /// <summary>
    /// Interaction logic for SearchPanel.xaml
    /// </summary>
    public partial class SearchPanel : UserControl
    {
        public String twitterAccountToken;

        public NavController navController;
        public SearchPanel()
        {
            InitializeComponent();
            if (MastodonAPIWrapper.sharedApiWrapper.nightModeEnabled)
            {
                contentsPanel.Background = new SolidColorBrush(Color.FromArgb(255, 24, 24, 24));
                this.Background = new SolidColorBrush(Color.FromArgb(255, 26, 26, 26));
                title.Foreground = Brushes.White;
            }
        }

        public SearchPanel createCopy()
        {
            SearchPanel panelCopy = new SearchPanel();
            panelCopy.twitterAccountToken = twitterAccountToken;
            return panelCopy;
        }

        private void searchField_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                ProfilesList profilesList = new ProfilesList();
                profilesList.twitterAccountToken = twitterAccountToken;
                profilesList.setTitle("\"" + searchField.Text + "\"");
                navController.pushControl(profilesList);

                String query = searchField.Text;

                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += (sender2, e2) =>
                {
                    Account twitterAccount = MastodonAPIWrapper.sharedApiWrapper.accountWithToken(twitterAccountToken);
                    profilesList.profiles = MastodonAPIWrapper.sharedApiWrapper.searchUsers(twitterAccount, query, 200);
                };
                worker.RunWorkerCompleted += (sender2, e2) =>
                {
                    profilesList.renderList();
                };
                worker.RunWorkerAsync();
            }
        }

        public void WillDisplay()
        {
            searchField.Text = "";
        }
    }
}
