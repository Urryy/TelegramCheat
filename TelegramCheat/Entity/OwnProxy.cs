namespace TelegramCheat.Entity;

public class OwnProxy
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Host { get; set; }
    public int Port { get; set; }
    public string Login { get; set; }
    public string Password { get; set; }
    public bool Available { get; set; } = true;
    public OwnProxy(string host, int port, string login, string password)
    {
        Host = host;
        Port = port;
        Login = login;
        Password = password;
    }
}
