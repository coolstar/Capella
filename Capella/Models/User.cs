using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capella.Models
{
    /// <summary>
    /// Represents a person
    /// </summary>
    public class User
    {
        [JsonProperty("acct")]
        public string screenName;

    }
}
