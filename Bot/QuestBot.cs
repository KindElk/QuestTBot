using System;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TheGateQuest.DataManagement.HintManagement;
using TheGateQuest.DataManagement.Telegram;

namespace TheGateQuest.Bot
{
    public class QuestBot : Telegram.Bot.TelegramBotClient
    {
        public QuestBot(string token) : base(token)
        {
            _questMasterChatId = 0;
        }

        private long? _questMasterChatId;
        private string _masterUserName;
        private DataManager _dataManager;

        public void Start(DataManager manager, string adminUserName)
        {
            _questMasterChatId = null;
            _dataManager = manager;
            _masterUserName = adminUserName;
            var me = GetMeAsync().Result;

            OnMessage += Bot_OnMessage;
            OnCallbackQuery += Bot_OnCallbackQuery;

            StartReceiving(Array.Empty<UpdateType>());
            Console.WriteLine($"Start listening for @{me.Username}");
        }

        public void Shutdown()
        {
            StopReceiving();
        }

        private async void Bot_OnMessage(object sender, MessageEventArgs messageEventArgs)
        {
            System.IO.File.WriteAllText("dataManager.json", JsonConvert.SerializeObject(_dataManager, Formatting.Indented));
            var chatId = messageEventArgs.Message.Chat.Id;
            if (chatId == _questMasterChatId)
                //return
                ;//here we should return later;

            if (_dataManager.IsTeamFinishedForUser(chatId))
            {
#pragma warning disable CS4014
                SendTextMessageAsync(chatId, "Ви вже закінчили квест.");
#pragma warning restore CS4014
                return;
            }

            var userName = GetChatAsync(chatId).Result.Username;

            if (null == _questMasterChatId && userName == _masterUserName)
            {
                _questMasterChatId = chatId;
#pragma warning disable CS4014
                SendTextMessageAsync(chatId, "Вітаю, quest master!");
                SendAdminActionMessageAsync(chatId,
                    "Щоб отримати статистику по кількості підказок для кожної команди, натисніть кнопку");//here we should return later;
#pragma warning restore CS4014
            }

                var teamName = _dataManager.GetTeamNameForUser(chatId);
            
            if (String.IsNullOrEmpty(teamName) && MessageType.Contact != messageEventArgs.Message.Type)
            {
                _OnFirstContact(messageEventArgs);
                return;
            }

            switch (messageEventArgs.Message.Type)
            {
                case MessageType.Photo:
                    _OnPhotoSent(messageEventArgs, teamName);
                    break;
                case MessageType.Contact:
                    _OnContactSent(messageEventArgs, ref teamName);
                    break;
                default:
                    Console.WriteLine($"User {userName} sent message of unsupported type "
                        + $"'{messageEventArgs.Message.Type.ToString()}'.");
                    break;
            }
        }

        private void _OnFirstContact(MessageEventArgs messageEventArgs)
        {
#pragma warning disable CS4014
            SendMessageWithSendContactButtons(messageEventArgs.Message.Chat.Id,
                "Вітаю! Для визначення команди надішліть, будь ласка, ваші контактні дані.");
#pragma warning restore CS4014
        }

        private void _OnPhotoSent(MessageEventArgs messageEventArgs, string teamName)
        {
            var chat = messageEventArgs.Message.Chat;
            var chatId = chat.Id;
            var firstName = chat.FirstName;
            var lastName = chat.LastName;
            var messageId = messageEventArgs.Message.MessageId;
            var currentLocation = _dataManager.GetLocationNameFor(chatId);
            var currentLocationIndex = _dataManager.GetLocationIndexFor(chatId);

            ForwardMessageAsync(_questMasterChatId, chatId, messageEventArgs.Message.MessageId).Wait();
#pragma warning disable CS4014
            SendTextMessageAsync(_questMasterChatId,
                $"{firstName} {lastName} ({teamName}) надсилає фото до загадки для {currentLocation}",
                replyMarkup: KeyboardLayoutsStorage.GetAnswerVerificationReplyMarkup(chatId, messageId, currentLocationIndex));
            SendTextMessageAsync(chatId, "Відповідь прийнято.");
#pragma warning restore CS4014
        }

