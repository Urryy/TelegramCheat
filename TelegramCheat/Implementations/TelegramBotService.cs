using Spire.Xls;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramCheat.Commands;
using TelegramCheat.Consts;
using TelegramCheat.Entity;
using TelegramCheat.Extension;
using TelegramCheat.Interfaces;
using TL;
using WTelegram;
using Starksoft.Net.Proxy;
using Microsoft.Extensions.Configuration;

namespace TelegramCheat.Implementations;

public class TelegramBotService : ITelegramBotService
{
    private TelegramBotClient _client;
    private readonly IConfiguration _config;
    private IServiceProvider _srvcProvider;
    private string Command { get; set; } = "";
    private string LastCommand { get; set; } = "";
    private string phone;

    private string verification_code = string.Empty;
    private string pathFile = string.Empty;
    private int countLikes = 0;
    private int countSubscribes = 0;

    public TelegramBotService(IConfiguration config, IServiceProvider srvcProvider)
    {
        _config = config;
        _srvcProvider = srvcProvider;
    }

    public async Task<TelegramBotClient> GetClient()
    {
        if (_client != null) return _client;
        _client = new TelegramBotClient(_config["TelegramToken"]);
        return _client;
    }

    public async Task Start(Telegram.Bot.Types.Update upd)
    {
        if (upd?.IsExistChat() == true && upd.CheckMessage("start") == true)
            await ExecuteStart(upd);

        if (upd?.IsExistChat() == true && upd.Message.Text.Contains(TelegramMessagesConsts.Back))
            Command = LastCommand;

        #region Login
        if (upd?.IsExistChat() == true && upd.Message.Text.Contains(TelegramMessagesConsts.AddMembersTxt) && Command == "")
        {
            await ExecutePreLogin(upd);
            return;
        }

        if (upd?.IsExistChat() == true && Command == CurrentCommand.AddUser)
        {
            await ExectueLogin(upd);
            return;
        }

        if (upd?.IsExistChat() == true && Command == CurrentCommand.AddCodeToUser)
        {
            await ExecutePostLogin(upd);
            return;
        }
        #endregion

        #region Subscribe
        if (upd?.IsExistChat() == true && upd.Message.Text.Contains(TelegramMessagesConsts.AddMembersChannel) && Command == "")
        {
            await ExecutePrePreAddMembersToChannel(upd);
            return;
        }

        if (upd?.IsExistChat() == true && Command == CurrentCommand.CountSubscribe)
        {
            await ExecutePreAddMembersToChannel(upd);
            return;
        }

        if (upd?.IsExistChat() == true && Command == CurrentCommand.Subscribe)
        {
            await ExecuteAddMembersToChannel(upd);
            return;
        }
           
        #endregion

        #region Like To Message
        if (upd?.IsExistChat() == true && upd.Message.Text.Contains(TelegramMessagesConsts.AddLikesByMembers) && Command == "")
        {
            await ExecutePreCountLikeMessageByMembers(upd);
            return;
        }

        if (upd?.IsExistChat() == true && Command == CurrentCommand.LikeToPostCount)
        {
            await ExecutePreLikeMessageByMembers(upd);
            return;
        }

        if (upd?.IsExistForwaredMsgId() == true && Command == CurrentCommand.LikeToPost)
        {
            await ExecuteLikeMessageByMembers(upd);
            return;
        }
        #endregion

        #region Add comment into post
        if (upd?.IsExistChat() == true && upd.Message.Text.Contains(TelegramMessagesConsts.AddCommentsToChannel) && Command == "")
        {
            await ExecutePreAddCommentIntoPost(upd);
            return;
        }

        if (upd?.IsExistDocument() == true && Command == CurrentCommand.SaveCommentsFromFile)
        {
            await ExecutePreSaveExcelAddCommentIntoPost(upd);
            return;
        }

        if (upd?.IsExistForwaredMsgId() == true && Command == CurrentCommand.CommentToPost)
        {
            await ExecuteAddCommentIntoPost(upd);
            return;
        }
        #endregion

        #region Administration - All Users.
        if (upd?.IsExistChat() == true && upd.Message.Text.Contains(TelegramMessagesConsts.AllUsers) && Command == "")
        {
            await ExecutePrintAllUsers(upd);
            return;
        }
        #endregion

        #region Add Proxy
        if (upd?.IsExistChat() == true && upd.Message.Text.Contains(TelegramMessagesConsts.AddProxy))
        {
            await ExecuteAddProxyMessage(upd);
            return;
        }

        if (upd?.IsExistChat() == true && Command == CurrentCommand.AddProxy)
        {
            await ExecuteAddProxy(upd);
            return;
        }
        #endregion

        verification_code = "";
        Command = "";
    }

