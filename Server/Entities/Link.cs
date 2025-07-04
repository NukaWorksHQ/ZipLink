using Server.Common;

namespace Server.Entities
{
    public class Link() : BaseEntity
    {
        public string Id { get; set; } = GuidUtils.GenerateLittleGuid();

        public required string UserId { get; set; }
        
        public required string Target { get; set; }
    }
}
