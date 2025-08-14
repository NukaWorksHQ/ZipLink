using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Contexts;
using Shared.DTOs;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StatsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Récupère les statistiques publiques de l'application
        /// </summary>
        /// <returns>Statistiques publiques</returns>
        [HttpGet("public")]
        public async Task<ActionResult<PublicStatsDto>> GetPublicStats()
        {
            try
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

                var stats = new PublicStatsDto
                {
                    TotalUsers = totalUsers,
                    TotalLinks = totalLinks,
                    LinksToday = linksToday,
                    LinksThisWeek = linksThisWeek,
                    LinksThisMonth = linksThisMonth,
                    TopApiHosts = topApiHosts,
                    LastUpdated = DateTime.UtcNow
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la récupération des statistiques", error = ex.Message });
            }
        }
    }
}
