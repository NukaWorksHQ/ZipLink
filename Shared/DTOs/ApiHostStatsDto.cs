using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs
{
    /// <summary>
    /// DTO pour les statistiques d'un hôte API
    /// </summary>
    public class ApiHostStatsDto
    {
        /// <summary>
        /// Nom de l'hôte API
        /// </summary>
        [Required]
        public required string HostName { get; set; }
        
        /// <summary>
        /// Nombre de liens créés pour cet hôte
        /// </summary>
        public int LinkCount { get; set; }
    }
}
