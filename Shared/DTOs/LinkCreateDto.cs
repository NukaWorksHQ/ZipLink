using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs
{
    public class LinkCreateDto
    {
        [Required(ErrorMessage = "UserId cannot be null.")]
        [StringLength(32, MinimumLength = 4, ErrorMessage = "Username should be between 4 and 32 characters.")]
        public required string UserId { get; set; }

        [Required(ErrorMessage = "Target cannot be null.")]
        [StringLength(2048, MinimumLength = 1, ErrorMessage = "Target must be between 1 and 2048 characters.")]
        [Url(ErrorMessage = "Target must be a valid URL.")]
        public required string Target { get; set; }

        [Required(ErrorMessage = "ApiHostName is required.")]
        public required string ApiHostName { get; set; }
    }
}
