using Shared.Common;

namespace Shared.Entities
{
    public class LinkStat : BaseEntity
    {
        public required string LinkId { get; set; }

        public required string IpAddress { get; set; }

        public string? UserAgent { get; set; }

        public string? Referer { get; set; }

        public DateTime AccessedAt { get; set; } = DateTime.UtcNow;

        public string? Country { get; set; }

        public string? City { get; set; }

        public Link? Link { get; set; }
    }
}
