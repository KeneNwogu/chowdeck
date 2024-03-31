using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Chowdeck.Models
{
    public static class UserRole
    {
        public const string ADMIN = "admin";
        public const string USER = "user";
        public const string TEST_USER = "test_user";
        public const string RIDER = "rider";

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
