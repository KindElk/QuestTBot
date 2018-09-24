using Newtonsoft.Json;
using System.Collections.Generic;

namespace TheGateQuest.DataModels.Quest
{
    public class LocationHint
    {
        [JsonProperty("name")]
        public string LocationName;

        [JsonProperty("id")]
        public int Id;

        [JsonProperty("hints")]
        public List<string> Hints;

        [JsonProperty("hintsTaken")]
        public int HintsTaken;

        public LocationHint()
        {
            HintsTaken = 0;
        }
    }
}
