using Server.Common;

namespace Server.Entities
{
    public class User : BaseEntity
    {
        public string Id { get; set; } = GuidUtils.GenerateLittleGuid();

        public required string Username { get; set; }

        public required string HashedPassword { get; set; }
    }
}
