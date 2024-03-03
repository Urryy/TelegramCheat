using Telegram.Bot.Types.ReplyMarkups;
using TelegramCheat.Consts;

namespace TelegramCheat.Extension;

public static class ButtonExtension
{
    public static IReplyMarkup GetStartButton()
    {
        return new ReplyKeyboardMarkup(new List<List<KeyboardButton>>
        {
            new List<KeyboardButton>
            {  
                new KeyboardButton(TelegramMessagesConsts.AddMembersTxt),
                new KeyboardButton(TelegramMessagesConsts.AddMembersChannel)
            },
            new List<KeyboardButton>
            {
                new KeyboardButton(TelegramMessagesConsts.AddLikesByMembers),
                new KeyboardButton(TelegramMessagesConsts.AddCommentsToChannel)
            },
            new List<KeyboardButton>
            {
                new KeyboardButton(TelegramMessagesConsts.AddProxy),
                new KeyboardButton(TelegramMessagesConsts.AllUsers)
            },
            new List<KeyboardButton>
            {
                new KeyboardButton(TelegramMessagesConsts.Back)
            }
        }){ ResizeKeyboard = true };
    }
}
