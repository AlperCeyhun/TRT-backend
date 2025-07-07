using Microsoft.EntityFrameworkCore;
using TRT_backend.Data;
using TRT_backend.Models;

namespace TRT_backend.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.UserClaims)
                    .ThenInclude(uc => uc.Claim)
                .FirstOrDefaultAsync(u => u.username == username);
        }

        public async Task<bool> UserExistsAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.username == username);
        }

        public async Task<bool> UserExistsAsync(int id)
        {
            return await _context.Users.AnyAsync(u => u.Id == id);
        }

        public async Task<List<User>> GetUsersWithRolesAsync()
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.UserClaims)
                    .ThenInclude(uc => uc.Claim)
                .ToListAsync();
        }

        public async Task<User> GetUserWithRolesAsync(int id)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.UserClaims)
                    .ThenInclude(uc => uc.Claim)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User> GetUserWithRolesAsync(string username)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.UserClaims)
                    .ThenInclude(uc => uc.Claim)
                .FirstOrDefaultAsync(u => u.username == username);
        }

        public async Task<List<int>> GetAssignedTaskIdsAsync(int userId)
        {
            return await _context.Assignees
                .Where(a => a.UserId == userId)
                .Select(a => a.TaskId)
                .ToListAsync();
        }

        public async Task<List<int>> GetRoleIdsAsync(int userId)
        {
            return await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToListAsync();
        }

        public async Task<List<string>> GetRoleNamesAsync(int userId)
        {
            return await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.Role.RoleName)
                .ToListAsync();
        }

        public async Task<List<string>> GetUserClaimNamesAsync(int userId)
        {
            return await _context.UserClaims
                .Where(uc => uc.UserId == userId)
                .Select(uc => uc.Claim.ClaimName)
                .ToListAsync();
        }

        public async Task<List<string>> GetRoleClaimNamesAsync(int userId)
        {
            var roleIds = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToListAsync();
            return await _context.RoleClaims
                .Where(rc => roleIds.Contains(rc.RoleId))
                .Select(rc => rc.Claim.ClaimName)
                .ToListAsync();
        }

        public async Task<bool> UserHasClaimAsync(int userId, int claimId)
        {
            return await _context.UserClaims.AnyAsync(uc => uc.UserId == userId && uc.ClaimId == claimId);
        }

        public async Task AddClaimToUserAsync(int userId, int claimId)
        {
            var userClaim = new UserClaim { UserId = userId, ClaimId = claimId };
            _context.UserClaims.Add(userClaim);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveClaimFromUserAsync(int userId, int claimId)
        {
            var userClaim = await _context.UserClaims.FirstOrDefaultAsync(uc => uc.UserId == userId && uc.ClaimId == claimId);
            if (userClaim != null)
            {
                _context.UserClaims.Remove(userClaim);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveUserRelationsAsync(int userId, string username)
        {
            // Mesajları sil
            var messages = await _context.Messages.Where(m => m.FromUserId == userId || m.ToUserId == userId).ToListAsync();
            _context.Messages.RemoveRange(messages);
            // UserClaims
            var userClaims = await _context.UserClaims.Where(uc => uc.UserId == userId).ToListAsync();
            _context.UserClaims.RemoveRange(userClaims);
            // UserRoles
            var userRoles = await _context.UserRoles.Where(ur => ur.UserId == userId).ToListAsync();
            _context.UserRoles.RemoveRange(userRoles);
            // Assignees
            var assignees = await _context.Assignees.Where(a => a.UserId == userId).ToListAsync();
            _context.Assignees.RemoveRange(assignees);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Claims>> GetAllClaimsAsync()
        {
            return await _context.Claims.ToListAsync();
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            return await _context.Roles.ToListAsync();
        }
    }
} 