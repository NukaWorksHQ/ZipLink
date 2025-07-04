using System.ComponentModel.DataAnnotations;

namespace Server.DTOs
{
    public class LinkCreateDto
    {
        [Required(ErrorMessage = "UserId cannot be null.")]
        [StringLength(8, MinimumLength = 8, ErrorMessage = "UserId must be 8 characters.")]
        public required string UserId { get; set; }

        [Required(ErrorMessage = "Target cannot be null.")]
        [StringLength(2048, MinimumLength = 1, ErrorMessage = "Target must be between 1 and 2048 characters.")]
        public required string Target { get; set; }
    }
}
