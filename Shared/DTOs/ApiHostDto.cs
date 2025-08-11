using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs
{
    public class ApiHostDto
    {
        [Required]
        public required string Name { get; set; }
        
        [Required]
        public required string Url { get; set; }
    }
}
