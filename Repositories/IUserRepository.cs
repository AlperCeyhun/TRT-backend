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
        Task<List<int>> GetAssignedTaskIdsAsync(int userId);
        Task<List<int>> GetRoleIdsAsync(int userId);
        Task<List<string>> GetRoleNamesAsync(int userId);
        Task<List<string>> GetUserClaimNamesAsync(int userId);
        Task<List<string>> GetRoleClaimNamesAsync(int userId);
        Task<bool> UserHasClaimAsync(int userId, int claimId);
        Task AddClaimToUserAsync(int userId, int claimId);
        Task RemoveClaimFromUserAsync(int userId, int claimId);
        Task RemoveUserRelationsAsync(int userId, string username);
        Task<List<Claims>> GetAllClaimsAsync();
        Task<List<Role>> GetAllRolesAsync();
    }
} 