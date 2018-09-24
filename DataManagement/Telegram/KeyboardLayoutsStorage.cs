using Telegram.Bot.Types.ReplyMarkups;

namespace TheGateQuest.DataManagement.Telegram
{
    public static class KeyboardLayoutsStorage
    {
        public static InlineKeyboardMarkup GetHintReplyMarkup()
        => new[]
        {
            InlineKeyboardButton.WithCallbackData("Нова підказка до поточної локації", $"Action: askHint"),
            InlineKeyboardButton.WithCallbackData("Повторити підказки до поточної локації", $"Action: replayHint")
        };

        public static InlineKeyboardMarkup GetAnswerVerificationReplyMarkup(long chatId, int messageId, int locationId)
        => new[]
        {
            InlineKeyboardButton.WithCallbackData("+", $"Verification: true {chatId} {messageId} {locationId}"),
            InlineKeyboardButton.WithCallbackData("-", $"Verification: false {chatId} {messageId} {locationId}")
        };

        public static InlineKeyboardMarkup GetAdminActionReplyMarkup()
        => new[]
        {
            InlineKeyboardButton.WithCallbackData("Статистика команд", "Stats")
        };
        public static ReplyKeyboardRemove GetDefaultKeyboard()
        => new ReplyKeyboardRemove();

        public static ReplyKeyboardMarkup GetRequestContactReplyMarkup()
        {
            var replyKbdMarkup = new ReplyKeyboardMarkup(
            new[]
            {
                KeyboardButton.WithRequestContact("Перевірити номер телефону")
            }, oneTimeKeyboard: true);
            replyKbdMarkup.OneTimeKeyboard = true;
            return replyKbdMarkup;
        }
    }
}
