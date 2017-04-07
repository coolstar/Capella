using System;
using System.Windows;
using System.Windows.Controls;

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
            String token = MastodonAPIWrapper.sharedApiWrapper.getAccountToken(endpoint.Text, username.Text, password.Password);
            if (token != "")
            {
                callbackDelegate.accountToken = token;
                callbackDelegate.accountEndpoint = endpoint.Text.Remove(0, "https://".Length);
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

        private void endpoint_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!endpoint.Text.StartsWith("https://"))
            {
                endpoint.Text = "https://mastodon.social";
            }
        }
    }
}
