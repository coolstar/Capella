using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

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
                    dynamic search = MastodonAPIWrapper.sharedApiWrapper.searchUsers(twitterAccount, query, 200);
                    profilesList.list = search;
                    profilesList.convertList();
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
