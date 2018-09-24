using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using TheGateQuest.DataModels.Quest;

namespace TheGateQuest.DataManagement.HintManagement
{
    using System;
    using ChatID = System.Int64;
    using TeamID = System.Int32;

    public class DataManager
    {
        private static readonly string _internalError =
            "500: Internal server error. " +
            "Будь ласка, зверніться до організаторів. " +
            "Деталі помилки: DataManager. ";

        [JsonProperty("teamsData")]
        private QuestTeamsMap _data;
        [JsonProperty("chatToTeamMapping")]
        private Dictionary<ChatID, TeamID> _chatToTeamMapping;
        [JsonProperty("hintsData")]
        private Hints _hints;

        public DataManager(QuestTeamsMap map, Hints hints)
        {
            _data = map;
            _hints = hints;
            _chatToTeamMapping = new Dictionary<ChatID, TeamID>();
        }

        public bool IsTeamFinishedForUser(ChatID chatId)
        {
            var team = _GetTeamForUser(chatId);
            if (null == team)
                return false;

            return team.CurrentLocationIndex >= team.Route.Count;
        }

        ///<summary>
        ///returns first unseen hint for current location as string.
        ///If all hints are taken, returns formatted combo of all hints for current location
        ///</summary>
        ///<param name="chatId">Used to indicate name of the team that asks for hint.</param>
        public string AskNewHintFor(ChatID chatId)
        {
            var team = _GetTeamForUser(chatId);
            var locationHint = _GetLocationHintFor(team);
            if (null == team || null == locationHint)
                return _internalError + "Немає підходящої підказки або команда невизначена.";

            var hintIndex = team.Route[team.CurrentLocationIndex].HintsCounter;
            if (hintIndex < locationHint.Hints.Count - 1) //hints also include main question
            {
                ++team.Route[team.CurrentLocationIndex].HintsCounter;
                ++locationHint.HintsTaken;
                return locationHint.Hints[hintIndex + 1];
            }
            else
                return GetOldHintsFor(chatId);
        }

        ///<summary>
        ///returns all hints given before for current location as string.
        ///If all hints are taken, returns formatted combo of all hints for current location
        ///</summary>
        ///<param name="chatId">Used to indicate name of the team that asks for hint.</param>
        public string GetOldHintsFor(ChatID chatId, Team team = null, LocationHint locationHint = null, int? hintIndex = null)
        {
            team = team ?? _GetTeamForUser(chatId);
            locationHint = locationHint ?? _GetLocationHintFor(team);
            if (null == team || null == locationHint)
                return _internalError + "Немає підходящої підказки або команда невизначена.";

            hintIndex = hintIndex ?? team.Route[team.CurrentLocationIndex].HintsCounter;
            string hintsCombo = "Все, що маю для вас:";
            for (int i = 0; i < locationHint.Hints.Count && i <= hintIndex.Value; ++i)
            {
                hintsCombo += $"\n#{i}: {locationHint.Hints[i]}";
            }
            return hintsCombo;
        }

        ///<summary>
        ///returns name of the team that user belongs to
        ///</summary>
        public string GetTeamNameForUser(ChatID chatId, string phoneNumber = "") 
            => _GetTeamForUser(chatId, phoneNumber)?.Name;

        private Team _GetTeamForUser(ChatID chatId, string phoneNumber = "")
        {
            if (_chatToTeamMapping.ContainsKey(chatId))
                return _data.Teams[_chatToTeamMapping[chatId]];

            if (string.IsNullOrEmpty(phoneNumber))
                return null;
            phoneNumber = Team.PreprocessPhoneNumber(phoneNumber);

            var userTeam = _data.Teams?.SingleOrDefault(team => team.Members.Contains(phoneNumber));
            if (userTeam != null)
                _chatToTeamMapping.Add(chatId, _data.Teams.IndexOf(userTeam));
            return userTeam;
        }

        ///<summary>
        ///Returns current location for the team of specified user
        ///</summary>
        ///<param name = "chatId" > Used to indicate user whose location needs to be identified.</param>
        public string GetLocationNameFor(ChatID chatId)
            => _GetLocationHintFor(_GetTeamForUser(chatId))?.LocationName;

        public int GetLocationIdFor(ChatID chatId)
            => _GetLocationIdFor(_GetTeamForUser(chatId));

        public int GetLocationIndexFor(ChatID chatId)
            => _GetTeamForUser(chatId)?.CurrentLocationIndex ?? -1;

        private int _GetLocationIdFor(Team team)
            => team?.Route[team.CurrentLocationIndex].Id ?? -1;

        private LocationHint _GetLocationHintFor(Team team)
            => _hints.Locations.SingleOrDefault(loc => loc.Id == _GetLocationIdFor(team));

        /// <summary>
        /// Updates data regarding current location of specified team
        /// </summary>
        /// <param name="chatId"> Used to specify which team stats should be updated</param>
        /// <returns></returns>
        public (string text, bool actionsAvailable) UpdateTeamProgress(ChatID chatId)
        {
            var team = _GetTeamForUser(chatId);
            if (null == team)
                return (_internalError + "Rоманда невизначена.", false);

            if (++team.CurrentLocationIndex == team.Route.Count)
                return ("Вітаю! Ви пройшли квест!", false);

            var locationHint = _GetLocationHintFor(team);
            if (null != locationHint)
                return (locationHint.Hints[0], true);

            return (_internalError + "Локацію не знайдено.", false);
        }

        public List<string> GetHintStats()
            => _data.Teams
            .Select(team => $"команда {team.Name} всього взяла {team.Route.Sum(loc => loc.HintsCounter)} підказок")
            .ToList();
    }
}
