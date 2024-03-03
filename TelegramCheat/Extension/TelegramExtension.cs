using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramCheat.Extension;

public static class TelegramExtension
{
    public static bool IsExistChat(this Update? upd)
    {
        if (upd?.Message != null && upd.Message.Text != null)
            return true;
        return false;
    }

    public static bool IsExistForwaredMsgId(this Update? upd)
    {
        if (upd?.Message != null && upd.Message.ForwardFromMessageId != null)
            return true;
        return false;
    }

    public static bool IsExistDocument(this Update? upd)
    {
        if (upd?.Message != null && upd.Message.Document != null)
            return true;
        return false;
    }

    public static bool CheckMessage(this Update? upd, string message)
    {
        if (upd.Message.Text.ToLower().Contains(message))
            return true;
        return false;
    }
}
