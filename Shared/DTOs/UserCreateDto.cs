using Shared.Common;
using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs
{
    public class UserCreateDto
    {
        [Required(ErrorMessage = "Username cannot be null.")]
        [StringLength(32, MinimumLength = 4, ErrorMessage = "Username should be between 4 and 32 characters.")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "Role cannot be null.")]
        public required UserRole Role { get; set; }

        [Required(ErrorMessage = "Password cannot be null.")]
        public required string HashedPassword { get; set; }
    }
}
