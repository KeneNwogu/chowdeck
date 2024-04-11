using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Chowdeck.Models
{
    public static class UserRole
    {
        public const string ADMIN = "ADMIN";
        public const string USER = "USER";
        public const string TEST_USER = "TEST_USER";
        public const string RIDER = "RIDER";

        public static bool IsValidRole(string role)
        {
            return role == ADMIN || role == USER || role == TEST_USER || role == RIDER;
        }
    }

    public class User
    {
        [Key]
        public string Id { get; set; }

        public string Username { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public string Password { get; set; }

        [Required]
        [ValidateUserRole]
        public string Role { get; set; }

        public string? ProfileImage { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ValidateUserRoleAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is string role && !UserRole.IsValidRole(role))
            {
                throw new BadHttpRequestException(
                    "Invalid user role. Please select a valid role from the predefined options.");
            }

            return ValidationResult.Success;
        }
    }
}
