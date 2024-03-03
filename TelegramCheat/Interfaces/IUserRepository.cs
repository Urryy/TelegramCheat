using TelegramCheat.Entity;

namespace TelegramCheat.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllUsers();
    Task AddRangeUser(List<User> users);
    Task AddUser(User user);
    Task RemoveRangeUsers(List<User> users);
    Task RemoveUser(User user);
    Task UpdateUser(User user);
}
