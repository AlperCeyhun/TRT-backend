using Microsoft.AspNetCore.Mvc;
using TRT_backend.Data;
using TRT_backend.Models;
using System.Linq;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

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
        public IActionResult GetAllUsers()
        {
            var users = _context.Users
                .Select(u => new UserDto {
                    Id = u.Id,
                    username = u.username
                })
                .ToList();
                
            return Ok(users);
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
                return Unauthorized("Incorrect username or password.");
            }
        
            // Token üret
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]);
        
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.username),
                    new Claim("UserId", user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

              var token = tokenHandler.CreateToken(tokenDescriptor);
              var tokenString = tokenHandler.WriteToken(token);

              return Ok(new { Token = tokenString });
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
    }
} 