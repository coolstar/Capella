using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media.Imaging;
using WPFGrowlNotification;

namespace Capella
{
    public class NotificationsHandler
    {
        //List<MentionsNotification> notifications = new List<MentionsNotification>();
        public static NotificationsHandler sharedNotificationsHandler;
        GrowlNotifiactions growlNotifications = new GrowlNotifiactions();
        private const double topOffset = 20;
        private const double leftOffset = 380;
        public NotificationsHandler()
        {
            sharedNotificationsHandler = this;
            growlNotifications.Top = SystemParameters.WorkArea.Top + topOffset;
            growlNotifications.Left = SystemParameters.WorkArea.Left + SystemParameters.WorkArea.Width - leftOffset;
        }
        public void pushNotification(dynamic tootNotification, int type)
        {
            String typeStr = "";
            if (type == 0)
                typeStr = "mentioned you.";
            else if (type == 1)
                typeStr = "sent you a direct message.";
            dynamic user;
            if (type == 1)
                user = tootNotification["sender"];
            else
                user = tootNotification["account"];

            String content = tootNotification["content"];
            content = Regex.Replace(content, "<.*?>", String.Empty);

            Notification notification = new Notification {
                Title = (String)user["display_name"] + " (" + "@" + (String)user["acct"] + ") " + typeStr,
                ImageUrl = ((String)user["avatar"]),
                Message = content
            };
            growlNotifications.AddNotification(notification);
            /*MentionsNotification notification = new MentionsNotification();
            notification.tootCell.tootID = tootNotification["id_str"];
            notification.tootCell.entities = tootNotification["entities"];

            dynamic user = tootNotification["user"];
            Uri ImageURL = (Uri)user["profile_image_url_https"];

            notification.tootCell.nameLabel.Content = user["name"];
            notification.tootCell.handleLabel.Content = "@" + (String)user["screen_name"];
            notification.tootCell.userHandle = (String)user["screen_name"];

            notification.tootCell.tootText.Text = WebUtility.HtmlDecode((String)tootNotification["text"]);

            BitmapImage profilePic = new BitmapImage();
            profilePic.BeginInit();
            profilePic.UriSource = ImageURL;
            profilePic.CacheOption = BitmapCacheOption.OnLoad;
            profilePic.EndInit();
            notification.tootCell.profilePic.Source = profilePic;

            notification.tootCell.tootText.Arrange(new Rect(0, 0, notification.tootCell.tootText.Width, 1000));
            double pageHeight = notification.tootCell.tootText.DesiredSize.Height;
            //cell.tootText.Height = pageHeight + 10;
            notification.tootCell.Height = 10 + pageHeight;
            notification.Height = notification.tootCell.Height + 24;

            Rect desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            if (notifications.Count > 0)
                notification.Top = notifications[notifications.Count - 1].Top - (notifications[notifications.Count - 1].ActualHeight + 5);
            else
                notification.Top = desktopWorkingArea.Height - (notification.Height + 40);
            notification.Left = desktopWorkingArea.Width - notification.Width;

            Console.WriteLine("Showing notification!");

            notification.Show();
            notifications.Add(notification);*/
        }
    }
}
