using Shared.Common;
using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs
{
    public class LinkUpdateDto
    {
        [Required(ErrorMessage = "Target cannot be null.")]
        [StringLength(2048, MinimumLength = 1, ErrorMessage = "Target must be between 1 and 2048 characters.")]
        
        [Url(ErrorMessage = "Target must be a valid URL.")]
        public required string Target { get; set; }

        public required string ApiHostName { get; set; }
    }
}
