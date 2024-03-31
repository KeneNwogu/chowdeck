using Chowdeck.DTOs;
using Chowdeck.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Chowdeck.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : Controller
    {
        private readonly ChowdeckContext _context;
        private readonly IConfiguration _config;

        public UsersController(ChowdeckContext context, IConfiguration configuration)
        {
            _context = context;
            _config = configuration;
        }

        [HttpPost("register")]
        public IActionResult Register(RegisterDto userDto)
        {
            if (userDto.Role == "admin") 
                return BadRequest(new { message = "Admin role is not sanctioned for this endpoint" });

            User? user = _context.Users.FirstOrDefault(
                u => u.Username == userDto.Username || u.Email == userDto.Email);

            if (user != null) return BadRequest(new { message = "User with username or email already exists." });

            User newUser = new User {
                Email = userDto.Email,
                Username = userDto.Username, Role = userDto.Role,
                Password = BCrypt.Net.BCrypt.HashPassword(userDto.Password)
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            var response = new
            {
                user = newUser
            };

            return Ok(response);
        }

        [HttpPost("login")]
        public IActionResult Login(LoginDto userDto)
        {
            User? user = _context.Users.FirstOrDefault(u => u.Username ==  userDto.Username);
            if(user == null) return BadRequest(new { message = "Invalid username or password" });

            if (!BCrypt.Net.BCrypt.Verify(userDto.Password, user.Password))
                return BadRequest(new { message = "Invalid username or password" });

            var response = new
            {
                token = GenerateJSONWebToken(user),
                user
            };

            return Ok(response);
        }


        [HttpGet("protected")]
        [Authorize]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2", "value3", "value4", "value5" };
        }

        private string GenerateJSONWebToken(User userInfo)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Issuer"],
              new List<Claim> { 
                  new Claim(ClaimTypes.NameIdentifier, userInfo.Id),
                  new Claim(ClaimTypes.Email, userInfo.Email),
                  new Claim(ClaimTypes.Role, userInfo.Role)
              },
              expires: DateTime.Now.AddMinutes(120),
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