        private void _OnContactSent(MessageEventArgs messageEventArgs, ref string teamName)
        {
            if (!String.IsNullOrEmpty(teamName))
                return;

            var chatId = messageEventArgs.Message.Chat.Id;
            var phone = messageEventArgs.Message.Contact?.PhoneNumber ?? string.Empty;

            teamName = _dataManager.GetTeamNameForUser(chatId, phone);

#pragma warning disable CS4014
            if (!String.IsNullOrEmpty(teamName))
            {
                SendMessageWithHintButtons(chatId,
                    $"Вітаю, ви з команди {teamName}! " +
                    "Для того, щоб отримати підказку, натисніть кнопку знизу.");
            }
            else
            {
                SendMessageWithSendContactButtons(chatId,
                    "Не можу визначити вашу команду. Будь ласка, " +
                    "перевірте, чи ви зареєстровані за цим номером.");
            }
#pragma warning restore CS4014
        }

        private async void Bot_OnCallbackQuery(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            var chatId = callbackQueryEventArgs.CallbackQuery.Message.Chat.Id;
            if (_dataManager.IsTeamFinishedForUser(chatId))
            {
#pragma warning disable CS4014
                SendTextMessageAsync(chatId, "Ви вже закінчили квест.");
#pragma warning restore CS4014
                return;
            }
            var callbackData = callbackQueryEventArgs.CallbackQuery.Data.Split(' ');

            switch (callbackData[0])
            {
                case "Action:":
                    _OnCallbackAction(callbackQueryEventArgs, callbackData);
                    break;
                case "Verification:":
                    _OnCallbackVerification(callbackQueryEventArgs, callbackData);
                    break;
                case "Stats":
                    _OnStatsRequest();
                    break;
                default:
                    Console.WriteLine("Unhandled callback message.");
                    break;
            }
            System.IO.File.WriteAllText("dataManager.json", JsonConvert.SerializeObject(_dataManager, Formatting.Indented));
        }

        private void _OnStatsRequest()
        {
            var hintStats = string.Join("\n",_dataManager.GetHintStats());
            SendTextMessageAsync(_questMasterChatId, hintStats,
                    replyMarkup: KeyboardLayoutsStorage.GetDefaultKeyboard());
        }

        private void _OnCallbackAction(CallbackQueryEventArgs callbackQueryEventArgs, string[] callbackData)
        {
            var chatId = callbackQueryEventArgs.CallbackQuery.Message.Chat.Id;
            switch (callbackData[1])
            {
                case "askHint":
                    var hint = _dataManager.AskNewHintFor(chatId);
                    SendMessageWithHintButtons(chatId, hint);
                    break;
                case "replayHint":
                    var allHints = _dataManager.GetOldHintsFor(chatId);
                    SendMessageWithHintButtons(chatId, allHints);
                    break;
                default:
                    Console.WriteLine("Unhandled Action type." + callbackData[1]);
                    break;
            }
        }

        private void _OnCallbackVerification(CallbackQueryEventArgs callbackQueryEventArgs, string[] callbackData)
        {
            var isAnswerCorrect = bool.Parse(callbackData[1]);
            var teamChatId = long.Parse(callbackData[2]);
            var messageId = int.Parse(callbackData[3]);
            var locationIndex = int.Parse(callbackData[4]);

            if (_dataManager.GetLocationIndexFor(teamChatId) != locationIndex)
            {
                Console.WriteLine("Reaction for previous location.");
                return;
            }

            SendTextMessageAsync(teamChatId, replyToMessageId: messageId,
                text: isAnswerCorrect ? "Об'єкт вірний" : "Об'єкт невірний");

            if (isAnswerCorrect)
            {
                var newTask = _dataManager.UpdateTeamProgress(teamChatId);
                if (newTask.actionsAvailable)
                    SendMessageWithHintButtons(teamChatId, newTask.text);
                else
                    SendTextMessageAsync(teamChatId, newTask.text);
            }
        }

        private Task<Message> SendMessageWithHintButtons(long chatId, string message)
            => SendTextMessageAsync(chatId, message,
                    replyMarkup: KeyboardLayoutsStorage.GetHintReplyMarkup());

        private Task<Message> SendMessageWithSendContactButtons(long chatId, string message)
            => SendTextMessageAsync(chatId, message,
                    replyMarkup: KeyboardLayoutsStorage.GetRequestContactReplyMarkup());

        private Task<Message> SendAdminActionMessageAsync(long chatId, string message)
            => SendTextMessageAsync(chatId, message,
                replyMarkup: KeyboardLayoutsStorage.GetAdminActionReplyMarkup());
    }
}
