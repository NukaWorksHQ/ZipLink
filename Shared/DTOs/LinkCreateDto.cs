using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs
{
    public class LinkCreateDto
    {
        [Required(ErrorMessage = "Target cannot be null.")]
        [StringLength(2048, MinimumLength = 1, ErrorMessage = "Target must be between 1 and 2048 characters.")]
        public required string Target { get; set; }
    }
}
