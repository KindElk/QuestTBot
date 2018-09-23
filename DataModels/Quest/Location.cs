using Newtonsoft.Json;

namespace TheGateQuest.DataModels.Quest
{
    public class Location
    {
        [JsonProperty("id")]
        public int Id { get; set; } //FK

        [JsonProperty("hintsCounter")]
        public int HintsCounter { get; set; }

        public Location()
        {
            HintsCounter = 0;
        }
    }
}
