using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs
{
    public class LinkStatsDto
    {
        [Required(ErrorMessage = "LinkId is required.")]
        public string LinkId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "TotalClicks must be non-negative.")]
        public int TotalClicks { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "UniqueVisitors must be non-negative.")]
        public int UniqueVisitors { get; set; }

        public DateTime? LastAccessed { get; set; }

        public Dictionary<string, int> CountryCounts { get; set; } = new();

        public List<LinkStatDetailDto> RecentAccesses { get; set; } = new();
    }

    public class LinkStatDetailDto
    {
        [Required(ErrorMessage = "IpAddress is required.")]
        public string IpAddress { get; set; }

        [StringLength(1000, ErrorMessage = "UserAgent cannot exceed 1000 characters.")]
        public string? UserAgent { get; set; }

        [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters.")]
        public string? Country { get; set; }

        public DateTime AccessedAt { get; set; }
    }
}
