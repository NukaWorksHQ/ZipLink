using Shared.Common;
using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs
{
    public class LinkUpdateDto
    {
        [StringLength(2048, MinimumLength = 1, ErrorMessage = "Target must be between 1 and 2048 characters.")]
        [Url(ErrorMessage = "Target must be a valid URL.")]
        public string? Target { get; set; }

        public string? ApiHostName { get; set; }

        public DateTime? ExpirationDate { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "MaxUses must be greater than 0.")]
        public int? MaxUses { get; set; }

        public bool? IsActive { get; set; }

        public bool? TrackingEnabled { get; set; }
    }
}
