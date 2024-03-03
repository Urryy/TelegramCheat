namespace TelegramCheat.Entity;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OwnProxyId { get; set; }
    public string Name { get; set; } = default!;
    public long TelegramId { get; set; } = default!;
    public int Session { get; set; } = default!;
    public string Token { get; set; } = default!;
    public string Number { get; set; } = default!;  
    public DateTime Date { get; set; } = default!;
    public bool IsBanned { get; set; } = default!;
    public User(string name, long telegramId, int session, string number, string token, DateTime date, Guid ownProxyId)
    {
        Name = name;
        TelegramId = telegramId;
        Session = session;
        Number = number;
        Token = token;
        Date = date;
        OwnProxyId = ownProxyId;
    }
}
