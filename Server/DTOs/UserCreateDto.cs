using System.ComponentModel.DataAnnotations;

namespace Server.DTOs
{
    public class UserCreateDto
    {
        [Required(ErrorMessage = "UserId cannot be null.")]
        [StringLength(8, MinimumLength = 8, ErrorMessage = "UserId must be 8 characters.")]
        public required string UserId { get; set; }

        [Required(ErrorMessage = "UserId cannot be null.")]
        [StringLength(32, MinimumLength = 4, ErrorMessage = "Username should be between 4 and 32 characters.")]
        public required string Username { get; set; }


        [Required(ErrorMessage = "Password cannot be null.")]
        public required string HashedPassword { get; set; }
    }
}
