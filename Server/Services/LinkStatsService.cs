using Microsoft.EntityFrameworkCore;
using Server.Contexts;
using Shared.DTOs;
using Shared.Entities;
using System.Text.Json;

namespace Server.Services
{
    public class LinkStatsService
    {
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public LinkStatsService(AppDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<LinkStatsDto> GetLinkStatsAsync(string linkId)
        {
            var linkStats = await _context.LinkStats
                .Where(s => s.LinkId == linkId)
                .ToListAsync();

            var totalClicks = linkStats.Count;
            var uniqueVisitors = linkStats.GroupBy(s => s.IpAddress).Count();
            var lastAccessed = linkStats.OrderByDescending(s => s.AccessedAt).FirstOrDefault()?.AccessedAt;

            var countryCounts = linkStats
                .Where(s => !string.IsNullOrEmpty(s.Country))
                .GroupBy(s => s.Country)
                .ToDictionary(g => g.Key!, g => g.Count())
                .OrderByDescending(kv => kv.Value)
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            var recentAccesses = linkStats
                .OrderByDescending(s => s.AccessedAt)
                .Take(20)
                .Select(s => new LinkStatDetailDto
                {
                    IpAddress = s.IpAddress,
                    UserAgent = s.UserAgent,
                    Country = s.Country,
                    AccessedAt = s.AccessedAt
                })
                .ToList();

            return new LinkStatsDto
            {
                LinkId = linkId,
                TotalClicks = totalClicks,
                UniqueVisitors = uniqueVisitors,
                LastAccessed = lastAccessed,
                CountryCounts = countryCounts,
                RecentAccesses = recentAccesses
            };
        }

        public async Task<bool> RecordAccessAsync(string linkId, string ipAddress, string? userAgent, string? referer)
        {
            try
            {
                // Obtenir les informations de géolocalisation
                var (country, city) = await GetLocationFromIpAsync(ipAddress);

                var linkStat = new LinkStat
                {
                    Id = Guid.NewGuid().ToString(),
                    LinkId = linkId,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    Referer = referer,
                    Country = country,
                    City = city,
                    AccessedAt = DateTime.UtcNow
                };

                _context.LinkStats.Add(linkStat);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                // En cas d'erreur, on enregistre quand même l'accès sans géolocalisation
                var linkStat = new LinkStat
                {
                    Id = Guid.NewGuid().ToString(),
                    LinkId = linkId,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    Referer = referer,
                    AccessedAt = DateTime.UtcNow
                };

                _context.LinkStats.Add(linkStat);
                await _context.SaveChangesAsync();
                return true;
            }
        }

        private async Task<(string? country, string? city)> GetLocationFromIpAsync(string ipAddress)
        {
            try
            {
                // Ignorer les IPs locales
                if (IsLocalIpAddress(ipAddress))
                    return (null, null);

                // Utiliser un service gratuit de géolocalisation IP avec HttpClient sans cache DNS
                using var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(10);
                httpClient.DefaultRequestHeaders.Add("User-Agent", "ZipLink-GeoResolver/1.0");

                var response = await httpClient.GetStringAsync($"http://ip-api.com/json/{ipAddress}?fields=country,city");
                var locationData = JsonSerializer.Deserialize<Dictionary<string, object>>(response);

                if (locationData != null)
                {
                    var country = locationData.ContainsKey("country") ? locationData["country"]?.ToString() : null;
                    var city = locationData.ContainsKey("city") ? locationData["city"]?.ToString() : null;
                    return (country, city);
                }
            }
            catch (Exception)
            {
                // En cas d'erreur, on retourne null pour les deux valeurs
            }

            return (null, null);
        }

        private static bool IsLocalIpAddress(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
                return true;

            // Vérifier seulement les vraies IPs locales, permettre plus d'IPs pour la géolocalisation
            return ipAddress == "127.0.0.1" ||
                   ipAddress == "::1" ||
                   ipAddress == "localhost" ||
                   ipAddress == "unknown";
        }

        public async Task<int> GetTotalClicksAsync(string linkId)
        {
            return await _context.LinkStats.CountAsync(s => s.LinkId == linkId);
        }

        public async Task<int> GetUniqueVisitorsAsync(string linkId)
        {
            return await _context.LinkStats
                .Where(s => s.LinkId == linkId)
                .GroupBy(s => s.IpAddress)
                .CountAsync();
        }

        public async Task<DateTime?> GetLastAccessAsync(string linkId)
        {
            var lastAccess = await _context.LinkStats
                .Where(s => s.LinkId == linkId)
                .OrderByDescending(s => s.AccessedAt)
                .FirstOrDefaultAsync();

            return lastAccess?.AccessedAt;
        }
    }
}
