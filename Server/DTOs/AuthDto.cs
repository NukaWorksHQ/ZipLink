using System.ComponentModel.DataAnnotations;

namespace Server.DTOs
{
    public class AuthDto
    {
        [Required(ErrorMessage = "Username cannot be null")]
        [StringLength(32, MinimumLength = 4, ErrorMessage = "Username must be between 4 and 32 characters.")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "Password cannot be null.")]
        [StringLength(256, MinimumLength = 8, ErrorMessage = "Password should be greater than 8 characters and max is 256.")]
        public required string Password { get; set; }
    }
}
