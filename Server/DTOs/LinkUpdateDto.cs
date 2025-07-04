using System.ComponentModel.DataAnnotations;

namespace Server.DTOs
{
    public class LinkUpdateDto
    {
        [Required(ErrorMessage = "Target cannot be null.")]
        [StringLength(2048, MinimumLength = 1, ErrorMessage = "Target must be between 1 and 2048 characters.")]
        public required string Target { get; set; }
    }
}
