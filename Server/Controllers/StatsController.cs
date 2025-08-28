using Microsoft.AspNetCore.Mvc;
using Server.Services;
using Shared.DTOs;

namespace Server.Controllers
{
    /// <summary>
    /// Contrôleur pour la gestion des statistiques
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class StatsController : ControllerBase
    {
        private readonly StatsService _statsService;

        /// <summary>
        /// Initialise une nouvelle instance du contrôleur de statistiques
        /// </summary>
        /// <param name="statsService">Service de statistiques</param>
        public StatsController(StatsService statsService)
        {
            _statsService = statsService;
        }

        /// <summary>
        /// Récupère les statistiques publiques de l'application
        /// </summary>
        /// <returns>Statistiques publiques</returns>
        [HttpGet("public")]
        public async Task<ActionResult> GetPublicStats()
        {
            try
            {
                var stats = await _statsService.GetPublicStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la récupération des statistiques", error = ex.Message });
            }
        }
    }
}
