using Telegram.Bot.Types.ReplyMarkups;

namespace TheGateQuest.DataManagement.Telegram
{
    public static class KeyboardLayoutsStorage
    {
        public static InlineKeyboardMarkup GetActionSelectReplyMarkup()
        => new[]
        {
            InlineKeyboardButton.WithCallbackData("Підказка до поточної локації", $"Action: askHint")
        };

        public static InlineKeyboardMarkup GetAnswerVerificationReplyMarkup(long chatId, int messageId, int locationId)
        => new[]
        {
            InlineKeyboardButton.WithCallbackData("+", $"Verification: true {chatId} {messageId} {locationId}"),
            InlineKeyboardButton.WithCallbackData("-", $"Verification: false {chatId} {messageId} {locationId}")
        };

        public static ReplyKeyboardMarkup GetRequestContactReplyMarkup()
        => new ReplyKeyboardMarkup(
        new[]
        {
            KeyboardButton.WithRequestContact("Перевірити номер телефону")
        });
    }
}
