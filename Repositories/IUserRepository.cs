using TRT_backend.Models;

namespace TRT_backend.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> GetByUsernameAsync(string username);
        Task<bool> UserExistsAsync(string username);
        Task<bool> UserExistsAsync(int id);
        Task<List<User>> GetUsersWithRolesAsync();
        Task<User> GetUserWithRolesAsync(int id);
        Task<User> GetUserWithRolesAsync(string username);
    }
} 