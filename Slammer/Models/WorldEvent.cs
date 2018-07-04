using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Slammer.Models
{
    public enum EventType {
        Unstable = 1,
        Blood = 2,
        Volan = 3,
        Other = 4
    }

    public enum Contribute {
        Zero = 0,
        Stones = 2,
        Weekly = 4
    }

    public class worldEventResponse
    {
        public string status;
        public List<WorldEvent> data;
    }

    public class WorldEvent
    {
        public static Dictionary<string, Contribute> zoneContributions = new Dictionary<string, Contribute>()
        {
            {"Shimmersand", Contribute.Zero },
            {"Moonshade Highlands",Contribute.Zero },
            {"Droughtlands",Contribute.Zero },
            {"Ember Isle",Contribute.Zero },
            {"Iron Pine Peak",Contribute.Zero },
            {"Scarlet Gorge",Contribute.Zero },
            {"Stillmoor",Contribute.Zero },
            {"Silverwood",Contribute.Zero },
            {"Freemarch",Contribute.Zero },
            {"Stonefield",Contribute.Zero },
            {"Scarwood Reach",Contribute.Zero },
            {"Gloamwood",Contribute.Zero },
            {"Cape Jule",Contribute.Zero },
            {"Kingdom of Pelladane",Contribute.Zero },
            {"Seratos",Contribute.Zero },
            {"City Core",Contribute.Zero },
            {"Steppes of Infinity",Contribute.Zero },
            {"Eastern Holdings",Contribute.Zero },
            {"Ashora",Contribute.Zero },
            {"Morban",Contribute.Zero },
            {"Ardent Domain",Contribute.Zero },
            {"Kingsward",Contribute.Zero },
            {"The Dendrome",Contribute.Zero },
            {"Goboro Reef",Contribute.Weekly },
            {"Draumheim",Contribute.Weekly },
            {"Tarken Glacier",Contribute.Weekly }
        };

        public string zone;
        public string shard;
        public string name;
        public long started;
        public string timeLeft;
        public string id { get {
            string hash = "";
            using (MD5 md5 = MD5.Create())
            {
                byte[] d = md5.ComputeHash(Encoding.UTF8.GetBytes(started.ToString() + shard + zone));
                hash = BitConverter.ToString(d);
            }
            return hash;
        }
        }
        public EventType eventType { 
            get {
                if (name.Contains("Unstable"))
                {
                    return EventType.Unstable;
                }
                if (name.StartsWith("Bloodfire"))
                {
                    return EventType.Blood;
                }
                if (name.StartsWith("Dreams of"))
                {
                    return EventType.Volan;
                }
                return EventType.Other; 
            } 
        }
        public Contribute contributions
        {
            get
            {
                return zoneContributions[zone];
            }
        }

    }


}
