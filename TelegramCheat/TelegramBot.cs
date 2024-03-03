using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramCheat.Implementations;
using TelegramCheat.Interfaces;

namespace TelegramCheat;

public class TelegramBot
{
	private TelegramBotClient _client;
	private ITelegramBotService _botService;
	public TelegramBot(ITelegramBotService botService)
	{
		_botService = botService;
	}

	public async Task<TelegramBotClient> StartRecieveBot()
	{
		try
		{
            if (_client != null)
            {
                await _client.ReceiveAsync(Update, Error);
			}
			else
			{
                _client = await _botService.GetClient();
                await _client.ReceiveAsync(Update, Error);
            }
			return _client;
        }
		catch (Exception ex)
		{
            await CloseAndStartConnection();
			return _client;
		}
	}

	private async Task Update(ITelegramBotClient client, Update upd, CancellationToken token)
	{
		try
		{
			if (_client.Timeout.Minutes > 1)
			{
				await CloseAndStartConnection();
			}

			await _botService.Start(upd);
        }
		catch (Exception)
		{
            await CloseAndStartConnection();
        }
	}

	private async Task Error(ITelegramBotClient client, Exception upd, CancellationToken token)
	{
		if (_client.Timeout.Minutes > 1)
		{
            await CloseAndStartConnection();
        }
		return;
	}

	private async Task CloseAndStartConnection()
	{
        await _client.CloseAsync();
        await StartRecieveBot();
    }
}
