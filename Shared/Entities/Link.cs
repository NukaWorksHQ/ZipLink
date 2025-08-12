using Shared.Common;

namespace Shared.Entities
{
    public class Link() : BaseEntity
    {
        public required string UserId { get; set; }
        
        public required string Target { get; set; }

        public required string ApiHostName { get; set; }

        public User User { get; set; }
    }
}
