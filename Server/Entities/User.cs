using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Server.Common;
using System.Text.Json.Serialization;

namespace Server.Entities
{
    [Index(nameof(Username), IsUnique = true)]
    public class User : BaseEntity
    {
        public required string Username { get; set; }

        public required UserRole Role { get; set; }

        [JsonIgnore]
        public string HashedPassword { get; set; }

        public ICollection<Link> Links { get; }
    }
}
