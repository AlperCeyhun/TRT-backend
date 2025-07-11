using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TRT_backend.Data;
using TRT_backend.Models;
using TRT_backend.Models.DTO;
using TRT_backend.Repositories;

namespace TRT_backend.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly ICacheService _cache;

        public UserService(IUserRepository userRepository, IConfiguration configuration, ICacheService cache)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _cache = cache;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            var cacheKey = "all_users";
            var cachedUsers = _cache.Get<List<User>>(cacheKey);
            
            if (cachedUsers != null)
                return cachedUsers;

            var users = await _userRepository.GetUsersWithRolesAsync();
            _cache.Set(cacheKey, users, TimeSpan.FromMinutes(30));
            return users;
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            var cacheKey = $"user_{id}";
            var cachedUser = _cache.Get<User>(cacheKey);
            
            if (cachedUser != null)
                return cachedUser;
            var user = await _userRepository.GetUserWithRolesAsync(id);
            if (user != null)
                _cache.Set(cacheKey, user, TimeSpan.FromMinutes(30));
            
            return user;
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            var cacheKey = $"user_username_{username}";
            var cachedUser = _cache.Get<User>(cacheKey);
            
            if (cachedUser != null)
                return cachedUser;

            var user = await _userRepository.GetByUsernameAsync(username);
            if (user != null)
                _cache.Set(cacheKey, user, TimeSpan.FromMinutes(30));
            
            return user;
        }

        public async Task<bool> IsUserAdminAsync(int userId)
        {
            var user = await GetUserByIdAsync(userId);
            return user.UserRoles?.Any(ur => ur.Role.RoleName == "Admin") ?? false;
        }

        public async Task<bool> HasUserClaimAsync(int userId, string claimName)
        {
            var user = await GetUserByIdAsync(userId);
            return user.UserClaims?.Any(uc => uc.Claim.ClaimName == claimName) ?? false;
        }

        public async Task<List<string>> GetUserClaimsAsync(int userId)
        {
            var user = await GetUserByIdAsync(userId);
            return user.UserClaims?.Select(uc => uc.Claim.ClaimName).ToList() ?? new List<string>();
        }

        public async Task<bool> ValidateUserCredentialsAsync(string username, string password)
        {
            var user = await GetUserByUsernameAsync(username);
            return user != null && user.password == password;
        }

        public async Task<string> GenerateJwtTokenAsync(User user)
        {
            var assignedTaskIds = await _userRepository.GetAssignedTaskIdsAsync(user.Id);

            var claims = new List<Claim>
            {
                new Claim("UserId", user.Id.ToString()),
                new Claim("Username", user.username)
            };

            var roleIds = await _userRepository.GetRoleIdsAsync(user.Id);
            var roleNames = await _userRepository.GetRoleNamesAsync(user.Id);
            var isAdmin = roleNames.Contains("Admin");

            claims.Add(new Claim("IsAdmin", isAdmin.ToString()));
            claims.Add(new Claim("CanViewAllTasks", isAdmin.ToString()));

            var permissions = new List<string>();
            permissions.AddRange(await _userRepository.GetUserClaimNamesAsync(user.Id));
            var roleClaims = await _userRepository.GetRoleClaimNamesAsync(user.Id);
            foreach (var claimName in roleClaims)
            {
                if (!permissions.Contains(claimName))
                    permissions.Add(claimName);
            }

            claims.Add(new Claim("RoleIds", string.Join(",", roleIds)));
            claims.Add(new Claim("RoleNames", string.Join(",", roleNames)));
            claims.Add(new Claim("Permissions", string.Join(",", permissions)));
            claims.Add(new Claim("AssignedTaskIds", string.Join(",", assignedTaskIds)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "default_secret_key"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(24),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public int GetUserIdFromToken(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst("UserId");
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        public bool IsUserAdminFromToken(ClaimsPrincipal user)
        {
            var isAdminClaim = user.FindFirst("IsAdmin");
            return isAdminClaim != null && bool.Parse(isAdminClaim.Value);
        }

        public List<string> GetUserRolesFromToken(ClaimsPrincipal user)
        {
            var roleNamesClaim = user.FindFirst("RoleNames");
            return roleNamesClaim != null ? roleNamesClaim.Value.Split(',').ToList() : new List<string>();
        }

        public List<string> GetUserPermissionsFromToken(ClaimsPrincipal user)
        {
            var permissionsClaim = user.FindFirst("Permissions");
            return permissionsClaim != null ? permissionsClaim.Value.Split(',').ToList() : new List<string>();
        }

        public bool HasUserPermissionFromToken(ClaimsPrincipal user, string permission)
        {
            var permissionsClaim = user.FindFirst("Permissions");
            if (permissionsClaim == null) return false;
            
            var permissions = permissionsClaim.Value.Split(',');
            return permissions.Contains(permission);
        }

        public List<int> GetAssignedTaskIdsFromToken(ClaimsPrincipal user)
        {
            var taskIdsClaim = user.FindFirst("AssignedTaskIds");
            if (taskIdsClaim == null || string.IsNullOrEmpty(taskIdsClaim.Value))
                return new List<int>();
            
            return taskIdsClaim.Value.Split(',')
                .Where(id => !string.IsNullOrEmpty(id))
                .Select(int.Parse)
                .ToList();
        }

        public bool CanViewAllTasksFromToken(ClaimsPrincipal user)
        {
            var canViewAllClaim = user.FindFirst("CanViewAllTasks");
            return canViewAllClaim != null && bool.Parse(canViewAllClaim.Value);
        }

        public async Task<bool> UserExistsAsync(string username)
        {
            var cacheKey = $"user_exists_{username}";
            var cachedResult = _cache.Get<bool?>(cacheKey);
            
            if (cachedResult.HasValue)
                return cachedResult.Value;

            var exists = await _userRepository.UserExistsAsync(username);
            _cache.Set(cacheKey, exists, TimeSpan.FromMinutes(10));
            return exists;
        }

        public async Task<bool> UserExistsAsync(int id)
        {
            var cacheKey = $"user_exists_id_{id}";
            var cachedResult = _cache.Get<bool?>(cacheKey);
            
            if (cachedResult.HasValue)
                return cachedResult.Value;

            var exists = await _userRepository.UserExistsAsync(id);
            _cache.Set(cacheKey, exists, TimeSpan.FromMinutes(10));
            return exists;
        }

        public async Task<User> CreateUserAsync(User user)
        {
            var createdUser = await _userRepository.AddAsync(user);
            
            // Cache'leri temizle
            _cache.Remove("all_users");
            _cache.Remove($"user_exists_{user.username}");
            
            return createdUser;
        }

        public async Task<bool> UserHasClaimAsync(int userId, int claimId)
        {
            var cacheKey = $"user_has_claim_{userId}_{claimId}";
            var cachedResult = _cache.Get<bool?>(cacheKey);
            if (cachedResult.HasValue)
                return cachedResult.Value;
            var hasClaim = await _userRepository.UserHasClaimAsync(userId, claimId);
            _cache.Set(cacheKey, hasClaim, TimeSpan.FromMinutes(10));
            return hasClaim;
        }

        public async Task AssignClaimToUserAsync(int userId, int claimId)
        {
            await _userRepository.AddClaimToUserAsync(userId, claimId);
            _cache.Remove($"user_{userId}");
            _cache.Remove($"user_has_claim_{userId}_{claimId}");
            _cache.Remove("all_users");
        }

        public async Task RemoveClaimFromUserAsync(int userId, int claimId)
        {
            await _userRepository.RemoveClaimFromUserAsync(userId, claimId);
            _cache.Remove($"user_{userId}");
            _cache.Remove($"user_has_claim_{userId}_{claimId}");
            _cache.Remove("all_users");
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await _userRepository.GetUserWithRolesAsync(id);
            if (user != null)
            {
                await _userRepository.RemoveUserRelationsAsync(id, user.username);
                await _userRepository.DeleteAsync(id);
                _cache.Remove("all_users");
                _cache.Remove($"user_{id}");
                _cache.Remove($"user_username_{user.username}");
                _cache.Remove($"user_exists_{user.username}");
                _cache.Remove($"user_exists_id_{id}");
                _cache.Remove("all_tasks");
                _cache.Remove("user_tasks");
            }
        }

        public async Task<List<Claims>> GetAllClaimsAsync()
        {
            var cacheKey = "all_claims";
            var cachedClaims = _cache.Get<List<Claims>>(cacheKey);
            if (cachedClaims != null)
                return cachedClaims;
            var claims = await _userRepository.GetAllClaimsAsync();
            _cache.Set(cacheKey, claims, TimeSpan.FromHours(1));
            return claims;
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            var cacheKey = "all_roles";
            var cachedRoles = _cache.Get<List<Role>>(cacheKey);
            if (cachedRoles != null)
                return cachedRoles;
            var roles = await _userRepository.GetAllRolesAsync();
            _cache.Set(cacheKey, roles, TimeSpan.FromHours(1));
            return roles;
        }

        public async Task<List<UserDto>> GetAllUserDtosAsync()
        {
            var users = await GetAllUsersAsync();
            return users.Select(u => new UserDto
            {
                Id = u.Id,
                username = u.username
            }).ToList();
        }

        public async Task<UserDto> GetUserDtoByIdAsync(int id)
        {
            var user = await GetUserByIdAsync(id);
            if (user == null) return null;
            return new UserDto
            {
                Id = user.Id,
                username = user.username
            };
        }
    }
} 