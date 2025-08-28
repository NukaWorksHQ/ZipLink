using Shared.Common;

namespace Shared.Entities
{
    public class Link() : BaseEntity
    {
        public required string UserId { get; set; }
        
        public required string Target { get; set; }

        public required string ApiHostName { get; set; }

        // Nouvelles propriétés de personnalisation
        public DateTime? ExpirationDate { get; set; }

        public int? MaxUses { get; set; }

        public int CurrentUses { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public bool TrackingEnabled { get; set; } = true;

        // Propriétés de navigation
        public User User { get; set; }
        public ICollection<LinkStat> LinkStats { get; set; } = new List<LinkStat>();
    }
}
