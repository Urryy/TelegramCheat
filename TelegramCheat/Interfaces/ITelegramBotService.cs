using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramCheat.Interfaces;

public interface ITelegramBotService
{
    Task Start(Update upd);
    Task<TelegramBotClient> GetClient();
}
