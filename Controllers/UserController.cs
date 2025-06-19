using Microsoft.AspNetCore.Mvc;
using TRT_backend.Data;
using TRT_backend.Models;
using System.Linq;

namespace TRT_backend.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.UserName) || string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest("username and password can't be empty.");
            }
            if (_context.Users.Any(u => u.UserName == dto.UserName))
            {
                return BadRequest("This user name has already added.");
            }
            var user = new User { UserName = dto.UserName, Password = dto.Password };
            _context.Users.Add(user);
            _context.SaveChanges();
            return Ok("Register Succesfull.");
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto dto)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserName == dto.UserName && u.Password == dto.Password);
            if (user == null)
            {
                return Unauthorized("incorrect username or password.");
            }
            return Ok("login succesfull.");
        }

        public class RegisterDto
        {
            public string UserName { get; set; }
            public string Password { get; set; }
        }

        public class LoginDto
        {
            public string UserName { get; set; }
            public string Password { get; set; }
        }
    }
} 