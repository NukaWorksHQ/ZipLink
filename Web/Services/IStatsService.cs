using Shared.DTOs;

namespace Web.Services
{
    /// <summary>
    /// Interface pour le service de statistiques
    /// </summary>
    public interface IStatsService
    {
        /// <summary>
        /// Récupère les statistiques publiques de l'application
        /// </summary>
        /// <returns>Statistiques publiques</returns>
        Task<PublicStatsDto?> GetPublicStatsAsync();
    }
}
