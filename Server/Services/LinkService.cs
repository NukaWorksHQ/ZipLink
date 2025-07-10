using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Common;
using Server.Contexts;
using Server.DTOs;
using Server.Entities;

namespace Server.Services
{
    public class LinkService : GenericService<AppDbContext, Link, LinkCreateDto, LinkUpdateDto>
    {
        public LinkService(AppDbContext context, IMapper mapper) : base(context, mapper)
        {
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
    }
}