    #region Start message
    private async Task ExecuteStart(Telegram.Bot.Types.Update upd)
    {
        await _client.SendTextMessageAsync(upd.Message.Chat.Id, "Добро пожаловать! Я бот для накручивания - который готов помочь вам с накруткой канал.\n\n" +
                                            "1. Добавление пользователя в базу данных.\n\n" +
                                            "2. Накрутка подписок.\n\n" +
                                            "3. Накрутка лайков.", replyMarkup: ButtonExtension.GetStartButton());
    }
    #endregion

    #region Login
    private async Task ExecutePreLogin(Telegram.Bot.Types.Update upd)
    {
        
        var availableProxy = await GetActiveProxy();
        if(availableProxy == null) 
            await _client.SendTextMessageAsync(upd.Message.Chat.Id, "На данный момент нет доступных Proxy. Добавьте Proxy для дальнейшей работы.");
        else
        {
            Command = CurrentCommand.AddUser; 
            await _client.SendTextMessageAsync(upd.Message.Chat.Id, "Введите номер пользователя.");
        } 
    }
    private async Task ExectueLogin(Telegram.Bot.Types.Update upd)
    {
        Command = CurrentCommand.AddCodeToUser;
        var accs = new List<Entity.User>();
        var num = upd.Message.Text;
        try
        {
            await _client.SendTextMessageAsync(upd.Message.Chat.Id, "Дождитесь сообщение об успешной авторизации");
            Task task = Task.Factory.StartNew(async () =>
            {
                var availableProxy = await GetActiveProxy();
                try
                {
                    using (var clientWTelegram = new WTelegram.Client(int.Parse(_config["AppId"]), _config["AppHash"], $"sessions/{num}_session"))
                    {
                        clientWTelegram.TcpHandler = async (address, port) =>
                        {
                            var socks = new Socks5ProxyClient(availableProxy.Host, availableProxy.Port, availableProxy.Login, availableProxy.Password);
                            return socks.CreateConnection(address, port);
                        };
                        await _client.SendTextMessageAsync(upd.Message.Chat.Id, $"Отправьте код верификации, который пришел на данный номер {num}.");
                        await DoLogin(num, clientWTelegram);

                        accs.Add(new Entity.User(clientWTelegram.User.MainUsername ?? clientWTelegram.User.first_name, clientWTelegram.UserId, clientWTelegram.TLConfig.tmp_sessions,
                            num, clientWTelegram.TLConfig.autologin_token ?? "", clientWTelegram.TLConfig.expires, availableProxy.Id));
                        availableProxy.Available = false;
                    }
                    verification_code = "";
                }
                catch (Exception ex)
                {
                    Command = "";
                    verification_code = "";
                    await _client.SendTextMessageAsync(upd.Message.Chat.Id, $"При авторизации возникла ошибка.\n{ex.Message}");
                    throw;
                }

                using (var scope = _srvcProvider.CreateScope())
                {
                    var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

                    var usersIds = (await repository.GetAllUsers()).Select(i => i.TelegramId);
                    var entities = accs.Where(i => !usersIds.Contains(i.TelegramId)).ToList();
                    if (entities.Any())
                        await repository.AddRangeUser(entities);

                    var proxyRepository = scope.ServiceProvider.GetRequiredService<IOwnProxyRepository>();
                    await proxyRepository.UpdateProxy(availableProxy);
                }

                Command = "";
                verification_code = "";
                await _client.SendTextMessageAsync(upd.Message.Chat.Id, "Авторизация прошла успешно");
            });
        }
        catch (Exception ex)
        {
            Command = CurrentCommand.AddUser;
            verification_code = "";
            await _client.SendTextMessageAsync(upd.Message.Chat.Id, "Введите корректный номер телефона или код!");
            return;
        }
    }
    private async Task ExecutePostLogin(Telegram.Bot.Types.Update upd)
    {
        if (!int.TryParse(upd.Message.Text, out int res))
        {
            verification_code = upd.Message.Text;
            Command = string.Empty;
            return;
        }
        
        verification_code = upd.Message.Text;
        Command = string.Empty;
    }
    #endregion

