using Server.Common;

namespace Server.Entities
{
    public class Link() : BaseEntity
    {
        public required string Target { get; set; }

        public User User { get; set; }
    }
}
