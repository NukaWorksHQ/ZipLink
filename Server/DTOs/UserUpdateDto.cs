using Server.Common;
using System.ComponentModel.DataAnnotations;

namespace Server.DTOs
{
    public class UserUpdateDto
    {
        [Required(ErrorMessage = "UserId cannot be null.")]
        [StringLength(32, MinimumLength = 4, ErrorMessage = "Username should be between 4 and 32 characters.")]
        public string Username { get; set; }
    }
}
