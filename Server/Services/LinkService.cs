using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Common;
using Server.Contexts;
using Shared.DTOs;
using Shared.Entities;

namespace Server.Services
{
    public class LinkService : GenericService<AppDbContext, Link, LinkCreateDto, LinkUpdateDto>
    {
        private readonly IApiHostService _apiHostService;

        public LinkService(AppDbContext context, IMapper mapper, IApiHostService apiHostService) : base(context, mapper)
        {
            _apiHostService = apiHostService;
        }

        public override async Task<Link> Create(LinkCreateDto dto)
        {
            // Validate that the ApiHostName is configured
            if (!_apiHostService.IsValidApiHost(dto.ApiHostName))
            {
                throw new ArgumentException($"Invalid API Host: {dto.ApiHostName}");
            }

            return await base.Create(dto);
        }

        public override async Task<Link> Edit(string id, LinkUpdateDto dto)
        {
            // Validate that the ApiHostName is configured
            if (!_apiHostService.IsValidApiHost(dto.ApiHostName))
            {
                throw new ArgumentException($"Invalid API Host: {dto.ApiHostName}");
            }

            return await base.Edit(id, dto);
        }

        public override async Task<Link> Get(string id)
        {
            var entity = await _context.Links
                .Include(e => e.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (entity is null)
                throw new KeyNotFoundException($"Entity with ID {id} not found.");

            return entity;
        }

        public async Task<Link> Get(string id, string userId)
        {
            var entity = await _context.Links
                .Include(e => e.User)
                .FirstOrDefaultAsync(m => m.User.Id == userId && m.Id == id);

            if (entity is null)
                throw new KeyNotFoundException($"Entity with ID {id} not found.");

            return entity;
        }

        public async Task<IEnumerable<Link>> GetAll(string userId)
        {
            return await _context.Links
                .Include(e => e.User)
                .Where(l => l.User.Id == userId)
                .ToListAsync();
        }

        public async Task<bool> Delete(string id, string userId)
        {
            var entity = await _context.Links
                .FirstOrDefaultAsync(e => e.User.Id == userId && e.Id == id);

            if (entity is null)
                throw new KeyNotFoundException($"Entity with ID {id} not found.");

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
    }
}
