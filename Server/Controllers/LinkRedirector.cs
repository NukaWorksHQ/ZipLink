using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Common;
using Shared.DTOs;
using Server.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace Server.Controllers
{
    [ApiController]
    [Route("/")]
    public class LinkRedirector : ControllerBase
    {
        private readonly LinkService _linkService;
        private readonly IApiHostService _apiHostService;

        public LinkRedirector(LinkService linkService, IApiHostService apiHostService)
        {
            _linkService = linkService;
            _apiHostService = apiHostService;
        }

        [
            SwaggerOperation(
            Summary = "Redirect to the Target with tracking and validation",
            Description = "Get a link by providing the LinkId, validate host and link availability, track usage, and redirect to the Target link.")
        ]
        [SwaggerResponse(301, "Redirect to the target")]
        [SwaggerResponse(404, "Link not found")]
        [SwaggerResponse(403, "Link is disabled, expired, or limit reached")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            try
            {
                // Récupérer le lien d'abord pour vérifier s'il existe
                var link = await _linkService.Get(id);
                if (link == null || string.IsNullOrEmpty(link.Target))
                    return StatusCode(404, "Link not found");

                // Validate that the origin host matches one of our configured hosts
                var requestHost = Request.Host.Value;
                var validHosts = _apiHostService.GetApiHosts();
                var isValidHost = validHosts.Any(host => 
                {
                    try
                    {
                        var hostUri = new Uri(host.Url);
                        return hostUri.Host.Equals(requestHost, StringComparison.OrdinalIgnoreCase) ||
                               $"{hostUri.Host}:{hostUri.Port}".Equals(requestHost, StringComparison.OrdinalIgnoreCase) ||
                               requestHost.Contains("localhost"); // Support pour développement local
                    }
                    catch
                    {
                        return false;
                    }
                });

                if (!isValidHost)
                {
                    return StatusCode(404, "Link not found");
                }

                // Vérifier si le lien peut être utilisé (nouvelles validations)
                if (!await _linkService.CanUseLinkAsync(id))
                {
                    return StatusCode(403, "Ce lien a expiré, est désactivé, ou a atteint sa limite d'utilisation.");
                }

                // Enregistrer l'accès si le tracking est activé (nouvelle fonctionnalité)
                if (link.TrackingEnabled)
                {
                    var ipAddress = GetClientIpAddress();
                    var userAgent = Request.Headers["User-Agent"].FirstOrDefault();
                    var referer = Request.Headers["Referer"].FirstOrDefault();

                    await _linkService.RecordLinkAccessAsync(id, ipAddress, userAgent, referer);
                }

                // Incrémenter le compteur d'utilisation (nouvelle fonctionnalité)
                await _linkService.IncrementLinkUsageAsync(id);

                // Rediriger vers l'URL cible (RedirectPermanent pour SEO)
                return RedirectPermanent(link.Target);
            } 
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in LinkRedirector: {ex.Message}");
                return NotFound();
            }
        }

        [
            SwaggerOperation(
            Summary = "Check if link can be used",
            Description = "Check if a link is active, not expired, within usage limits, and on valid host.")
        ]
        [SwaggerResponse(200, "Return link availability status")]
        [SwaggerResponse(404, "Link not found or invalid host")]
        [HttpGet("{id}/status")]
        public async Task<IActionResult> CheckLinkStatus(string id)
        {
            try
            {
                // Récupérer le lien d'abord
                var link = await _linkService.Get(id);
                if (link == null)
                    return NotFound();

                // Validate host
                var requestHost = Request.Host.Value;
                var validHosts = _apiHostService.GetApiHosts();
                var isValidHost = validHosts.Any(host => 
                {
                    try
                    {
                        var hostUri = new Uri(host.Url);
                        return hostUri.Host.Equals(requestHost, StringComparison.OrdinalIgnoreCase) ||
                               $"{hostUri.Host}:{hostUri.Port}".Equals(requestHost, StringComparison.OrdinalIgnoreCase) ||
                               requestHost.Contains("localhost"); // Support pour développement local
                    }
                    catch
                    {
                        return false;
                    }
                });

                if (!isValidHost)
                {
                    return NotFound();
                }

                var canUse = await _linkService.CanUseLinkAsync(id);

                return Ok(new { 
                    canUse = canUse,
                    isActive = link.IsActive,
                    isExpired = link.ExpirationDate.HasValue && link.ExpirationDate.Value < DateTime.UtcNow,
                    usageCount = link.CurrentUses,
                    maxUses = link.MaxUses,
                    trackingEnabled = link.TrackingEnabled
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CheckLinkStatus: {ex.Message}");
                return NotFound();
            }
        }

        private string GetClientIpAddress()
        {
            // Essayer de récupérer l'IP à travers différents headers (pour proxies et load balancers)
            var forwarded = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwarded))
            {
                var ips = forwarded.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (ips.Length > 0)
                {
                    var cleanIp = ips[0].Trim();
                    if (!string.IsNullOrEmpty(cleanIp))
                        return cleanIp;
                }
            }

            var realIp = Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp.Trim();
            }

            var cfConnectingIp = Request.Headers["CF-Connecting-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(cfConnectingIp))
            {
                return cfConnectingIp.Trim();
            }

            // Récupérer l'IP de connexion directe
            var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (!string.IsNullOrEmpty(remoteIp))
            {
                // Convertir IPv6 loopback en IPv4 loopback pour cohérence
                if (remoteIp == "::1")
                    return "127.0.0.1";

                // Extraire l'IPv4 si c'est mappé en IPv6
                if (remoteIp.StartsWith("::ffff:"))
                    return remoteIp.Substring(7);

                return remoteIp;
            }

            // Essayer de récupérer l'IP publique via des services externes pour développement local
            try
            {
                // Pour le développement local, on peut essayer de détecter l'IP publique
                var localIp = HttpContext.Connection.LocalIpAddress?.ToString();
                if (!string.IsNullOrEmpty(localIp) && !localIp.StartsWith("127.") && !localIp.StartsWith("::"))
                {
                    return localIp;
                }
            }
            catch
            {
                // Ignorer les erreurs
            }

            return "127.0.0.1"; // Fallback par défaut
        }
    }
}
