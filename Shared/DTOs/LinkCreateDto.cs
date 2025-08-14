using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs
{
    public class LinkCreateDto
    {
        [Required(ErrorMessage = "UserId cannot be null.")]
        [StringLength(8, MinimumLength = 3, ErrorMessage = "UserId should be between 3 and 8 characters.")]
        public required string UserId { get; set; }

        [Required(ErrorMessage = "Target cannot be null.")]
        [StringLength(2048, MinimumLength = 1, ErrorMessage = "Target must be between 1 and 2048 characters.")]
        [Url(ErrorMessage = "Target must be a valid URL.")]
        public required string Target { get; set; }

        [Required(ErrorMessage = "ApiHostName is required.")]
        public required string ApiHostName { get; set; }

        // Nouvelles propriétés de personnalisation
        public DateTime? ExpirationDate { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "MaxUses must be greater than 0.")]
        public int? MaxUses { get; set; }

        public bool TrackingEnabled { get; set; } = true;
    }
}
