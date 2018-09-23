using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TheGateQuest.DataModels.Quest
{
    using Phone = System.String;
    public class Team
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("route")]
        public List<Location> Route { get; set; }

        [JsonProperty("currentLocationIndex")]
        public int CurrentLocationIndex { get; set; }

        [JsonProperty("members")]
        public List<Phone> Members { get; set; }

        public Team()
        {
            CurrentLocationIndex = 0;
        }

        [OnDeserialized]
        void OnDeserialised(StreamingContext context)
        {
            for (int i = 0; i < Members.Count; ++i)
            {
                Members[i] = Members[i].Replace("+", "");
                Members[i] = Members[i].Replace("-", "");
                Members[i] = Members[i].Replace("(", "");
                Members[i] = Members[i].Replace(")", "");
                Members[i] = Members[i].Replace(" ", "");
                if (!string.IsNullOrEmpty(Members[i]))
                    Members[i] = "+" + Members[i];
            }
        }
    }
}
