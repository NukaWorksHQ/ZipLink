using Shared.Common;

namespace Shared.Entities
{
    public class Link() : BaseEntity
    {
        public required string Target { get; set; }

        public User User { get; set; }
    }
}
