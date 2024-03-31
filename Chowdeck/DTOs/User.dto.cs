using Chowdeck.Models;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;

namespace Chowdeck.DTOs
{
    public class RegisterDto
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public string Email { get; set; }

        [Required]
        [ValidateUserRole]
        public string Role { get; set; }

        public bool validateDTO()
        {
            if (this.Role is string role && !UserRole.IsValidRole(role))
            {
                throw new BadHttpRequestException(
                    "Invalid user role. Please select a valid role from the predefined options.");
            }

            return true;
        }
    }

    public class LoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class UserTokenDto
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }

        public string Role { get; set; }

    }
}
