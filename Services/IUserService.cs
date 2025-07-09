using TRT_backend.Models;
using System.Security.Claims;
using TRT_backend.Models.DTO;

namespace TRT_backend.Services
{
    public interface IUserService
    {
        Task<User> GetUserByIdAsync(int id);
        Task<User> GetUserByUsernameAsync(string username);
        Task<bool> IsUserAdminAsync(int userId);
        Task<bool> HasUserClaimAsync(int userId, string claimName);
        Task<List<string>> GetUserClaimsAsync(int userId);
        Task<bool> ValidateUserCredentialsAsync(string username, string password);
        Task<string> GenerateJwtTokenAsync(User user);
        int GetUserIdFromToken(ClaimsPrincipal user);
        Task<List<User>> GetAllUsersAsync();
        Task<bool> UserExistsAsync(string username);
        Task<bool> UserExistsAsync(int id);
        Task<User> CreateUserAsync(User user);
        Task<bool> UserHasClaimAsync(int userId, int claimId);
        Task AssignClaimToUserAsync(int userId, int claimId);
        Task RemoveClaimFromUserAsync(int userId, int claimId);
        Task DeleteUserAsync(int id);
        Task<List<Claims>> GetAllClaimsAsync();
        Task<List<Role>> GetAllRolesAsync();
        
        // Token'dan bilgi okuma metodları
        bool IsUserAdminFromToken(ClaimsPrincipal user);
        List<string> GetUserRolesFromToken(ClaimsPrincipal user);
        List<string> GetUserPermissionsFromToken(ClaimsPrincipal user);
        bool HasUserPermissionFromToken(ClaimsPrincipal user, string permission);
        List<int> GetAssignedTaskIdsFromToken(ClaimsPrincipal user);
        bool CanViewAllTasksFromToken(ClaimsPrincipal user);
        Task<List<UserDto>> GetAllUserDtosAsync();
        Task<UserDto> GetUserDtoByIdAsync(int id);
    }
} 