using Shared.DTOs;
using System.Text.Json;

namespace Web.Services
{
    public interface IStatsService
    {
        Task<PublicStatsDto?> GetPublicStatsAsync();
    }

    public class StatsService : IStatsService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public StatsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<PublicStatsDto?> GetPublicStatsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/stats/public");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<PublicStatsDto>(json, _jsonOptions);
                }
                
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération des statistiques: {ex.Message}");
                return null;
            }
        }
    }
}
