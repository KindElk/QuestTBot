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
        private static readonly string backupFile = "dataManager.json";

        private static DataManager _CreateDataManager()
        {
            if (File.Exists(backupFile))
            {
                Console.WriteLine("Trying restore from backup...");
                try
                {
                    return JsonConvert.DeserializeObject<DataManager>(File.ReadAllText(backupFile));
                }
                catch (Exception e)
                {
                    Console.WriteLine("Restoring from backup failed. Trying to start normaly...");
                }
            }

            Console.WriteLine("Normal cold start.");

            string hintsJson = "hints.json";
            string teamsJson = "teams.json";

            Hints hints = JsonConvert.DeserializeObject<Hints>(File.ReadAllText(hintsJson));
            QuestTeamsMap questTeams = JsonConvert.DeserializeObject<QuestTeamsMap>(File.ReadAllText(teamsJson));
            Console.WriteLine(JsonConvert.SerializeObject(questTeams));
            return new DataManager(questTeams, hints);
        }

        private static string _ReadAdminUserName(bool fromFile)
        {
            bool correctAdmin = fromFile && File.Exists("admin.txt");
            string adminUserName = fromFile ? File.ReadAllText("admin.txt") : "";
            while (!correctAdmin)
            {
                Console.Write("Please, enter admin username: ");
                adminUserName = Console.ReadLine();
                Console.WriteLine($"You've entered '{adminUserName}'. Is that correct admin username? (+/-)");
                correctAdmin = Console.ReadLine().Equals("+");
            }
            return adminUserName;
        }

        static void Main(string[] args)
        {
            var dataManagement = _CreateDataManager();
            var adminUserName = _ReadAdminUserName(true);

            try
            {
                Bot.Start(dataManagement, adminUserName);
                Console.ReadLine();
                Bot.Shutdown();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n\n" + e.StackTrace);
                Console.ReadLine();
            }
            File.WriteAllText(backupFile, JsonConvert.SerializeObject(dataManagement, Formatting.Indented));
        }
    }
}
