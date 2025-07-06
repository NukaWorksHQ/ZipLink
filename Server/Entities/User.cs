using Server.Common;

namespace Server.Entities
{
    public class User : BaseEntity
    {
        public required string Username { get; set; }

        public required string HashedPassword { get; set; }
    }
}
