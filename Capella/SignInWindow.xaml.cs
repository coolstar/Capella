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

namespace Capella
{
    /// <summary>
    /// Interaction logic for SignInWindow.xaml
    /// </summary>
    public partial class SignInWindow : Window
    {
        public WelcomeWindow callbackDelegate;

        public SignInWindow()
        {
            InitializeComponent();
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            String streamCookie = "";
            String token = MastodonAPIWrapper.sharedApiWrapper.getAccountToken(username.Text, password.Password, out streamCookie);
            if (token != "")
            {
                callbackDelegate.accountToken = token;
                callbackDelegate.streamCookie = streamCookie;
                callbackDelegate.authenticated = true;
                this.Close();
            } else
            {
                MessageBox.Show("Error: Invalid Username or Password.");
            }
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
