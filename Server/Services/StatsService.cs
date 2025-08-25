using Microsoft.EntityFrameworkCore;
using Server.Contexts;
using Shared.DTOs;

namespace Server.Services
{
    /// <summary>
    /// Service pour la gestion des statistiques publiques de l'application
    /// </summary>
    public class StatsService
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Initialise une nouvelle instance du service de statistiques
        /// </summary>
        /// <param name="context">Contexte de base de données</param>
        public StatsService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Récupère les statistiques publiques de l'application
        /// </summary>
        /// <returns>Statistiques publiques</returns>
        public async Task<PublicStatsDto> GetPublicStatsAsync()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalLinks = await _context.Links.CountAsync();
            
            var linksToday = await _context.Links
                .Where(l => l.CreatedAt.Date == DateTime.Today)
                .CountAsync();

            var linksThisWeek = await _context.Links
                .Where(l => l.CreatedAt >= DateTime.Today.AddDays(-7))
                .CountAsync();

            var linksThisMonth = await _context.Links
                .Where(l => l.CreatedAt >= DateTime.Today.AddDays(-30))
                .CountAsync();

            var topApiHosts = await _context.Links
                .GroupBy(l => l.ApiHostName)
                .Select(g => new ApiHostStatsDto
                {
                    HostName = g.Key,
                    LinkCount = g.Count()
                })
                .OrderByDescending(x => x.LinkCount)
                .Take(5)
                .ToListAsync();

            return new PublicStatsDto
            {
                TotalUsers = totalUsers,
                TotalLinks = totalLinks,
                LinksToday = linksToday,
                LinksThisWeek = linksThisWeek,
                LinksThisMonth = linksThisMonth,
                TopApiHosts = topApiHosts,
                LastUpdated = DateTime.UtcNow
            };
        }
    }
}