    #region Subscribe
    private async Task ExecutePrePreAddMembersToChannel(Telegram.Bot.Types.Update upd)
    {
        Command = CurrentCommand.CountSubscribe;
        LastCommand = "";
        await _client.SendTextMessageAsync(upd.Message.Chat.Id, "Пришлите колличество нужных вам подписок на канал");
    }

    private async Task ExecutePreAddMembersToChannel(Telegram.Bot.Types.Update upd)
    {
        if (!int.TryParse(upd.Message.Text, out var resultCount))
            return;
        countSubscribes = resultCount;
        Command = CurrentCommand.Subscribe;
        LastCommand = CurrentCommand.CountSubscribe;
        await _client.SendTextMessageAsync(upd.Message.Chat.Id, "Пришлите ссылку на канал, на который требуется подписаться.\n\nПример: https://t.me/yourchannelnamefromchannel. \n\nТут вы должны ввести слово которое идет после https://t.me/ !");
    }

    private async Task ExecuteAddMembersToChannel(Telegram.Bot.Types.Update upd)
    {
        try
        {
            int localCount = 1;
            var users = new List<Entity.User>();
            var bannedUsers = new List<Entity.User>();
            using (var scope = _srvcProvider.CreateScope())
            {
                var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                users.AddRange(await repository.GetAllUsers());
            }
            users = users.Where(i => !i.IsBanned).ToList();

            foreach (var item in users)
            {
                if (localCount == countSubscribes)
                    break;
                
                try
                {
                    phone = item.Number;
                    var availableProxy = await GetProxyFromUser(item.OwnProxyId);
                    using (var wtc = new WTelegram.Client(Config))
                    {
                        wtc.TcpHandler = async (address, port) =>
                        {
                            var socks = new Socks5ProxyClient(availableProxy.Host, availableProxy.Port, availableProxy.Login, availableProxy.Password);
                            return socks.CreateConnection(address, port);
                        };
                        await wtc.LoginUserIfNeeded();
                        
                        var resolved = await wtc.Contacts_ResolveUsername(upd.Message.Text);

                        if (resolved.Channel is not null && resolved.Channel is Channel channel_chn)
                            await wtc.Channels_JoinChannel(channel_chn);

                        if (resolved.Chat is not null && resolved.Chat is Channel channel_cht)
                            await wtc.Channels_JoinChannel(channel_cht);
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.ToLower().Contains("banned"))
                    {
                        item.IsBanned = true;
                        bannedUsers.Add(item);
                        continue;
                    }
                }
                localCount++;
            }
            Command = "";
            LastCommand = "";
            await BannedUsers(bannedUsers);
            await _client.SendTextMessageAsync(upd.Message.Chat.Id, "Пользователи успешно добавлены в группу.");
        }
        catch (Exception ex)
        {
            await _client.SendTextMessageAsync(upd.Message.Chat.Id, "Введите корректное название канала!");
            Command = CurrentCommand.Subscribe;
            LastCommand = CurrentCommand.CountSubscribe; 
            return;
        }
    }
    #endregion

    #region Like To Message
    private async Task ExecutePreCountLikeMessageByMembers(Telegram.Bot.Types.Update upd)
    {
        Command = CurrentCommand.LikeToPostCount;
        LastCommand = "";
        await _client.SendTextMessageAsync(upd.Message.Chat.Id, "Пришлите нужное вам колличество реакций, которые нужно проставить.");
    }

    private async Task ExecutePreLikeMessageByMembers(Telegram.Bot.Types.Update upd)
    {
        if (int.TryParse(upd.Message.Text, out var countOfLikes))
            countLikes = countOfLikes;
        Command = CurrentCommand.LikeToPost;
        LastCommand = CurrentCommand.LikeToPostCount;
        await _client.SendTextMessageAsync(upd.Message.Chat.Id, "Пришлите через функцию Forward сообщения с канала, на которое стоит проставить реакции.");
    }

