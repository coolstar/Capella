using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Capella.Models
{
    /// <summary>
    /// Logged in User account.
    /// </summary>
    public class Account
    {
        public String accessToken;
        public String endpoint;
        public String accountID = null;
        public String myHandle = null;
        public JArray publicTimeline;
        public JArray homeTimeline;
        public JArray mentionsTimeline;
        public JArray userTimeline;
        public List<String> publicTimelineIds = new List<String>();
        public List<String> homeTimelineIds = new List<String>();
        public List<String> mentionsTimelineIds = new List<String>();
        public List<String> userTimelineIds = new List<String>();
        public List<String> blockedIDs = new List<String>();
        public List<String> listFollowing = null;
    }
}
