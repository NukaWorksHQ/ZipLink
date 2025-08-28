using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs
{
    /// <summary>
    /// DTO pour les statistiques publiques de l'application
    /// </summary>
    public class PublicStatsDto
    {
        /// <summary>
        /// Nombre total d'utilisateurs
        /// </summary>
        public int TotalUsers { get; set; }
        
        /// <summary>
        /// Nombre total de liens
        /// </summary>
        public int TotalLinks { get; set; }
        
        /// <summary>
        /// Nombre de liens créés aujourd'hui
        /// </summary>
        public int LinksToday { get; set; }
        
        /// <summary>
        /// Nombre de liens créés cette semaine
        /// </summary>
        public int LinksThisWeek { get; set; }
        
        /// <summary>
        /// Nombre de liens créés ce mois
        /// </summary>
        public int LinksThisMonth { get; set; }
        
        /// <summary>
        /// Top 5 des hôtes API les plus utilisés
        /// </summary>
        public List<ApiHostStatsDto> TopApiHosts { get; set; } = new();
        
        /// <summary>
        /// Date de dernière mise à jour des statistiques
        /// </summary>
        public DateTime LastUpdated { get; set; }
    }
}
