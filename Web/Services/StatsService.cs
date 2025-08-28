using Shared.DTOs;
using System.Net.Http.Json;

namespace Web.Services
{
    /// <summary>
    /// Service pour la gestion des statistiques côté client
    /// </summary>
    public class StatsService : IStatsService
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initialise une nouvelle instance du service de statistiques
        /// </summary>
        /// <param name="httpClient">Client HTTP pour les appels API</param>
        public StatsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Récupère les statistiques publiques de l'application
        /// </summary>
        /// <returns>Statistiques publiques</returns>
        public async Task<PublicStatsDto?> GetPublicStatsAsync()
        {
            var response = await _httpClient.GetAsync("/Stats");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<PublicStatsDto>();
            }
            return null;
        }
    }
}
