using TelegramCheat.Entity;

namespace TelegramCheat.Interfaces
{
    public interface IOwnProxyRepository
    {
        Task<IEnumerable<OwnProxy>> GetAll();
        Task AddProxy(OwnProxy proxy);
        Task UpdateProxy(OwnProxy proxy);
    }
}