    private async Task ExecuteLikeMessageByMembers(Telegram.Bot.Types.Update upd)
    {
        try
        {
            if(upd.Message.ForwardFromMessageId == null)
            {
                await _client.SendTextMessageAsync(upd.Message.Chat.Id, "Вышлите корректное сообщение для установки реакции!");
                Command = CurrentCommand.LikeToPost;
                Command = CurrentCommand.LikeToPostCount;
                return;
            }

            var users = new List<Entity.User>();
            var bannedUsers = new List<Entity.User>();
            using (var scope = _srvcProvider.CreateScope())
            {
                var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                users.AddRange(await repository.GetAllUsers());
            }
            users = users.Where(i => !i.IsBanned).ToList();

            for (int i = 0; i < countLikes; i++)
            {
                if (users.Count == i)
                    break;

                phone = users[i].Number;
                var availableProxy = await GetProxyFromUser(users[i].OwnProxyId);
                using (var wtc = new WTelegram.Client(Config))
                {
                    try
                    {
                        wtc.TcpHandler = async (address, port) =>
                        {
                            var socks = new Socks5ProxyClient(availableProxy.Host, availableProxy.Port, availableProxy.Login, availableProxy.Password);
                            return socks.CreateConnection(address, port);
                        };
                        await wtc.LoginUserIfNeeded();

                        InputPeer channel = await wtc.Contacts_ResolveUsername(upd.Message.ForwardFromChat.Username);

                        var full = await wtc.GetFullChat(channel);
                        var all_emoji = await wtc.Messages_GetAvailableReactions();
                        Reaction reaction = full.full_chat.AvailableReactions switch
                        {
                            ChatReactionsSome some => some.reactions[0],
                            ChatReactionsAll all => all.flags.HasFlag(ChatReactionsAll.Flags.allow_custom) && wtc.User.flags.HasFlag(TL.User.Flags.premium)
                                ? new ReactionCustomEmoji { document_id = 5190875290439525089 }
                                : new ReactionEmoji { emoticon = all_emoji.reactions[0].reaction },
                            _ => null
                        };
                        if (reaction == null) return;

                        if (upd.Message.ForwardFromMessageId != null)
                            await wtc.Messages_SendReaction(channel, upd.Message.ForwardFromMessageId.Value, reaction: new[] { reaction });
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.ToLower().Contains("banned"))
                        {
                            users[i].IsBanned = true;
                            bannedUsers.Add(users[i]);
                            continue;
                        }
                        await _client.SendTextMessageAsync(upd.Message.Chat.Id, $"Произошла ошибка при установке реакции: {ex.Message}");
                    }
                }
            }
            Command = "";
            LastCommand = "";
            await BannedUsers(bannedUsers);
            await _client.SendTextMessageAsync(upd.Message.Chat.Id, "Реакция проставлена успешно!");
        }
        catch (Exception ex)
        {
            await _client.SendTextMessageAsync(upd.Message.Chat.Id, "Вышлите корректное сообщение для установки реакции!");
            Command = CurrentCommand.LikeToPost;
            LastCommand = CurrentCommand.LikeToPostCount;
            return;
        }
    }

    #endregion

    #region Add comment to discussion
    private async Task ExecutePreAddCommentIntoPost(Telegram.Bot.Types.Update upd)
    {
        Command = CurrentCommand.SaveCommentsFromFile;
        LastCommand = "";
        await _client.SendTextMessageAsync(upd.Message.Chat.Id, "Пришлите excel файл с комментариями.");
    }

    private async Task ExecutePreSaveExcelAddCommentIntoPost(Telegram.Bot.Types.Update upd)
    {
        try
        {
            if(upd.Message.Document == null)
            {
                Command = CurrentCommand.SaveCommentsFromFile;
                LastCommand = "";
                await _client.SendTextMessageAsync(upd.Message.Chat.Id, "Пришлите корректный файл.");
                return;
            }

            var fileFromBot = await _client.GetFileAsync(upd.Message.Document.FileId);
            var filePath = fileFromBot.FilePath;

            using (var saveImageStream = new FileStream(filePath, FileMode.Create))
            {
                await _client.DownloadFileAsync(fileFromBot.FilePath, saveImageStream);
            }

            pathFile = fileFromBot.FilePath;
            Command = CurrentCommand.CommentToPost;
            LastCommand = CurrentCommand.SaveCommentsFromFile;
            await _client.SendTextMessageAsync(upd.Message.Chat.Id, "Пришлите через функцию Forward сообщения с канала, на котором нужно оставить комментарии.");
        }
        catch (Exception ex)
        {
            Command = CurrentCommand.SaveCommentsFromFile;
            LastCommand = "";
            await _client.SendTextMessageAsync(upd.Message.Chat.Id, "Пришлите корректный файл.");
            return;
        }
    }

    private async Task ExecuteAddCommentIntoPost(Telegram.Bot.Types.Update upd)
    {
        try
        {
            #region Add Comments
            List<string> comments = new List<string>();
            if (string.IsNullOrEmpty(pathFile))
            {
                Command = CurrentCommand.SaveCommentsFromFile;
                LastCommand = "";
                await _client.SendTextMessageAsync(upd.Message.Chat.Id, "Пришлите корректный файл.");
                return;
            }

            try
            {
                Workbook wb = new Workbook();
                wb.LoadFromFile(pathFile);
                Worksheet sheet = wb.Worksheets[0];

                CellRange locatedRange = sheet.AllocatedRange;

                for (int i = 1; i < locatedRange.Rows.Length; i++)
                    for (int j = 1; j < locatedRange.Rows[i].ColumnCount; j++)
                        if (!string.IsNullOrEmpty(locatedRange[i + 1, j + 1].Value))
                            comments.Add(locatedRange[i + 1, j + 1].Value + "  ");
            }
            catch (Exception ex)
            {
                Command = CurrentCommand.SaveCommentsFromFile;
                LastCommand = "";
                await _client.SendTextMessageAsync(upd.Message.Chat.Id, "Пришлите корректный файл.");
                return;
            }
            #endregion

            var users = new List<Entity.User>();
            var bannedUsers = new List<Entity.User>();
            using (var scope = _srvcProvider.CreateScope())
            {
                var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                users.AddRange(await repository.GetAllUsers());
            }
            users = users.Where(i => !i.IsBanned).ToList(); 


            for (int i = 0; i < users.Count; i++)
            {
                try
                {
                    phone = users[i].Number;
                    if (comments.Count == i)
                        break;

                    string comment = comments[i];
                    var availableProxy = await GetProxyFromUser(users[i].OwnProxyId);
                    using (var wtc = new WTelegram.Client(Config))
                    {
                        wtc.TcpHandler = async (address, port) =>
                        {
                            var socks = new Socks5ProxyClient(availableProxy.Host, availableProxy.Port, availableProxy.Login, availableProxy.Password);
                            return socks.CreateConnection(address, port);
                        };
                        await wtc.LoginUserIfNeeded();

                        InputPeer channel = await wtc.Contacts_ResolveUsername(upd.Message.ForwardFromChat.Username);

                        if (upd.Message.ForwardFromMessageId != null)
                        {
                            var discussion = await wtc.Messages_GetDiscussionMessage(channel, upd.Message.ForwardFromMessageId.Value);
                            var groupMsg = discussion.messages[0];
                            await wtc.SendMessageAsync(discussion.chats[groupMsg.Peer.ID], comment, reply_to_msg_id: groupMsg.ID);
                        }

                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.ToLower().Contains("banned"))
                    {
                        users[i].IsBanned = true;
                        bannedUsers.Add(users[i]);
                        continue;
                    }
                }
                
            }
            Command = "";
            LastCommand = "";
            await BannedUsers(bannedUsers);
            await _client.SendTextMessageAsync(upd.Message.Chat.Id, "Вы успешно прокоментировали в данной дискуссии.");
        }
        catch (Exception ex)
        {
            Command = CurrentCommand.CommentToPost;
            LastCommand = CurrentCommand.SaveCommentsFromFile;
            await _client.SendTextMessageAsync(upd.Message.Chat.Id, "При комментировании возникла ошибка.");
            return;
        }
    }
    #endregion

    #region Administration - All Users.
    async Task ExecutePrintAllUsers(Telegram.Bot.Types.Update upd)
    {
        var users = new List<Entity.User>();
        using (var scope = _srvcProvider.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            users.AddRange(await repository.GetAllUsers());
        }

        if (users.Count == 0)
            await _client.SendTextMessageAsync(upd.Message.Chat.Id, "В дазе данных нет пользователей.");
        else
        {
            await _client.SendTextMessageAsync(upd.Message.Chat.Id, $"Число активных аккаунтов: {users.Where(i => !i.IsBanned).Count()}\n\n");
            StringBuilder sb = new StringBuilder();
            foreach (var user in users)
            {
                var activeText = user.IsBanned == true ? "не активен ❌" : "активен ✅";
                sb.AppendLine($"{user.Number}  -  {activeText}");
            }

            await _client.SendTextMessageAsync(upd.Message.Chat.Id, sb.ToString());
        }
    }
    #endregion

    #region Add Proxy
    async Task ExecuteAddProxyMessage(Telegram.Bot.Types.Update upd)
    {
        Command = CurrentCommand.AddProxy;
        LastCommand = "";
        await _client.SendTextMessageAsync(upd.Message.Chat.Id, "Пришлите вашу Proxy для вставки в базу данных." +
            "\n\nПрокси должна иметь вид - host:port:login:password");
    }
    async Task ExecuteAddProxy(Telegram.Bot.Types.Update upd)
    {
        LastCommand = CurrentCommand.AddProxy;
        try
        {
            var proxy = upd.Message.Text.Split(":");
            if (proxy.Length == 4) 
            {
                var ownProxy = new OwnProxy(proxy[0], int.Parse(proxy[1]), proxy[2], proxy[3]);
                using (var scope = _srvcProvider.CreateScope())
                {
                    var repositoryProxy = scope.ServiceProvider.GetRequiredService<IOwnProxyRepository>();
                    await repositoryProxy.AddProxy(ownProxy);
                    Command = "";
                }
                await _client.SendTextMessageAsync(upd.Message.Chat.Id, "Proxy была успешно добавлена в базу данных.");
            }
            else
            {
                await _client.SendTextMessageAsync(upd.Message.Chat.Id, "При вставке Proxy в Базу данных возникла ошибка.\nПришлите еще раз корректную Proxy");
                LastCommand = CurrentCommand.AddProxy;
                Command = CurrentCommand.AddProxy;
            }

        }
        catch (Exception)
        {
            LastCommand = CurrentCommand.AddProxy;
            Command = CurrentCommand.AddProxy;
            await _client.SendTextMessageAsync(upd.Message.Chat.Id, "При вставке Proxy в Базу данных возникла ошибка.\nПришлите еще раз корректную Proxy");
        }
    }
    #endregion

    async Task DoLogin(string loginInfo, WTelegram.Client client) 
    {
        if (!string.IsNullOrEmpty(verification_code))
            verification_code = "";
        while (client.User == null)
            switch (await client.Login(loginInfo))
            {
                case "verification_code":
                    {
                        Console.Write("Code: ");
                        while (true)
                            if (!string.IsNullOrEmpty(verification_code))
                            {
                                loginInfo = verification_code;
                                break;
                            }
                        break;  
                    }
                case "password": loginInfo = "secret!"; break;
                default: loginInfo = null; break;
            }
    }
    async Task BannedUsers(List<Entity.User> entities)
    {
        if (entities.Count == 0)
            return;

        using (var scope = _srvcProvider.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            foreach (var item in entities)
            {
               await repository.UpdateUser(item);
            }
        }
    }
    string Config(string what)
    {
        switch (what)
        {
            case "api_id": return _config["AppId"];
            case "api_hash": return _config["AppHash"];
            case "phone_number": return phone;
            case "session_pathname": return $"sessions/{phone}_session";
            case "verification_code": return Console.ReadLine();
            default: return null;
        };

    }
    async Task<OwnProxy> GetActiveProxy()
    {
        using (var scope = _srvcProvider.CreateScope())
        {
            var repositoryProxy = scope.ServiceProvider.GetRequiredService<IOwnProxyRepository>();
            var proxies = await repositoryProxy.GetAll();
            var activeProxy = proxies.FirstOrDefault(i => i.Available);
            return activeProxy;
        }
    }
    async Task<OwnProxy> GetProxyFromUser(Guid objectId)
    {
        using (var scope = _srvcProvider.CreateScope())
        {
            var repositoryProxy = scope.ServiceProvider.GetRequiredService<IOwnProxyRepository>();
            var proxies = await repositoryProxy.GetAll();
            var activeProxy = proxies.FirstOrDefault(i => i.Id == objectId);
            return activeProxy;
        }
    }
}
