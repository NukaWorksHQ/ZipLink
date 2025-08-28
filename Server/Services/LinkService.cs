using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Common;
using Server.Contexts;
using Shared.Common;
using Shared.DTOs;
using Shared.Entities;

namespace Server.Services
{
    public class LinkService : GenericService<AppDbContext, Link, LinkCreateDto, LinkUpdateDto>
    {
        private readonly IApiHostService _apiHostService;
        private readonly LinkStatsService _statsService;

        public LinkService(AppDbContext context, IMapper mapper, IApiHostService apiHostService, LinkStatsService statsService) : base(context, mapper)
        {
            _apiHostService = apiHostService;
            _statsService = statsService;
        }

        public override async Task<Link> Create(LinkCreateDto dto)
        {
            // Validate that the ApiHostName is configured
            if (!_apiHostService.IsValidApiHost(dto.ApiHostName))
            {
                throw new ArgumentException($"Invalid API Host: {dto.ApiHostName}");
            }

            // Générer un ID unique court
            var existingIds = await _context.Links.Select(l => l.Id).ToHashSetAsync();
            var uniqueId = GuidUtils.GenerateUniqueShortId(existingIds);

            var entity = _mapper.Map<Link>(dto);
            entity.Id = uniqueId;
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            _context.Add(entity);
            await _context.SaveChangesAsync();

            return entity;
        }

        // Surcharge pour accepter les informations du créateur
        public async Task<Link> CreateWithContext(LinkCreateDto dto, string ipAddress, string? userAgent)
        {
            // Validate that the ApiHostName is configured
            if (!_apiHostService.IsValidApiHost(dto.ApiHostName))
            {
                throw new ArgumentException($"Invalid API Host: {dto.ApiHostName}");
            }

            // Générer un ID unique court
            var existingIds = await _context.Links.Select(l => l.Id).ToHashSetAsync();
            var uniqueId = GuidUtils.GenerateUniqueShortId(existingIds);

            var entity = _mapper.Map<Link>(dto);
            entity.Id = uniqueId;
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            _context.Add(entity);
            await _context.SaveChangesAsync();

            // Créer une statistique initiale de création avec les vraies données du créateur
            try
            {
                await _statsService.RecordAccessAsync(
                    entity.Id, 
                    ipAddress,
                    userAgent ?? "Unknown Browser",
                    "Link Creation"
                );
            }
            catch (Exception)
            {
                // Ignorer les erreurs de logging pour ne pas affecter la création du lien
            }

            return entity;
        }

        public override async Task<Link> Edit(string id, LinkUpdateDto dto)
        {
            // Validate that the ApiHostName is configured if provided
            if (dto.ApiHostName != null && !_apiHostService.IsValidApiHost(dto.ApiHostName))
            {
                throw new ArgumentException($"Invalid API Host: {dto.ApiHostName}");
            }

            // Récupérer l'entité existante
            var entity = await _context.Links
                .Include(e => e.User)
                .Include(e => e.LinkStats)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (entity is null)
                throw new KeyNotFoundException($"Entity with ID {id} not found.");

            // Mapper seulement les propriétés non-null du DTO
            if (dto.Target != null)
                entity.Target = dto.Target;
            if (dto.ApiHostName != null)
                entity.ApiHostName = dto.ApiHostName;
            if (dto.ExpirationDate.HasValue)
                entity.ExpirationDate = dto.ExpirationDate;

            // Gérer MaxUses : traiter les cas spéciaux pour supprimer les limites
            if (dto.MaxUses.HasValue)
            {
                if (dto.MaxUses.Value <= 0 || dto.MaxUses.Value == -1)
                    entity.MaxUses = null; // Supprimer les limites si valeur <= 0 ou -1
                else
                    entity.MaxUses = dto.MaxUses.Value;
            }
            // Note: Si MaxUses n'est pas fourni dans le DTO, on garde la valeur existante
            // Pour supprimer les limites depuis le frontend, envoyer -1 ou 0

            if (dto.IsActive.HasValue)
                entity.IsActive = dto.IsActive.Value;
            if (dto.TrackingEnabled.HasValue)
                entity.TrackingEnabled = dto.TrackingEnabled.Value;

            // Mettre à jour le timestamp
            entity.UpdatedAt = DateTime.UtcNow;

            try
            {
                _context.Update(entity);
                await _context.SaveChangesAsync();
                return entity;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EntityExists(entity.Id))
                    throw new KeyNotFoundException($"Entity with ID {id} not found.");
                else
                    throw;
            }
        }

