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
            if (_context.Users.Any(u => u.username == dto.username))
            {
                return BadRequest("This user name has already added.");
            }
            var user = new User { username = dto.username, password = dto.password };
            _context.Users.Add(user);
            _context.SaveChanges();
            return Ok("Register Succesfull.");
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto dto)
        {
            var user = _context.Users.FirstOrDefault(u => u.username == dto.username && u.password == dto.password);
            if (user == null)
            {
                return Unauthorized("incorrect username or password.");
            }
            return Ok("login succesfull.");
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
    }
} 