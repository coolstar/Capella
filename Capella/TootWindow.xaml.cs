using System;
using System.ComponentModel;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Capella
{
    /// <summary>
    /// Interaction logic for TootWindow.xaml
    /// </summary>
    public partial class TootWindow : Window
    {
        public MARGINS margins;

        public TootCell replyPreviewCell;
        public String tootInReplyTo;

        private double addedHeight = 0;

        //private String imageFileName = "";
        private ArrayList imageFileNames = new ArrayList();

        public Account twitterAccount;
        public TootWindow(Account account)
        {
            InitializeComponent();
            twitterAccount = account;
            if (MastodonAPIWrapper.sharedApiWrapper.nightModeEnabled)
            {
                this.Background = new SolidColorBrush(Color.FromArgb(255, 26, 26, 26));
                tootBackground.Background = Brushes.Black;
                tootText.Foreground = Brushes.White;
            }

            tootInReplyTo = "";

            refreshAccountImage();
            tootText.Focus();
        }

        public TootWindow(Account account, String _tootInReplyTo, String handleInReplyTo)
        {
            InitializeComponent();
            twitterAccount = account;
            if (MastodonAPIWrapper.sharedApiWrapper.nightModeEnabled)
            {
                tootText.Background = Brushes.Black;
                tootText.Foreground = Brushes.White;
            }

            this.tootInReplyTo = _tootInReplyTo;
            this.tootText.Text = handleInReplyTo + " ";

            this.replyPreviewCell = new TootCell();
            this.replyPreviewCell.twitterAccount = twitterAccount;
            this.replyPreviewCell.VerticalAlignment = VerticalAlignment.Top;
            this.replyPreviewCell.HorizontalAlignment = HorizontalAlignment.Left;
            this.replyPreviewCell.Width = this.Width;
            replyTootContainer.Children.Add(replyPreviewCell);

            replyTootContainer.Visibility = Visibility.Visible;

            refreshAccountImage();
            tootText.Focus();
            tootText.SelectionStart = tootText.Text.Length;
            tootText.SelectionLength = 0;
        }

        public void updateReply()
        {
            double pageHeight = replyPreviewCell.tootText.DesiredSize.Height;
            replyPreviewCell.tootText.Height = pageHeight;
            replyTootContainer.Height = pageHeight + 10;
            addedHeight = pageHeight + 20;
            this.Height += addedHeight;
        }

        private void refreshAccountImage()
        {
            accountImg.Source = MastodonAPIWrapper.sharedApiWrapper.accountImages[twitterAccount.accountID].Source;
        }

        private void tootBtn_Click(object sender, RoutedEventArgs e)
        {
            tootBtn.IsEnabled = false;
            tootText.IsEnabled = false;
            BackgroundWorker worker = new BackgroundWorker();
            String text = this.tootText.Text;
            bool isSensitive = markSensitive.IsChecked.Value;
            int visibility = visibilitySelection.SelectedIndex;
            worker.DoWork += (sender2, e2) =>
                {
                    /*if (imageFileName.Equals(""))
                        TwitterAPIWrapper.sharedApiWrapper.postToot(text, tootInReplyTo, twitterAccount);
                    else
                        TwitterAPIWrapper.sharedApiWrapper.postToot(text, tootInReplyTo, imageFileName, twitterAccount);*/
                    if (imageFileNames.Count == 0)
                        MastodonAPIWrapper.sharedApiWrapper.postToot(text, tootInReplyTo, isSensitive, visibility, twitterAccount);
                    else
                    {
                        ArrayList mediaIds = new ArrayList();
                        ArrayList mediaURLs = new ArrayList();
                        foreach (String fileName in imageFileNames)
                        {
                            dynamic mediaInfo = MastodonAPIWrapper.sharedApiWrapper.uploadMedia(twitterAccount, fileName);
                            mediaIds.Add((String)mediaInfo["id"]);
                            mediaURLs.Add((String)mediaInfo["text_url"]);
                        }
                        String mediaIdsStr = string.Join("&media_ids[]=", ((String[])mediaIds.ToArray(Type.GetType("System.String"))));
                        String mediaUrlsStr = string.Join("\n", ((String[])mediaURLs.ToArray(Type.GetType("System.String"))));
                        text += "\n" + mediaUrlsStr;
                        MastodonAPIWrapper.sharedApiWrapper.postToot(text, tootInReplyTo, isSensitive, visibility, mediaIdsStr, twitterAccount);
                    }
                };
            worker.RunWorkerCompleted += (sender2, e2) =>
                {
                    this.Close();
                };
            worker.RunWorkerAsync();
        }

        private void tootText_TextChanged(object sender, TextChangedEventArgs e)
        {
            String textToCheck = tootText.Text;
            textToCheck = Regex.Replace(textToCheck,
                @"((http|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?)",
                "aaaaaaaaaaaaaaaaaaaaaa");
            int tootLength = textToCheck.Length;
            if (/*imageFileName != "" || */imageFileNames.Count > 0)
                tootLength += 23;
            characterCount.Content = "" + (500 - tootLength);
            if ((500 - tootLength) < 0)
            {
                characterCount.Foreground = Brushes.Red;
                tootBtn.IsEnabled = false;
            }
            else
            {
                characterCount.Foreground = Brushes.Black;
                tootBtn.IsEnabled = true;
            }
        }

        private void imageSelector_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png, *.gif, *.bmp) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png; *.gif; *.bmp";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                addImage(ofd.FileName);
            }
        }

        private void addImage(String filePath)
        {
            if (imageFileNames.Count == 4)
                return;
            FileInfo info = new FileInfo(filePath);
            long fileLength = info.Length;
            if (fileLength < 3145728)
            {
                if (!imageFileNames.Contains(filePath))
                    imageFileNames.Add(filePath);
            }
            else
                MessageBox.Show("The selected image is larger than 3 MB.", "Error adding image", MessageBoxButton.OK);
            this.refreshImages();
        }

        private void refreshImages()
        {
            tootText_TextChanged(null, null);

            if (imageFileNames.Count == 0)
                this.Height = 200 + addedHeight;
            else if (imageFileNames.Count > 2)
                this.Height = 405 + addedHeight;
            else
                this.Height = 300 + addedHeight;

            tootImages.Margin = new Thickness(0, 134 + addedHeight, 0, 0);

            //this.replyPreviewCell.Margin = new Thickness(0, tootBackground.Height + 5, 10, 0);

            if (imageFileNames.Count >= 4)
                imageSelector.IsEnabled = false;
            else
                imageSelector.IsEnabled = true;

            if (imageFileNames.Count == 0)
                tootImages.Visibility = Visibility.Collapsed;
            else
                tootImages.Visibility = Visibility.Visible;

            while (tootImages.Children.Count > 0)
                tootImages.Children.RemoveAt(0);
            int i = 0;
            foreach (String fileName in imageFileNames)
            {
                int x = 5;
                int y = 0;
                if (i == 1 || i == 3)
                    x = 160;
                if (i > 1)
                    y = 105;

                Image image = new Image();
                image.HorizontalAlignment = HorizontalAlignment.Left;
                image.VerticalAlignment = VerticalAlignment.Top;
                image.Margin = new Thickness(x, y, 0, 0);
                image.Width = 150;
                image.Height = 100;
                image.Stretch = Stretch.UniformToFill;
                image.Source = new BitmapImage(new Uri(fileName));
                tootImages.Children.Add(image);

                Button deleteBtn = new Button();
                deleteBtn.HorizontalAlignment = HorizontalAlignment.Left;
                deleteBtn.VerticalAlignment = VerticalAlignment.Top;
                deleteBtn.Width = 20;
                deleteBtn.Height = 20;
                deleteBtn.Margin = new Thickness(x + 128, y + 2, 0, 0);
                deleteBtn.Style = Resources["FlatButton"] as Style;
                deleteBtn.DataContext = fileName;
                deleteBtn.Click += DeleteBtn_Click;
                deleteBtn.Cursor = Cursors.Hand;

                Image deleteBtnImage = new Image();
                deleteBtnImage.Source = new BitmapImage(new Uri("Resources/remove.png", UriKind.Relative));
                RenderOptions.SetBitmapScalingMode(deleteBtnImage, BitmapScalingMode.HighQuality);
                deleteBtn.Content = deleteBtnImage;

                tootImages.Children.Add(deleteBtn);

                i++;
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            Button deleteBtn = (Button)sender;
            String fileName = (String)deleteBtn.DataContext;
            imageFileNames.Remove(fileName);
            refreshImages();
        }

        private void imageSelectorImg_MouseEnter(object sender, MouseEventArgs e)
        {
            //if (imageFileName.Equals(""))
            //{
                imageSelectorImg.Opacity = 0.8;
            //}
        }

        private void optionsBtnImg_MouseLeave(object sender, MouseEventArgs e)
        {
            optionsButtonImage.Opacity = 0.3;
        }

        private void optionsBtnImg_MouseEnter(object sender, MouseEventArgs e)
        {
            //if (imageFileName.Equals(""))
            //{
            optionsButtonImage.Opacity = 0.8;
            //}
        }

        private void imageSelectorImg_MouseLeave(object sender, MouseEventArgs e)
        {
            imageSelectorImg.Opacity = 0.3;
        }

        private void FileDropped(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (String path in files)
                {
                    if (path.ToLower().EndsWith(".jpg") || path.ToLower().EndsWith(".jpeg") || path.ToLower().EndsWith(".jpe") || path.ToLower().EndsWith(".jfif") || path.ToLower().EndsWith(".png") || path.ToLower().EndsWith(".bmp"))
                    {
                        addImage(path);
                        //imageSelectorImg.Source = new BitmapImage(new Uri(imageFileName));
                        //imageSelectorClip.RadiusX = imageSelectorClip.RadiusY = 12.5;
                    }
                }
            }
        }

        private void accountImg_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ContextMenu accountMenu = new ContextMenu();
            foreach (Account account in MastodonAPIWrapper.sharedApiWrapper.accounts) {
                MenuItem accountItem = new MenuItem();

                String endpointDomain = account.endpoint;
                if (endpointDomain.IndexOf("/") != -1)
                {
                    endpointDomain = endpointDomain.Substring(0, endpointDomain.IndexOf("/"));
                }

                String fullHandle = account.myHandle + "@" + endpointDomain;
                accountItem.Header = fullHandle;
                accountItem.Click += AccountItem_Click;
                accountMenu.Items.Add(accountItem);
            }
            accountMenu.PlacementTarget = accountImg;
            accountMenu.IsOpen = true;
        }

        private void AccountItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem accountItem = (MenuItem)sender;
            foreach (Account account in MastodonAPIWrapper.sharedApiWrapper.accounts)
            {
                if (account.myHandle.Equals(accountItem.Header))
                {
                    twitterAccount = account;
                    refreshAccountImage();
                    break;
                }
            }
        }

        private void KeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                this.tootBtn_Click(sender, null);
            }
        }

        private void Textbox_DragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        private void optionsBtn_Click(object sender, RoutedEventArgs e)
        {
            optionsPopupBackdrop.Visibility = Visibility.Visible;
            optionsPopup.Visibility = Visibility.Visible;
        }

        private void optionsPopupBackdrop_MouseDown(object sender, MouseButtonEventArgs e)
        {
            optionsPopupBackdrop.Visibility = Visibility.Collapsed;
            optionsPopup.Visibility = Visibility.Collapsed;
        }
    }
}
