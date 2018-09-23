using Newtonsoft.Json;
using System;
using System.IO;
using TheGateQuest.DataManagement.HintManagement;
using TheGateQuest.DataModels.Quest;

namespace TheGateQuest.Bot
{
    class Program
    {
        private static readonly QuestBot Bot = new QuestBot("642277495:AAHhrrFbKWWOIC8Zd9eWaGFOq0mMbuQboxw");
        
        static void Main(string[] args)
        {
            string hintsJson = "hints.json";
            string teamsJson = "teams.json";
            
            Hints hints = JsonConvert.DeserializeObject<Hints>(File.ReadAllText(hintsJson));
            QuestTeamsMap questTeams = JsonConvert.DeserializeObject<QuestTeamsMap>(File.ReadAllText(teamsJson));
            Console.WriteLine(JsonConvert.SerializeObject(questTeams));
            DataManager dataManagement = new DataManager(questTeams, hints);

            bool correctAdmin = true;//false;
            string adminUserName = File.ReadAllText("admin.txt"); 
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
