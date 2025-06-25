using Microsoft.AspNetCore.Mvc;
using TRT_backend.Data;
using TRT_backend.Models;
using System.Linq;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace TRT_backend.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public UserController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
       } 

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Select(u => new UserDto {
                    Id = u.Id,
                    username = u.username
                })
                .ToListAsync();
                
            return Ok(users);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.username == dto.username))
            {
                return BadRequest("This user name has already added.");
            }
            var user = new User { username = dto.username, password = dto.password };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Otomatik olarak User rolü (RoleId=2) ata
            _context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = 2 });
            await _context.SaveChangesAsync();

            return Ok("Register Succesfull.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _context.Users
                .Where(u => u.username == dto.username && u.password == dto.password)
                .Select(u => new {
                    User = u,
                    RoleIds = u.UserRoles.Select(ur => ur.RoleId).ToList()
                })
                .FirstOrDefaultAsync();
        
            if (user == null)
            {
                return Unauthorized("Incorrect username or password.");
            }

            // Kullanıcının sahip olduğu tüm claimleri topla
            var roleClaimIds = _context.UserRoles
                .Where(ur => ur.UserId == user.User.Id)
                .SelectMany(ur => _context.RoleClaims.Where(rc => rc.RoleId == ur.RoleId).Select(rc => rc.ClaimId))
                .ToList();
            var roleClaims = _context.Claims.Where(c => roleClaimIds.Contains(c.Id)).Select(c => c.ClaimName);

            var userClaimNames = _context.UserClaims.Where(uc => uc.UserId == user.User.Id).Select(uc => uc.Claim.ClaimName);

            var allClaims = roleClaims.Concat(userClaimNames).Distinct().ToList();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.User.username),
                new Claim("UserId", user.User.Id.ToString())
            };

            foreach (var claimName in allClaims)
            {
                claims.Add(new Claim("permission", claimName));
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]);
        
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new { token = tokenString, roleIds = user.RoleIds });
        }

        
        [HttpPost("assign-role")]
        
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto dto)
        {
            int userId = GetUserIdFromToken();
            bool isAdmin = _context.UserRoles.Any(ur => ur.UserId == userId && ur.Role.RoleName == "Admin");
            if (!isAdmin && !HasClaim(userId, "Manage Users"))
                return StatusCode(403, "You are not authorized to perform this operation.");
            var user = await _context.Users.FindAsync(dto.UserId);
            var role = await _context.Roles.FindAsync(dto.RoleId);
            if (user == null || role == null)
                return NotFound("User or Role not found.");

            bool alreadyHas = await _context.UserRoles.AnyAsync(ur => ur.UserId == dto.UserId && ur.RoleId == dto.RoleId);
            if (alreadyHas)
                return BadRequest("User already has this role.");

            _context.UserRoles.Add(new UserRole { UserId = dto.UserId, RoleId = dto.RoleId });
            await _context.SaveChangesAsync();
            return Ok("Role assigned to user.");
        }

        
        [HttpPost("remove-role")]
        
        public async Task<IActionResult> RemoveRole([FromBody] AssignRoleDto dto)
        {
            int userId = GetUserIdFromToken();
            bool isAdmin = _context.UserRoles.Any(ur => ur.UserId == userId && ur.Role.RoleName == "Admin");
            if (!isAdmin && !HasClaim(userId, "Manage Users"))
                return StatusCode(403, "You are not authorized to perform this operation.");
            var userRole = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == dto.UserId && ur.RoleId == dto.RoleId);
            if (userRole == null)
                return NotFound("User does not have this role.");

            _context.UserRoles.Remove(userRole);
            await _context.SaveChangesAsync();
            return Ok("Role removed from user.");
        }

        [HttpPost("assign-claim")]
        public async Task<IActionResult> AssignClaim([FromBody] AssignClaimDto dto)
        {
            int userId = GetUserIdFromToken();
            bool isAdmin = _context.UserRoles.Any(ur => ur.UserId == userId && ur.Role.RoleName == "Admin");
            if (!isAdmin && !HasClaim(userId, "Manage Users"))
                return StatusCode(403, "You are not authorized to perform this operation.");
            var user = await _context.Users.FindAsync(dto.UserId);
            var claim = await _context.Claims.FindAsync(dto.ClaimId);
            if (user == null || claim == null)
                return NotFound("User or Claim not found.");

            bool alreadyHas = await _context.UserClaims.AnyAsync(uc => uc.UserId == dto.UserId && uc.ClaimId == dto.ClaimId);
            if (alreadyHas)
                return BadRequest("User already has this claim.");

            _context.UserClaims.Add(new UserClaim { UserId = dto.UserId, ClaimId = dto.ClaimId });
            await _context.SaveChangesAsync();
            return Ok("Claim assigned to user.");
        }

        [HttpPost("remove-claim")]
        public async Task<IActionResult> RemoveClaim([FromBody] AssignClaimDto dto)
        {
            int userId = GetUserIdFromToken();
            bool isAdmin = _context.UserRoles.Any(ur => ur.UserId == userId && ur.Role.RoleName == "Admin");
            if (!isAdmin && !HasClaim(userId, "Manage Users"))
                return StatusCode(403, "You are not authorized to perform this operation.");
            var userClaim = await _context.UserClaims.FirstOrDefaultAsync(uc => uc.UserId == dto.UserId && uc.ClaimId == dto.ClaimId);
            if (userClaim == null)
                return NotFound("User does not have this claim.");

            _context.UserClaims.Remove(userClaim);
            await _context.SaveChangesAsync();
            return Ok("Claim removed from user.");
        }

        [AllowAnonymous]
        [HttpGet("my-claims")]
        public IActionResult GetMyClaims()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null)
                return Unauthorized();
            int userId = int.Parse(userIdClaim.Value);

            // Rollerden gelen claimler
            var roleClaimIds = _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .SelectMany(ur => _context.RoleClaims.Where(rc => rc.RoleId == ur.RoleId).Select(rc => rc.ClaimId))
                .ToList();
            var roleClaims = _context.Claims.Where(c => roleClaimIds.Contains(c.Id)).Select(c => c.ClaimName);

            // Kullanıcıya özel claimler
            var userClaimNames = _context.UserClaims.Where(uc => uc.UserId == userId).Select(uc => uc.Claim.ClaimName);

            var allClaims = roleClaims.Concat(userClaimNames).Distinct().ToList();
            return Ok(allClaims);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            int userId = GetUserIdFromToken();
            bool isAdmin = _context.UserRoles.Any(ur => ur.UserId == userId && ur.Role.RoleName == "Admin");
            if (!isAdmin && !HasClaim(userId, "Delete User"))
                return StatusCode(403, "You are not authorized to perform this operation.");

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("User not found.");

            // Kullanıcıya ait mesajları sil
            var fromMessages = _context.Messages.Where(m => m.FromUserId == id);
            _context.Messages.RemoveRange(fromMessages);
            var toMessages = _context.Messages.Where(m => m.ToUserId == id);
            _context.Messages.RemoveRange(toMessages);

            // İlişkili tüm kayıtları sil
            var userRoles = _context.UserRoles.Where(ur => ur.UserId == id);
            _context.UserRoles.RemoveRange(userRoles);

            var userClaims = _context.UserClaims.Where(uc => uc.UserId == id);
            _context.UserClaims.RemoveRange(userClaims);

            var assignees = _context.Assignees.Where(a => a.UserId == id);
            _context.Assignees.RemoveRange(assignees);

            // Gerekirse başka ilişkili tablolar da eklenebilir

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok("User and all related data deleted successfully.");
        }

        private int GetUserIdFromToken()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        private bool HasClaim(int userId, string claimName)
        {
            var roleClaimIds = _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .SelectMany(ur => _context.RoleClaims.Where(rc => rc.RoleId == ur.RoleId).Select(rc => rc.ClaimId))
                .ToList();
            var roleClaims = _context.Claims.Where(c => roleClaimIds.Contains(c.Id)).Select(c => c.ClaimName);
            var userClaimNames = _context.UserClaims.Where(uc => uc.UserId == userId).Select(uc => uc.Claim.ClaimName);
            var allClaims = roleClaims.Concat(userClaimNames).Distinct();
            return allClaims.Contains(claimName);
        }

        public class RegisterDto
        {
            public string username { get; set; }
            public string password { get; set; }
        }

        public class LoginDto
        {
            public string username { get; set; }
            public string password { get; set; }
        }
        public class UserDto
        {
            public int Id { get; set; }
            public string username { get; set; }
        }

        public class AssignRoleDto
        {
            public int UserId { get; set; }
            public int RoleId { get; set; }
        }

        public class AssignClaimDto
        {
            public int UserId { get; set; }
            public int ClaimId { get; set; }
        }
    }
} 