        public override async Task<Link> Get(string id)
        {
            var entity = await _context.Links
                .Include(e => e.User)
                .Include(e => e.LinkStats)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (entity is null)
                throw new KeyNotFoundException($"Entity with ID {id} not found.");

            return entity;
        }

        public async Task<Link> Get(string id, string userId)
        {
            var entity = await _context.Links
                .Include(e => e.User)
                .Include(e => e.LinkStats)
                .FirstOrDefaultAsync(m => m.User.Id == userId && m.Id == id);

            if (entity is null)
                throw new KeyNotFoundException($"Entity with ID {id} not found.");

            return entity;
        }

        public async Task<IEnumerable<Link>> GetAll(string userId)
        {
            return await _context.Links
                .Include(e => e.User)
                .Include(e => e.LinkStats)
                .Where(l => l.User.Id == userId)
                .ToListAsync();
        }

        public async Task<bool> Delete(string id, string userId)
        {
            var entity = await _context.Links
                .FirstOrDefaultAsync(e => e.User.Id == userId && e.Id == id);

            if (entity is null)
                throw new KeyNotFoundException($"Entity with ID {id} not found.");

            // Supprimer les statistiques associées
            var stats = await _context.LinkStats.Where(s => s.LinkId == id).ToListAsync();
            if (stats.Any())
            {
                _context.LinkStats.RemoveRange(stats);
            }

            _context.Links.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        // New method to get the full URL for a link
        public string GetFullLinkUrl(Link link)
        {
            if (link is null)
                throw new ArgumentNullException(nameof(link));

            var hostUrl = _apiHostService.GetApiHostUrl(link.ApiHostName);
            return $"{hostUrl}/{link.Id}";
        }

        // Nouvelle méthode pour vérifier si un lien peut être utilisé
        public async Task<bool> CanUseLinkAsync(string linkId)
        {
            var link = await _context.Links.FindAsync(linkId);
            if (link == null || !link.IsActive)
                return false;

            // Vérifier l'expiration
            if (link.ExpirationDate.HasValue && link.ExpirationDate.Value < DateTime.UtcNow)
                return false;

            // Vérifier les limites d'utilisation
            if (link.MaxUses.HasValue && link.CurrentUses >= link.MaxUses.Value)
                return false;

            return true;
        }

        // Nouvelle méthode pour incrémenter les utilisations d'un lien
        public async Task<bool> IncrementLinkUsageAsync(string linkId)
        {
            var link = await _context.Links.FindAsync(linkId);
            if (link == null)
                return false;

            link.CurrentUses++;
            await _context.SaveChangesAsync();
            return true;
        }

        // Nouvelle méthode pour obtenir les statistiques d'un lien
        public async Task<LinkStatsDto> GetLinkStatsAsync(string linkId, string userId)
        {
            var link = await _context.Links
                .Include(l => l.LinkStats)
                .FirstOrDefaultAsync(l => l.Id == linkId && l.UserId == userId);

            if (link == null)
                throw new KeyNotFoundException($"Link with ID {linkId} not found.");

            return await _statsService.GetLinkStatsAsync(linkId);
        }

        // Nouvelle méthode pour enregistrer l'accès à un lien
        public async Task<bool> RecordLinkAccessAsync(string linkId, string ipAddress, string? userAgent, string? referer)
        {
            var link = await _context.Links.FindAsync(linkId);
            if (link == null || !link.TrackingEnabled)
                return false;

            return await _statsService.RecordAccessAsync(linkId, ipAddress, userAgent, referer);
        }

        private new bool EntityExists(string id)
        {
            return _context.Links.Any(e => e.Id == id);
        }
    }
}
