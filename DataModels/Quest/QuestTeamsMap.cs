using Newtonsoft.Json;
using System.Collections.Generic;

namespace TheGateQuest.DataModels.Quest
{
    public class QuestTeamsMap
    {
        [JsonProperty("teams")]
        public List<Team> Teams { get; set; }
    }
}
