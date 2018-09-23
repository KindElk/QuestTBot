using Newtonsoft.Json;
using System;
using TheGateQuest.DataManagement.HintManagement;
using TheGateQuest.DataModels.Quest;

namespace TheGateQuest.Bot
{
    class Program
    {
        private static readonly QuestBot Bot = new QuestBot("642277495:AAHhrrFbKWWOIC8Zd9eWaGFOq0mMbuQboxw");
        
        static void Main(string[] args)
        {
            string hintsJson =
            #region hintsJson
@"
{
  'locations':[
    {
      'name':'locOne',
      'id': 1,
      'hints':[
        'locOne question',
        'locOne hint1',
        'locOne hint2'
      ]
    },
    {
      'name':'locTwo',
      'id': 0,
      'hints':[
        'locTwo question',
        'locTwo hint1',
        'locTwo hint2'
      ]
    }
  ]
}
";
            #endregion
            string teamsJson =
            #region teamsJson
@"
{
  'teams':[
    {
      'name' : 'team1',
      'id' : 0,
      'route':[
        {'id': 0}, {'id': 1}
      ],
      'members':[
        '',
        ''
      ]
    },
    {
      'name' : '2team',
      'id' : 1,
      'route':[
        {'id': 1}, {'id': 0}
      ],
      'members':[
        ''
      ]
    }
  ]
}
";
            #endregion

            Hints hints = JsonConvert.DeserializeObject<Hints>(hintsJson);
            Console.WriteLine(JsonConvert.SerializeObject(hints));
            QuestTeamsMap questTeams = JsonConvert.DeserializeObject<QuestTeamsMap>(teamsJson);
            Console.WriteLine(JsonConvert.SerializeObject(questTeams));
            DataManager dataManagement = new DataManager(questTeams, hints);

            bool correctAdmin = true;//false;
            string adminUserName = ""; 
            while (!correctAdmin)
            {
                Console.Write("Please, enter admin username: ");
                adminUserName = Console.ReadLine();
                Console.WriteLine($"You've entered '{adminUserName}'. Is that correct admin username? (+/-)");
                correctAdmin = Console.ReadLine().Equals("+");
            }

            Bot.Start(dataManagement, adminUserName);
            Console.ReadLine();
            Bot.Shutdown();
        }
    }
}
