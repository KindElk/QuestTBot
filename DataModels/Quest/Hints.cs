using Newtonsoft.Json;
using System.Collections.Generic;

namespace TheGateQuest.DataModels.Quest
{
    public class Hints
    {
        [JsonProperty("locations")]
        public List<LocationHint> Locations;
    }
}
