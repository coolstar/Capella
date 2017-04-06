using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocketSharp;
using System.Security.Authentication;

namespace Capella
{
    public class MastodonAPIWrapper
    {
        public delegate void TimelineChanged(object sender, String action, int index, Account account);
        public event TimelineChanged publicTimelineChanged;
        public event TimelineChanged homeTimelineChanged;
        public event TimelineChanged mentionsTimelineChanged;

        public OAuthUtils sharedOAuthUtils;
        public List<Account> accounts;
        public Account selectedAccount;
        public bool nightModeEnabled = false;
        public List<String> keywords = new List<String>();
        public Dictionary<String, Image> accountImages;

        public String endpoint = "mastodon.social";

        public MastodonAPIWrapper()
        {
            Console.WriteLine("Initializing Capella Mastodon API Wrapper...");

            sharedApiWrapper = this;
            sharedOAuthUtils = new OAuthUtils();
            this.consumerKey = "";
            this.consumerSecret = "";

            sharedOAuthUtils.getTokens("https://"+endpoint+"/", out this.consumerKey, out this.consumerSecret);

            Console.WriteLine(this.consumerKey);
            Console.WriteLine(this.consumerSecret);

            try
            {
                if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Capella\\settings.json"))
                {
                    return;
                }
                String rawJson = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Capella\\settings.json");
                dynamic json = JsonConvert.DeserializeObject(rawJson);

                if (json["version"] != null)
                {
                    double version = json["version"];
                    if (version < 0.25)
                    {
                        return;
                    }
                } else
                {
                    return;
                }

                if (json["nightModeEnabled"] != null)
                {
                    nightModeEnabled = (bool)json["nightModeEnabled"];
                }

                if (json["mutes"] != null)
                {
                    dynamic mutes = json["mutes"];
                    if (mutes["keywords"] != null)
                    {
                        JArray rawKeywords = mutes["keywords"];
                        foreach (String keyword in rawKeywords.Children()){
                            keywords.Add(keyword);
                        }
                    }
                }

                dynamic accountsTokens = json["accounts"];
                if (accountsTokens == null)
                {
                    return;
                }

                if (accountsTokens.Count == 0)
                {
                    return;
                }

                Console.WriteLine("Loading Accounts...");

                accounts = new List<Account>();
                foreach (dynamic accountTokens in accountsTokens){
                    Account account = new Account();

                    Object rawAccessToken = (String)accountTokens["token"];

                    account.accessToken = (String)rawAccessToken;

                    account.myHandle = getCurrentHandle(account);

                    account.blockedIDs = new List<String>();
                    /*BackgroundWorker worker = new BackgroundWorker();
                    worker.DoWork += (sender, e) =>
                        {
                            JObject rawData = (JObject)JsonConvert.DeserializeObject(sharedOAuthUtils.GetData("https://localhost/1.1/blocks/ids.json", "stringify_ids=true", account, true));
                            JArray blockedIDs = (JArray)rawData["ids"];
                            foreach (JValue id in blockedIDs.Children())
                            {
                                account.blockedIDs.Add((String)id);
                            }
                        };
                    worker.RunWorkerAsync();*/

                    accounts.Add(account);
                }

                accountImages = new Dictionary<string, Image>();

                selectedAccount = accounts[0];
                String[] args = Environment.GetCommandLineArgs();
                if (args.Length > 1)
                {
                    String rawSelectedAccount = args[1];
                    int selectedIdx;
                    if (Int32.TryParse(rawSelectedAccount, out selectedIdx))
                    {
                        if (selectedIdx < accounts.Count)
                            selectedAccount = accounts[selectedIdx];
                    }
                }
                Console.WriteLine("Capella Mastodon API Wrapper Initialized.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            //this.accessToken = "379029313-avcZCimJqvy0sA86fkKESqAuadqCzGnWCDXsg0i4";
            //this.accessTokenSecret = "Usm5pK86YoorXk7VaUAgRnCQXzZTnoB0g4Q2ATo0";
        }

        public String getAccountToken(String username, String password, out String streamCookie)
        {
            return sharedOAuthUtils.getAccountToken("https://" + endpoint + "/", consumerKey, consumerSecret, username, password, out streamCookie);
        }

        public Account accountWithToken(String accessToken)
        {
            foreach (Account twitterAccount in accounts)
            {
                if (twitterAccount.accessToken.Equals(accessToken))
                    return twitterAccount;
            }
            return null;
        }

        public String getCurrentHandle(Account account)
        {
            String cookieHeader;
            String halfProfile = sharedOAuthUtils.GetData("https://" + endpoint + "/api/v1/accounts/verify_credentials", "", account, false, out cookieHeader);
            dynamic accountData = JsonConvert.DeserializeObject(halfProfile);
            if (account.accountID == null)
            {
                account.accountID = "" + accountData["id"];
            }
            return accountData["username"];
        }

        public dynamic getProfile(String accountID, Account account)
        {
            try
            {
                if (accountID == null || accountID == "")
                {
                    String halfProfile = sharedOAuthUtils.GetData("https://" + endpoint + "/api/v1/accounts/verify_credentials", "", account, true);
                    dynamic accountData = JsonConvert.DeserializeObject(halfProfile);
                    accountID = "" + accountData["id"];
                    if (account.accountID == null)
                    {
                        account.accountID = "" + accountData["id"];
                    }
                }

                String json = sharedOAuthUtils.GetData("https://" + endpoint + "/api/v1/accounts/"+ accountID, "", account, true);
                dynamic profile = JsonConvert.DeserializeObject(json);
                return profile;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        public void getProfileAvatar(Account twitterAccount, Image accountImage)
        {
            dynamic profile = null;
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (sender, e) =>
            {
                profile = this.getProfile("", twitterAccount);
            };
            worker.RunWorkerCompleted += (sender, e) =>
            {
                DoubleAnimation anim = new DoubleAnimation();
                anim.From = 0;
                if (twitterAccount.accessToken.Equals(MastodonAPIWrapper.sharedApiWrapper.selectedAccount.accessToken))
                    anim.To = 1;
                else
                    anim.To = 0.5;
                Storyboard.SetTarget(anim, accountImage);
                Storyboard.SetTargetProperty(anim, new PropertyPath(UserControl.OpacityProperty));

                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(anim);
                storyboard.SpeedRatio *= 2;
                storyboard.Begin();
                try
                {
                    accountImage.Source = new BitmapImage(new Uri((String)profile["avatar"], UriKind.Absolute));
                    MastodonAPIWrapper.sharedApiWrapper.accountImages[twitterAccount.accountID] = accountImage;
                }
                catch (Exception e2)
                {
                }
                profile.RemoveAll();
            };
            worker.RunWorkerAsync();
        }

        public dynamic getNotifications(Account account, String maximumID)
        {
            bool cacheValid = false;
            if (File.Exists(Path.GetTempPath() + "Capella\\notifications_" + account.accessToken + ".json"))
            {
                if (!NetworkInterface.GetIsNetworkAvailable())
                    cacheValid = true;
                DateTime lastModified = File.GetLastWriteTimeUtc(Path.GetTempPath() + "Capella\\notifications_" + account.accessToken + ".json");
                TimeSpan ts = DateTime.UtcNow - lastModified;
                if (ts.TotalSeconds < 30)
                {
                    cacheValid = true;
                }
            }
            if (maximumID != null && maximumID != "")
                cacheValid = false;
            String json;
            dynamic notifications = null;
            if (cacheValid)
            {
                json = File.ReadAllText(Path.GetTempPath() + "Capella\\notifications_" + account.accessToken + ".json", UTF8Encoding.UTF8);
                notifications = JsonConvert.DeserializeObject(json);
            }
            else
            {
                String queryStr = "limit=30";
                if (maximumID != null && maximumID != "")
                    queryStr += "&max_id=" + maximumID;
                json = sharedOAuthUtils.GetData("https://" + endpoint + "/api/v1/notifications/", queryStr, account, false);

                if (!Directory.Exists(Path.GetTempPath() + "Capella\\"))
                    Directory.CreateDirectory(Path.GetTempPath() + "Capella\\");
                notifications = JsonConvert.DeserializeObject(json);
                String cachePath = Path.GetTempPath() + "Capella\\notifications_" + account.accessToken + ".json";
                if (notifications != null && (maximumID == null || maximumID == ""))
                {
                    if (((Object)notifications).GetType() == typeof(JArray))
                        File.WriteAllText(cachePath, json, UTF8Encoding.UTF8);
                }
            }
            return notifications;
        }

        public dynamic getMentionsShim(Account account)
        {
            dynamic notifications = null;
            String maxID = "";
            JArray timeline = new JArray();
            for (int i = 0; i < 3; i++)
            {
                notifications = getNotifications(account, maxID);
                foreach (JObject rawNotification in notifications.Children())
                {
                    String notificationType = (String)rawNotification["type"];
                    if (notificationType.Equals("mention"))
                    {
                        dynamic toot = rawNotification["status"];
                        timeline.Add(toot);
                    }
                    maxID = (String)rawNotification["id"];
                }

                account.mentionsTimeline = timeline;
                account.mentionsTimelineIds = new List<String>();
                foreach (JObject rawToot in timeline.Children())
                {
                    JObject toot = rawToot;
                    String tootID = (String)toot["id"];
                    String text = (String)toot["content"];
                    bool mute = false;
                    foreach (String keyword in MastodonAPIWrapper.sharedApiWrapper.keywords)
                    {
                        if (text.ToLower().Contains(keyword.ToLower()))
                        {
                            mute = true;
                        }
                    }
                    if (!mute && !account.mentionsTimelineIds.Contains(tootID))
                        account.mentionsTimelineIds.Add(tootID);
                }
                if (mentionsTimelineChanged != null)
                    mentionsTimelineChanged(this, "refresh", 0, account);
            }
            return timeline;
        }

        public dynamic getTimeline(Account account, String timelineType, String targetID, String sinceID)
        {
            if (timelineType == "mentions")
            {
                if ((targetID != null && targetID != "") || (sinceID != null && sinceID != ""))
                    throw new Exception("Mentions does not support target or since ID on Mastodon.");
                return getMentionsShim(account);
            }
            bool cacheValid = false;
            if (File.Exists(Path.GetTempPath() + "Capella\\" + timelineType + "_timeline"+targetID+"_" + account.accessToken + ".json"))
            {
                if (!NetworkInterface.GetIsNetworkAvailable())
                    cacheValid = true;
                DateTime lastModified = File.GetLastWriteTimeUtc(Path.GetTempPath() + "Capella\\" + timelineType + "_timeline" + targetID + "_" + account.accessToken + ".json");
                TimeSpan ts = DateTime.UtcNow - lastModified;
                if (ts.TotalSeconds < 30)
                {
                    cacheValid = true;
                }
            }
            if (sinceID != null && !sinceID.Equals(""))
                cacheValid = false;
            String json;
            dynamic timeline = null;
            if (cacheValid)
            {
                json = File.ReadAllText(Path.GetTempPath() + "Capella\\" + timelineType.Replace("/","_") + "_timeline" + targetID + "_" + account.accessToken + ".json", UTF8Encoding.UTF8);
                timeline = JsonConvert.DeserializeObject(json);
            }
            else
            {
                String query = "";
                if (sinceID != null && !sinceID.Equals(""))
                    query = "?since_id=" + sharedOAuthUtils.UrlEncode(sinceID);

                if (targetID == "")
                    json = sharedOAuthUtils.GetData("https://" + endpoint + "/api/v1/timelines/" + timelineType, query, account, false);
                else
                    json = sharedOAuthUtils.GetData("https://" + endpoint + "/api/v1/accounts/" + targetID + "/statuses", query, account, false);

                if (sinceID != null && !sinceID.Equals(""))
                {
                    handleUpdateInput(json, timelineType, account);
                    return null;
                }

                if (!Directory.Exists(Path.GetTempPath() + "Capella\\"))
                    Directory.CreateDirectory(Path.GetTempPath() + "Capella\\");
                timeline = JsonConvert.DeserializeObject(json);
                String cachePath = Path.GetTempPath() + "Capella\\" + timelineType.Replace("/", "_") + "_timeline" + targetID + "_" + account.accessToken + ".json";
                if (timeline != null)
                {
                    if (((Object)timeline).GetType() == typeof(JArray))
                        File.WriteAllText(cachePath, json, UTF8Encoding.UTF8);
                    else
                        return timeline;
                } else {
                    if (File.Exists(cachePath))
                    {
                        timeline = File.ReadAllText(cachePath, UTF8Encoding.UTF8);
                    }
                }
            }
            if (timelineType == "public" && (targetID.Equals("") || targetID == null))
            {
                account.publicTimeline = timeline;
                account.publicTimelineIds = new List<String>();
                foreach (JObject rawToot in timeline.Children())
                {
                    JObject toot = rawToot;
                    if (toot["reblog"] != null && toot["reblog"].Type == JTokenType.Object){
                        toot = (JObject)toot["reblog"];
                    }
                    String tootID = (String)toot["id"];
                    String text = (String)toot["content"];
                    bool mute = false;
                    foreach (String keyword in MastodonAPIWrapper.sharedApiWrapper.keywords)
                    {
                        if (text.ToLower().Contains(keyword.ToLower()))
                        {
                            mute = true;
                        }
                    }
                    if (!mute && !account.publicTimelineIds.Contains(tootID))
                        account.publicTimelineIds.Add(tootID);
                }
                if (publicTimelineChanged != null)
                    publicTimelineChanged(this, "refresh", 0, account);
            }
            if (timelineType == "home" && (targetID.Equals("") || targetID == null))
            {
                account.homeTimeline = timeline;
                account.homeTimelineIds = new List<String>();
                foreach (JObject rawToot in timeline.Children())
                {
                    JObject toot = rawToot;
                    if (toot["reblog"] != null && toot["reblog"].Type == JTokenType.Object)
                    {
                        toot = (JObject)toot["reblog"];
                    }
                    String tootID = (String)toot["id"];
                    String text = (String)toot["content"];
                    bool mute = false;
                    foreach (String keyword in MastodonAPIWrapper.sharedApiWrapper.keywords)
                    {
                        if (text.ToLower().Contains(keyword.ToLower()))
                        {
                            mute = true;
                        }
                    }
                    if (!mute && !account.homeTimelineIds.Contains(tootID))
                        account.homeTimelineIds.Add(tootID);
                }
                if (homeTimelineChanged != null)
                    homeTimelineChanged(this, "refresh", 0, account);
            }
            if (timelineType == "mentions" && (targetID.Equals("") || targetID == null))
            {
                account.mentionsTimeline = timeline;
                account.mentionsTimelineIds = new List<String>();
                foreach (JObject rawToot in timeline.Children())
                {
                    JObject toot = rawToot;
                    String tootID = (String)toot["id"];
                    String text = (String)toot["content"];
                    bool mute = false;
                    foreach (String keyword in MastodonAPIWrapper.sharedApiWrapper.keywords)
                    {
                        if (text.ToLower().Contains(keyword.ToLower()))
                        {
                            mute = true;
                        }
                    }
                    if (!mute && !account.mentionsTimelineIds.Contains(tootID))
                        account.mentionsTimelineIds.Add(tootID);
                }
                if (mentionsTimelineChanged != null)
                    mentionsTimelineChanged(this, "refresh", 0, account);
            }
            return timeline;
        }

        public dynamic getToot(Account account, String tootID)
        {
            String tootID2 = Uri.EscapeUriString(tootID);
            String tootStr = sharedOAuthUtils.GetData("https://" + endpoint + "/api/v1/statuses/"+tootID2, "", account, true);

            dynamic toot = JsonConvert.DeserializeObject(tootStr);
            return toot;
        }

        public dynamic searchUsers(Account account, String query, int count)
        {
            String output = sharedOAuthUtils.GetData("https://" + endpoint + "/api/v1/accounts/search", "q=" + Uri.EscapeUriString(query) + "&limit=" + count, account, true);
            Console.WriteLine(output);
            return JsonConvert.DeserializeObject(output);
        }

        public dynamic getConversation(Account account, String tootID)
        {
            dynamic returnArray = new JArray();

            dynamic toot = getToot(account, tootID);

            String contextStr = sharedOAuthUtils.GetData("https://" + endpoint + "/api/v1/statuses/" + Uri.EscapeUriString(tootID) + "/context", "", account, true);

            dynamic context = JsonConvert.DeserializeObject(contextStr);

            foreach (dynamic ancestorToot in context["ancestors"].Children())
            {
                returnArray.Add(ancestorToot);
            }

            returnArray.Add(toot);

            foreach (dynamic ancestorToot in context["descendants"].Children())
            {
                returnArray.Add(ancestorToot);
            }

            return returnArray;
        }

        public void handleUpdateInput(String streamedData, String timelineType, Account account)
        {
            if (streamedData == null || streamedData.Replace(" ", "") == "")
                return;
            JArray streamedObject = (JArray)JsonConvert.DeserializeObject(streamedData);

            JArray timeline = null; 
            List<String> timelineIds = null; 

            if (timelineType == "public")
            {
                timeline = account.publicTimeline;
                timelineIds = account.publicTimelineIds;
            } else if (timelineType == "home")
            {
                timeline = account.homeTimeline;
                timelineIds = account.homeTimelineIds;
            } else if (timelineType == "mentions")
            {
                timeline = account.mentionsTimeline;
                timelineIds = account.mentionsTimelineIds;
            }

            int added = 0;
            int skipped = 0;
            foreach (dynamic toot in streamedObject.Reverse())
            {
                String id = toot["id"];
                String text = (String)toot["content"];
                bool mute = false;
                foreach (String keyword in MastodonAPIWrapper.sharedApiWrapper.keywords)
                {
                    if (text.ToLower().Contains(keyword.ToLower()))
                    {
                        mute = true;
                    }
                }
                if (mute)
                    continue;

                if (!timelineIds.Contains(id))
                {
                    timelineIds.Insert(0, id);
                    timeline.Insert(0, toot);

                    if (timelineType == "public")
                    {
                        account.publicTimeline = timeline;
                        account.publicTimelineIds = timelineIds;
                        publicTimelineChanged(this, "insert", 0, account);
                    } else if (timelineType == "home")
                    {
                        account.homeTimeline = timeline;
                        account.homeTimelineIds = timelineIds;
                        homeTimelineChanged(this, "insert", 0, account);
                    }
                    else if (timelineType == "mentions")
                    {
                        account.mentionsTimeline = timeline;
                        account.mentionsTimelineIds = timelineIds;

                        //status found
                        MainWindow.sharedMainWindow.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            NotificationsHandler.sharedNotificationsHandler.pushNotification(toot/*, 0*/);
                        }));

                        mentionsTimelineChanged(this, "insert", 0, account);
                    }
                    added++;
                } else
                {
                    skipped++;
                }
            }
            Console.WriteLine("Added " + added + " and skipped " + skipped);
        }

        public String postToot(String tootText, String tootInReplyTo, bool sensitive, bool isPrivate, Account account)
        {
            String tootText2 = Uri.EscapeDataString(tootText);
            String uploadText = "";
            if (tootInReplyTo != "")
                uploadText += "in_reply_to_id=" + sharedOAuthUtils.UrlEncode(tootInReplyTo) + "&";
            if (sensitive)
                uploadText += "sensitive=true&";
            if (isPrivate)
                uploadText += "visibility=private&";
            else
                uploadText += "visibility=public&";
            uploadText += "status=" + tootText2;
            String output = sharedOAuthUtils.PostData("https://" + endpoint + "/api/v1/statuses", uploadText, account, false);
            return output;
        }

        public String postToot(String tootText, String tootInReplyTo, bool sensitive, bool unlisted, String imageIds, Account account)
        {
            String tootText2 = Uri.EscapeDataString(tootText);
            String uploadText = "";
            if (tootInReplyTo != "")
                uploadText += "in_reply_to_id=" + sharedOAuthUtils.UrlEncode(tootInReplyTo) + "&";
            uploadText += "status=" + tootText2;
            if (sensitive)
                uploadText += "sensitive=true&";
            if (unlisted)
                uploadText += "unlisted=true&";
            uploadText += "&media_ids[]=" + imageIds;
            Console.WriteLine(uploadText);
            String output = sharedOAuthUtils.PostData("https://" + endpoint + "/api/v1/statuses", uploadText, account, false);
            return output;
        }

        public dynamic uploadMedia(Account account, String imagePath)
        {
            String output = sharedOAuthUtils.PostData("https://" + endpoint + "/api/v1/media", "", "file", imagePath, account);
            return JsonConvert.DeserializeObject(output);
        }

        public bool deleteToot(String tootID, Account account)
        {
            String tootID2 = Uri.EscapeUriString(tootID);
            String tootStr = sharedOAuthUtils.DeleteData("https://" + endpoint + "/api/v1/statuses/" + tootID2, account);
            dynamic toot = JsonConvert.DeserializeObject(tootStr);
            if (toot["id"] != null)
                return false;
            return true;
        }

        public bool retootToot(String tootID, bool undoRetoot, Account account)
        {
            if (undoRetoot == false)
            {
                String tootID2 = Uri.EscapeUriString(tootID);
                String output = sharedOAuthUtils.PostData("https://" + endpoint + "/api/v1/statuses/" + tootID2 + "/reblog", "", account, false);
                dynamic toot = JsonConvert.DeserializeObject(output);
                bool retooted = (bool)toot.reblog.reblogged;
                return retooted;
            }
            else
            {
                String tootID2 = Uri.EscapeUriString(tootID);
                String output = sharedOAuthUtils.PostData("https://" + endpoint + "/api/v1/statuses/" + tootID2 + "/unreblog", "", account, false);
                dynamic toot = JsonConvert.DeserializeObject(output);
                bool retooted = (bool)toot.reblogged;
                return retooted;
            }
        }

        public dynamic getRelationship(String userID, Account account)
        {
            String userID2 = Uri.EscapeUriString(userID);
            String output = sharedOAuthUtils.GetData("https://" + endpoint + "/api/v1/accounts/relationships", "id=" + userID2, account, false);
            return JsonConvert.DeserializeObject(output);
        }

        public bool followAccount(String userID, Account account, bool follow)
        {
            String userID2 = Uri.EscapeUriString(userID);
            String output;
            if (follow)
            {
                output = sharedOAuthUtils.PostData("https://" + endpoint + "/api/v1/accounts/"+userID2+"/follow", "", account, false);
            } else
            {
                output = sharedOAuthUtils.PostData("https://" + endpoint + "/api/v1/accounts/" + userID2 + "/unfollow", "user_id=" + userID2, account, false);
            }
            dynamic profile = JsonConvert.DeserializeObject(output);
            return (bool)profile["following"];
        }

        public bool favoriteToot(String tootID, bool undoFavorite, Account account)
        {
            String tootID2 = Uri.EscapeUriString(tootID);
            String output;
            if (undoFavorite == true)
                output = sharedOAuthUtils.PostData("https://" + endpoint + "/api/v1/statuses/"+tootID+"/unfavourite", "", account, false);
            else
                output = sharedOAuthUtils.PostData("https://" + endpoint + "/api/v1/statuses/" + tootID + "/favourite", "", account, false);
            dynamic toot = JsonConvert.DeserializeObject(output);
            bool favorited = (bool)toot.favourited;
            return favorited;
        }

        public dynamic followersList(Account account, String userId, int count)
        {
            String output;
            if (userId == null || userId == "")
                userId = account.accountID;
            output = sharedOAuthUtils.GetData("https://" + endpoint + "/api/v1/accounts/" + userId + "/followers", "", account, true);
            return JsonConvert.DeserializeObject(output);
        }

        public bool blockUser(String userID, bool undoBlock, Account account)
        {
            String userID2 = Uri.EscapeUriString(userID);
            String output;
            if (undoBlock == true)
                output = sharedOAuthUtils.PostData("https://" + endpoint + "/api/v1/accounts/"+userID2+"/unblock", "", account, false);
            else
                output = sharedOAuthUtils.PostData("https://" + endpoint + "/api/v1/accounts/" + userID2 + "/block", "", account, false);
            dynamic user = JsonConvert.DeserializeObject(output);
            if (user["id_str"] != null)
            {
                if (undoBlock == false)
                {
                    if (account.listFollowing.Contains((String)user["id_str"]))
                        account.listFollowing.Remove((String)user["id_str"]);
                    if (!account.blockedIDs.Contains((String)user["id_str"]))
                        account.blockedIDs.Add((String)user["id_str"]);
                }
                else
                {
                    if (account.blockedIDs.Contains((String)user["id_str"]))
                        account.blockedIDs.Remove((String)user["id_str"]);
                }
                return !undoBlock;
            }
            return undoBlock;
        }

        public dynamic followingList(Account account, String userId, int count)
        {
            String output;
            if (userId == null || userId == "")
                userId = account.accountID;
            output = sharedOAuthUtils.GetData("https://" + endpoint + "/api/v1/accounts/"+userId+"/following", "", account, true);
            return JsonConvert.DeserializeObject(output);
        }

        public dynamic retootsList(Account account, String tootId, int count)
        {
            String data = sharedOAuthUtils.GetData("https://" + endpoint + "/api/v1/statuses/" + tootId+"/reblogged_by", "", account, true);
            dynamic retootUsers = JsonConvert.DeserializeObject(data);
            return retootUsers;
        }

        public dynamic favoritesList(Account account, String tootId, int count)
        {
            String data = sharedOAuthUtils.GetData("https://" + endpoint + "/api/v1/statuses/" + tootId + "/favourited_by", "", account, true);
            dynamic retootUsers = JsonConvert.DeserializeObject(data);
            return retootUsers;
        }

        public void handleStreamInput(String rawData, Account account, String streamName)
        {
            dynamic streamData = JsonConvert.DeserializeObject(rawData);
            String messageType = streamData["event"];
            if (messageType.Equals("notification"))
            {
                if (streamName != "user")
                    return;
                dynamic notification = JsonConvert.DeserializeObject((String)streamData["payload"]);

                MainWindow.sharedMainWindow.Dispatcher.BeginInvoke(new Action(() =>
                {
                    NotificationsHandler.sharedNotificationsHandler.pushNotification(notification);
                }));

                String notificationType = notification["type"];
                if (notificationType.Equals("mention"))
                {
                    dynamic toot = notification["status"];

                    String id = toot["id"];
                    String text = (String)toot["content"];
                    bool mute = false;
                    foreach (String keyword in MastodonAPIWrapper.sharedApiWrapper.keywords)
                    {
                        if (text.ToLower().Contains(keyword.ToLower()))
                        {
                            mute = true;
                        }
                    }
                    if (mute)
                        return;

                    if (!account.mentionsTimelineIds.Contains(id))
                    {
                        account.mentionsTimelineIds.Insert(0, id);
                        account.mentionsTimeline.Insert(0, toot);

                        mentionsTimelineChanged(this, "insert", 0, account);
                    }
                }
            }
            else if (messageType.Equals("update"))
            {
                dynamic toot = JsonConvert.DeserializeObject((String)streamData["payload"]);

                String id = toot["id"];
                String text = (String)toot["content"];
                bool mute = false;
                foreach (String keyword in this.keywords)
                {
                    if (text.ToLower().Contains(keyword.ToLower()))
                    {
                        mute = true;
                    }
                }
                if (mute)
                    return;

                JArray timeline = null;
                List<String> timelineIds = null;

                if (streamName == "user")
                {
                    timeline = account.homeTimeline;
                    timelineIds = account.homeTimelineIds;
                } else if (streamName == "public")
                {
                    timeline = account.publicTimeline;
                    timelineIds = account.publicTimelineIds;
                }

                if (timeline == null || timelineIds == null)
                    return;

                if (!timelineIds.Contains(id))
                {
                    timelineIds.Insert(0, id);
                    timeline.Insert(0, toot);

                    if (streamName == "user")
                    {
                        account.homeTimeline = timeline;
                        account.homeTimelineIds = timelineIds;
                        homeTimelineChanged(this, "insert", 0, account);
                    }
                    else if (streamName == "public")
                    {
                        account.publicTimeline = timeline;
                        account.publicTimelineIds = timelineIds;
                        publicTimelineChanged(this, "insert", 0, account);
                    }
                }
            }
            else if (messageType.Equals("delete"))
            {
                string id = streamData["payload"];

                int indexToDelete = account.publicTimelineIds.IndexOf(id);
                if (indexToDelete >= 0)
                {
                    account.publicTimelineIds.RemoveAt(indexToDelete);
                    account.publicTimeline.RemoveAt(indexToDelete);
                    if (publicTimelineChanged != null)
                        publicTimelineChanged(this, "delete", indexToDelete, account);
                }

                indexToDelete = account.mentionsTimelineIds.IndexOf(id);
                if (indexToDelete >= 0)
                {
                    account.mentionsTimelineIds.RemoveAt(indexToDelete);
                    account.mentionsTimeline.RemoveAt(indexToDelete);
                    if (mentionsTimelineChanged != null)
                        mentionsTimelineChanged(this, "delete", indexToDelete, account);
                }

                if (account.homeTimeline == null || account.homeTimelineIds == null)
                    return;

                indexToDelete = account.homeTimelineIds.IndexOf(id);
                if (indexToDelete >= 0)
                {
                    account.homeTimelineIds.RemoveAt(indexToDelete);
                    account.homeTimeline.RemoveAt(indexToDelete);
                    if (homeTimelineChanged != null)
                        homeTimelineChanged(this, "delete", indexToDelete, account);
                }
            }
        }

        public void startStreaming(Account account)
        {
            setupStreaming(account, "user");
            setupStreaming(account, "public");
        }

        public void setupStreaming(Account account, String streamName)
        {
            String wsURL = "wss://" + endpoint + "/api/v1/streaming/?access_token="+account.accessToken+"&stream="+ streamName;

            WebSocket socket = new WebSocket(wsURL);
            socket.Origin = "https://" + endpoint;
            socket.SslConfiguration.EnabledSslProtocols = SslProtocols.Tls12;
            if (App.isDebugEnabled)
                socket.Log.Level = LogLevel.Warn;
            else
                socket.Log.Level = LogLevel.Error;
            socket.WaitTime = TimeSpan.MaxValue;

            socket.OnOpen += (sender, e) =>
            {
                Console.WriteLine("Opened");
            };

            socket.OnMessage += (sender, e) =>
            {
                if (streamName == "public")
                    Console.WriteLine("Streamed Data" + e.Data);
                handleStreamInput(e.Data, account, streamName);
            };

            socket.OnClose += (sender, e) =>
            {
                Console.WriteLine("Disconnected");
            };
            
            socket.Connect();
            Console.WriteLine("Is secure? " + socket.IsSecure);
            Console.WriteLine("Connected");
        }

        public void updateAccount(Account account)
        {
            if (account.publicTimeline != null)
            {
                if (account.publicTimelineIds.Count < 1)
                    return;
                String lastId = account.publicTimelineIds[0];
                getTimeline(account, "public", "", lastId);
            }
            if (account.homeTimeline != null)
            {
                if (account.homeTimelineIds.Count < 1)
                    return;
                String lastId = account.homeTimelineIds[0];
                getTimeline(account, "home", "", lastId);
            }
            if (account.mentionsTimeline != null)
            {
                if (account.mentionsTimelineIds.Count < 1)
                    return;
                String lastId = account.mentionsTimelineIds[0];
                getTimeline(account, "mentions", "", lastId);
            }
        }

        public String consumerKey;
        public String consumerSecret;
        public static MastodonAPIWrapper sharedApiWrapper;
    }
}
