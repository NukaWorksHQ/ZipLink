namespace Shared.Common
{
    public abstract class BaseEntity
    {
        public string Id { get; set; } = GuidUtils.GenerateShortId();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
