using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Common;
using Shared.DTOs;
using Server.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class LinksController : ControllerBase
    {
        private readonly LinkService _linkService;
        private readonly UserAccessValidator _userAccessValidator;

        public LinksController(LinkService linkService, UserAccessValidator userAccessValidator)
        {
            _linkService = linkService;
            _userAccessValidator = userAccessValidator;
        }

        [
            SwaggerOperation(
            Summary = "Gets all links created by the user",
            Description = "GetAll will return an array of links that belongs to the right user.")
        ]
        [SwaggerResponse(200, "Return an array of links")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userClaim = _userAccessValidator.GetUserClaimStatus(User);
            _userAccessValidator.ValidateUser(User, userClaim.UserId, needsAdminPrivileges: false);

            if (userClaim.Role is not UserRole.Admin)
            {
                return Ok(await _linkService.GetAll(userClaim.UserId));
            }
            return Ok(await _linkService.GetAll());
        }

        [
            SwaggerOperation(
            Summary = "Get an existing Link",
            Description = "Get a link by providing the LinkId and UserId.")
        ]
        [SwaggerResponse(200, "Return the object found")]
        [SwaggerResponse(404, "Link not found")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            try
            {
                var userClaim = _userAccessValidator.GetUserClaimStatus(User);
                _userAccessValidator.ValidateUser(User, userClaim.UserId, needsAdminPrivileges: false);

                if (userClaim.Role is UserRole.Admin)
                    return Ok(await _linkService.Get(id));
                  else
                    return Ok(await _linkService.Get(id, userClaim.UserId));
            } catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [
            SwaggerOperation(
            Summary = "Create a new Link with customization options",
            Description = "Create a new Link with expiration date, usage limits, and tracking settings.")
        ]
        [SwaggerResponse(200, "Return the created object")]
        [SwaggerResponse(400, "Invalid input data")]
        [SwaggerResponse(409, "A link with the same ID already exists.")]
        [SwaggerResponse(500, "DbUpdateConcurrencyException or a server error is thrown")]
        [HttpPut]
        public async Task<IActionResult> Create([FromBody] LinkCreateDto dto)
        {
            try
            {
                // Validation des données d'entrée
                if (dto.ExpirationDate.HasValue && dto.ExpirationDate.Value <= DateTime.UtcNow)
                {
                    return BadRequest("Expiration date must be in the future.");
                }

                // MaxUses : null/vide = illimité, sinon doit être > 0
                if (dto.MaxUses.HasValue && dto.MaxUses.Value <= 0)
                {
                    // Convertir les valeurs <= 0 en null pour illimité
                    dto.MaxUses = null;
                }

                // Capturer les vraies informations du créateur pour les statistiques
                var ipAddress = GetClientIpAddress();
                var userAgent = Request.Headers["User-Agent"].FirstOrDefault();

                var userClaim = _userAccessValidator.GetUserClaimStatus(User);
                _userAccessValidator.ValidateUser(User, userClaim.UserId, needsAdminPrivileges: false);
                dto.UserId = userClaim.UserId;

                var link = await _linkService.CreateWithContext(dto, ipAddress, userAgent);
                return await Get(link.Id);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("A link with the same ID already exists.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [
            SwaggerOperation(
            Summary = "Edit an existing Link configuration",
            Description = "Edit a link's customization settings including expiration, limits, and tracking.")
        ]
        [SwaggerResponse(200, "Return the updated object")]
        [SwaggerResponse(400, "Invalid input data")]
        [SwaggerResponse(404, "Link not found")]
        [SwaggerResponse(500, "DbUpdateConcurrencyException or a server error is thrown")]
        [HttpPost("{id}")]
        public async Task<IActionResult> Edit(string id, [FromBody] LinkUpdateDto dto)
        {
            try
            {
                var userClaim = _userAccessValidator.GetUserClaimStatus(User);
                _userAccessValidator.ValidateUser(User, userClaim.UserId, needsAdminPrivileges: false);

                // Validation des données d'entrée
                if (dto.ExpirationDate.HasValue && dto.ExpirationDate.Value <= DateTime.UtcNow)
                {
                    return BadRequest("Expiration date must be in the future.");
                }

                // MaxUses : null/vide = illimité, sinon doit être > 0
                if (dto.MaxUses.HasValue && dto.MaxUses.Value <= 0)
                {
                    // Convertir les valeurs <= 0 en null pour illimité
                    dto.MaxUses = null;
                }

                if (userClaim.Role == UserRole.Admin)
                    await _linkService.Edit(id, dto);
                else
                {
                    var link = await _linkService.Get(id, userClaim.UserId);
                    if (link is null)
                        return BadRequest("Failed to edit the Link, Permission Denied");

                    await _linkService.Edit(id, dto);
                }

                return await Get(id);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [
            SwaggerOperation(
            Summary = "Get link statistics",
            Description = "Get detailed statistics for a specific link including clicks, visitors, and geographical data.")
        ]
        [SwaggerResponse(200, "Return link statistics")]
        [SwaggerResponse(404, "Link not found")]
        [SwaggerResponse(403, "Permission denied")]
        [HttpGet("{id}/stats")]
        public async Task<IActionResult> GetStats(string id)
        {
            try
            {
                var userClaim = _userAccessValidator.GetUserClaimStatus(User);
                _userAccessValidator.ValidateUser(User, userClaim.UserId, needsAdminPrivileges: false);

                var stats = await _linkService.GetLinkStatsAsync(id, userClaim.UserId);
                return Ok(stats);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [
            SwaggerOperation(
            Summary = "Record link access",
            Description = "Record an access to a link for statistics tracking. This endpoint is typically called during link redirection.")
        ]
        [SwaggerResponse(200, "Access recorded successfully")]
        [SwaggerResponse(404, "Link not found")]
        [SwaggerResponse(403, "Link is disabled or expired")]
        [AllowAnonymous]
        [HttpPost("{id}/access")]
        public async Task<IActionResult> RecordAccess(string id)
        {
            try
            {
                // Vérifier si le lien peut être utilisé
                if (!await _linkService.CanUseLinkAsync(id))
                {
                    return Forbid("Link is expired, disabled, or has reached its usage limit.");
                }

                // Obtenir les informations de la requête
                var ipAddress = GetClientIpAddress();
                var userAgent = Request.Headers["User-Agent"].FirstOrDefault();
                var referer = Request.Headers["Referer"].FirstOrDefault();

                // Enregistrer l'accès
                await _linkService.RecordLinkAccessAsync(id, ipAddress, userAgent, referer);

                // Incrémenter le compteur d'utilisation
                await _linkService.IncrementLinkUsageAsync(id);

                return Ok(new { message = "Access recorded successfully" });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [
            SwaggerOperation(
            Summary = "Check if link can be used",
            Description = "Check if a link is active, not expired, and within usage limits.")
        ]
        [SwaggerResponse(200, "Return link availability status")]
        [SwaggerResponse(404, "Link not found")]
        [AllowAnonymous]
        [HttpGet("{id}/canuse")]
        public async Task<IActionResult> CanUseLink(string id)
        {
            try
            {
                var canUse = await _linkService.CanUseLinkAsync(id);
                return Ok(new { canUse = canUse });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [
            SwaggerOperation(
            Summary = "Delete a Link",
            Description = "Delete a link by providing the LinkId and UserId and delete it on the database.")
        ]
        [SwaggerResponse(200, "Return 200 code if success")]
        [SwaggerResponse(404, "Link not found")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var userClaim = _userAccessValidator.GetUserClaimStatus(User);
                _userAccessValidator.ValidateUser(User, userClaim.UserId, needsAdminPrivileges: false);

                if (userClaim.Role == UserRole.Admin)
                    return Ok(await _linkService.Delete(id));
                else
                    return Ok(await _linkService.Delete(id, userClaim.UserId));
            } catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [
            SwaggerOperation(
            Summary = "Create a new Link quickly with query parameters",
            Description = "Create a new Link using query parameters for easy curl API calls. Perfect for quick link creation via command line.")
        ]
        [SwaggerResponse(200, "Return the created object")]
        [SwaggerResponse(400, "Invalid input data")]
        [SwaggerResponse(409, "A link with the same ID already exists.")]
        [SwaggerResponse(500, "DbUpdateConcurrencyException or a server error is thrown")]
        [HttpPost("CreateLink")]
        public async Task<IActionResult> CreateLink(
            [FromQuery] string target,
            [FromQuery] string token,
            [FromQuery] string? apiHostName = null,
            [FromQuery] string? name = null,
            [FromQuery] DateTime? expirationDate = null,
            [FromQuery] int? maxUses = null,
            [FromQuery] bool trackingEnabled = true)
        {
            // Validate required parameters
            if (string.IsNullOrWhiteSpace(target))
            {
                return BadRequest("Target URL is required.");
            }

            // Validate URL format
            if (!Uri.TryCreate(target, UriKind.Absolute, out var validUri) || 
                (validUri.Scheme != Uri.UriSchemeHttp && validUri.Scheme != Uri.UriSchemeHttps))
            {
                return BadRequest("Target must be a valid HTTP or HTTPS URL.");
            }

            // Set default API host if not provided
            if (string.IsNullOrWhiteSpace(apiHostName))
            {
                return BadRequest("ApiHostName is required. Please specify the API host to use.");
            }

            // Get user information for DTO creation
            var userClaim = _userAccessValidator.GetUserClaimStatus(User);
            _userAccessValidator.ValidateUser(User, userClaim.UserId, needsAdminPrivileges: false);

            // Create the DTO using the same structure as the traditional PUT endpoint
            var dto = new LinkCreateDto
            {
                UserId = userClaim.UserId,
                Target = target,
                ApiHostName = apiHostName,
                ExpirationDate = expirationDate,
                MaxUses = maxUses,
                TrackingEnabled = trackingEnabled
            };

            // Reuse the existing Create logic
            return await Create(dto);
        }

        private string GetClientIpAddress()
        {
            // Essayer de récupérer l'IP à travers différents headers
            var forwarded = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwarded))
            {
                var ips = forwarded.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (ips.Length > 0)
                {
                    return ips[0].Trim();
                }
            }

            var realIp = Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            // Fallback sur l'IP de connexion directe
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}
