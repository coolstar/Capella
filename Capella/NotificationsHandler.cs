using System;
using System.Text.RegularExpressions;
using System.Windows;
using WPFGrowlNotification;

using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using System.Net;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Runtime.InteropServices;
using DesktopNotifications;
using System.IO;
using OsInfo;
using OsInfo.Extensions;

namespace Capella
{
    // The GUID CLSID must be unique to your app. Create a new GUID if copying this code.
    [ClassInterface(ClassInterfaceType.None)]
    [ComSourceInterfaces(typeof(INotificationActivationCallback))]
    [Guid("85445821-9de4-4775-808f-13305a45e9a7"), ComVisible(true)]
    public class NotificationsActivator : NotificationActivator
    {
        public override void OnActivated(string invokedArgs, NotificationUserInput userInput, string appUserModelId)
        {
            // TODO: Handle activation
        }
    }

    public class NotificationsHandler
    {
        //List<MentionsNotification> notifications = new List<MentionsNotification>();
        public static NotificationsHandler sharedNotificationsHandler;
        GrowlNotifiactions growlNotifications = new GrowlNotifiactions();
        private const double topOffset = 20;
        private const double leftOffset = 380;
        private const String APP_ID = "com.squirrel.capella.Capella";
        OperatingSystem currentOS = Environment.OSVersion;

        public NotificationsHandler()
        {
            sharedNotificationsHandler = this;
            growlNotifications.Top = SystemParameters.WorkArea.Top + topOffset;
            growlNotifications.Left = SystemParameters.WorkArea.Left + SystemParameters.WorkArea.Width - leftOffset;
        }
        public void pushNotification(dynamic tootNotification/*, int type*/)
        {
            String typeStr = tootNotification["type"];
            if (typeStr == null)
                return;

            if (typeStr == "favourite")
                typeStr = "favorited your post";
            else if (typeStr == "mention")
                typeStr = "mentioned you";
            else if (typeStr == "reblog")
                typeStr = "boosted you";
            else
            {
                Console.WriteLine("Unhandled type: " + typeStr);
                return;
            }

            /*if (type == 0)
                typeStr = "mentioned you.";
            else if (type == 1)
                typeStr = "sent you a direct message.";
            dynamic user;
            if (type == 1)
                user = tootNotification["sender"];
            else
                user = tootNotification["account"];*/

            dynamic user = tootNotification["account"];

            dynamic toot = tootNotification["status"];

            String content = toot["content"];
            content = Regex.Replace(content, "<.*?>", String.Empty);

            if (currentOS.IsEqualTo(OsVersion.Win7))
            {
                WPFGrowlNotification.Notification notification = new WPFGrowlNotification.Notification
                {
                    Title = (String)user["display_name"] + " (" + "@" + (String)user["acct"] + ") " + typeStr,
                    ImageUrl = ((String)user["avatar"]),
                    Message = content
                };
                growlNotifications.AddNotification(notification);
            }
            else
            {
                // Construct notification
                WebClient webClient = new WebClient();
                String tempFile = Path.GetTempFileName();
                webClient.DownloadFileCompleted += (sender, e) =>
                {
                    ToastVisual visual = new ToastVisual()
                    {
                        BindingGeneric = new ToastBindingGeneric()
                        {
                            Children =
                        {
                        new AdaptiveText()
                        {
                            Text = (String)user["display_name"] + " " + typeStr,
                            HintMaxLines = 3
                        },

                        new AdaptiveText()
                        {
                            Text = WebUtility.HtmlDecode(content)
                        }
                        },

                        /*Attribution = new ToastGenericAttributionText()
                        {
                            Text = $"Via {(String)toot["application"]["name"]}"
                        },*/

                            AppLogoOverride = new ToastGenericAppLogo()
                            {
                                Source = tempFile,
                                HintCrop = ToastGenericAppLogoCrop.Circle
                            }
                        }

                    };

                // Now we can construct the final toast content
                ToastContent toastContent = new ToastContent()
                    {
                        Visual = visual,
                    //Actions = actions,

                    // Arguments when the user taps body of toast
                    /*Launch = new QueryString()
                    {
                        { "action", "viewConversation" },
                        { "conversationId", conversationId.ToString() }

                    }.ToString()*/
                    };
                // And create the toast notification
                XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(toastContent.GetContent());
                    var toast = new ToastNotification(xmlDoc);
                    ToastNotificationManager.CreateToastNotifier(APP_ID).Show(toast);
                    webClient.Dispose();
                };
                webClient.DownloadFileAsync(new Uri((String)user["avatar"]), tempFile);
            }

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
