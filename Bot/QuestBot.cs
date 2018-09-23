using System;
using Telegram.Bot.Args;
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
            var chatId = messageEventArgs.Message.Chat.Id;
            if (chatId == _questMasterChatId)
                ;//here we should return later;

            Console.WriteLine(messageEventArgs.Message.Text);

            var userName = GetChatAsync(chatId).Result.Username;
            var phone = messageEventArgs.Message.Contact?.PhoneNumber ?? string.Empty;

            if (null == _questMasterChatId && userName == _masterUserName)
            {
                _questMasterChatId = chatId;
#pragma warning disable CS4014
                SendTextMessageAsync(messageEventArgs.Message.Chat.Id, "Вітаю, quest master!");
#pragma warning restore CS4014
                ;//here we should return later;
            }

            var teamName = _dataManager.GetTeamNameForUser(chatId);
            
            if (String.IsNullOrEmpty(teamName))
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
            SendTextMessageAsync(messageEventArgs.Message.Chat.Id,
                "Вітаю! Для визначення команди надішліть, будь ласка, ваші контактні дані.",
                replyMarkup: KeyboardLayoutsStorage.GetRequestContactReplyMarkup());
#pragma warning restore CS4014
        }

        private void _OnPhotoSent(MessageEventArgs messageEventArgs, string teamName)
        {
            var chatId = messageEventArgs.Message.Chat.Id;
            var userName = GetChatAsync(chatId).Result.Username;
            var messageId = messageEventArgs.Message.MessageId;
            var currentLocation = _dataManager.GetLocationNameFor(chatId);
            var currentLocationIndex = _dataManager.GetLocationIndexFor(chatId);

            ForwardMessageAsync(_questMasterChatId, chatId, messageEventArgs.Message.MessageId).Wait();
#pragma warning disable CS4014
            SendTextMessageAsync(_questMasterChatId,
                $"{userName} ({teamName}) надсилає фото до загадки для {currentLocation}",
                replyMarkup: KeyboardLayoutsStorage.GetAnswerVerificationReplyMarkup(chatId, messageId, currentLocationIndex));
            SendTextMessageAsync(chatId, "Відповідь прийнято.");
#pragma warning restore CS4014
        }

        private void _OnContactSent(MessageEventArgs messageEventArgs, ref string teamName)
        {
            if (!String.IsNullOrEmpty(teamName))
                return;

            var chatId = messageEventArgs.Message.Chat.Id;
            var userName = GetChatAsync(chatId).Result.Username;
            var phone = messageEventArgs.Message.Contact?.PhoneNumber ?? string.Empty;

            teamName = _dataManager.GetTeamNameForUser(chatId, phone);

#pragma warning disable CS4014
            if (!String.IsNullOrEmpty(teamName))
            {
                SendTextMessageAsync(chatId,
                    $"Вітаю, {userName} з команди {teamName}! " +
                    $"Для того, щоб отримати підказку, натисніть кнопку знизу.",
                    replyMarkup: KeyboardLayoutsStorage.GetActionSelectReplyMarkup());
            }
            else
            {
                SendTextMessageAsync(chatId,
                    "Не можу визначити вашу команду. Будь ласка, " +
                    "перевірте, чи ви зареєстровані за цим номером.",
                    replyMarkup: KeyboardLayoutsStorage.GetRequestContactReplyMarkup());
            }
#pragma warning restore CS4014
        }

        private async void Bot_OnCallbackQuery(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            var chatId = callbackQueryEventArgs.CallbackQuery.Message.Chat.Id;
            var callbackData = callbackQueryEventArgs.CallbackQuery.Data.Split(' ');

            switch (callbackData[0])
            {
                case "Action:":
                    _OnCallbackAction(callbackQueryEventArgs, callbackData);
                    break;
                case "Verification:":
                    _OnCallbackVerification(callbackQueryEventArgs, callbackData);
                    break;
                default:
                    Console.WriteLine("Unhandled callback message.");
                    break;
            }
        }

        private void _OnCallbackAction(CallbackQueryEventArgs callbackQueryEventArgs, string[] callbackData)
        {
            var chatId = callbackQueryEventArgs.CallbackQuery.Message.Chat.Id;
            switch (callbackData[1])
            {
                case "askHint":
                    var hint = _dataManager.AskHintFor(chatId);
                    SendTextMessageAsync(chatId, hint);
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
                SendTextMessageAsync(teamChatId, newTask);
            }
        }
    }
}
