using Microsoft.AspNetCore.Mvc;
using TRT_backend.Services;
using TRT_backend.Models;
using Microsoft.AspNetCore.Authorization;
using TRT_backend.Models.DTO;

namespace TRT_backend.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [Tags("UserManagement")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }
    
        [Tags("UserManagement")]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (await _userService.UserExistsAsync(dto.username))
            {
                return BadRequest("This user name has already added.");
            }

            var user = new User { username = dto.username, password = dto.password };
            var createdUser = await _userService.CreateUserAsync(user);
            
            return Ok("Register Successful.");
        }

        [Tags("UserManagement")]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!await _userService.ValidateUserCredentialsAsync(dto.username, dto.password))
            {
                return Unauthorized("Incorrect username or password.");
            }

            var user = await _userService.GetUserByUsernameAsync(dto.username);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            var token = await _userService.GenerateJwtTokenAsync(user);
            var roleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();

            return Ok(new { token = token, roleIds = roleIds });
        }

        [Tags("ClaimManagement")]
        [HttpPost("assign-claim")]
        [Authorize]
        public async Task<IActionResult> AssignClaim([FromBody] AssignClaimDto dto)
        {
            if (!_userService.IsUserAdminFromToken(User))
                return StatusCode(403, "Only admins can assign claims to users.");

            if (await _userService.UserHasClaimAsync(dto.UserId, dto.ClaimId))
                return BadRequest("User already has this claim.");

            await _userService.AssignClaimToUserAsync(dto.UserId, dto.ClaimId);
            return Ok("Claim assigned to user.");
        }
    
        [Tags("ClaimManagement")]
        [HttpPost("remove-claim")]
        [Authorize]
        public async Task<IActionResult> RemoveClaim([FromBody] AssignClaimDto dto)
        {
            if (!_userService.IsUserAdminFromToken(User))
                return StatusCode(403, "Only admins can remove claims from users.");

            if (!await _userService.UserHasClaimAsync(dto.UserId, dto.ClaimId))
                return NotFound("User does not have this claim.");

            await _userService.RemoveClaimFromUserAsync(dto.UserId, dto.ClaimId);
            return Ok("Claim removed from user.");
        }

        [Tags("UserManagement")]
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (!_userService.IsUserAdminFromToken(User))
                return StatusCode(403, "Only admins can delete users.");

            if (!await _userService.UserExistsAsync(id))
                return NotFound("User not found.");

            await _userService.DeleteUserAsync(id);
            return Ok("User deleted successfully.");
        }

        [Tags("ClaimManagement")]
        [HttpGet("user-claims/{userId}")]
        public async Task<IActionResult> GetUserClaims(int userId)
        {
            var claims = await _userService.GetUserClaimsAsync(userId);
            return Ok(claims);
        }

        [Tags("ClaimManagement")]
        [HttpGet("claims")]
        public async Task<IActionResult> GetAllClaims()
        {
            var claims = await _userService.GetAllClaimsAsync();
            return Ok(claims);
        }
    
        [Tags("UserManagement")]
        [HttpGet("roles")]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _userService.GetAllRolesAsync();
            return Ok(roles);
        }
    }
} 