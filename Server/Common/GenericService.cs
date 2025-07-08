using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Server.Common
{
    public class GenericService<CTX, ENTITY, C, U> : IGenericService<ENTITY, C, U>
        where CTX : DbContext
        where ENTITY : BaseEntity
        where C : class
        where U : class
    {
        protected readonly CTX _context;
        protected readonly IMapper _mapper;

        public GenericService(CTX context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(_context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(_mapper));
        }

        public async Task<ENTITY> Create(C dto)
        {
            var entity = _mapper.Map<ENTITY>(dto);

            try
            {
                _context.Add(entity);

                await _context.SaveChangesAsync();
                return entity;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (EntityExists(entity.Id))
                {
                    throw new DbUpdateConcurrencyException("A link with the same ID already exists.");
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<bool> Delete(string id)
        {
            var entity = await _context.Set<ENTITY>().FindAsync(id);

            if (entity != null)
            {
                _context.Set<ENTITY>().Remove(entity);

                await _context.SaveChangesAsync();
                return true;
            }
            else
            {
                throw new KeyNotFoundException($"Entity with ID {id} not found.");
            }
        }

        public async Task<ENTITY> Edit(string id, U dto)
        {
            var entity = await _context.Set<ENTITY>().FindAsync(id);

            if (entity == null || id != entity.Id)
            {
                throw new KeyNotFoundException($"Entity with ID {id} not found.");
            }

            _mapper.Map(dto, entity);

            // Resync timestamps
            entity.UpdatedAt = DateTime.UtcNow;

            try
            {
                _context.Set<ENTITY>().Add(entity);
                _context.Update(entity);

                await _context.SaveChangesAsync();
                return entity;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EntityExists(entity.Id))
                {
                    throw new KeyNotFoundException($"Entity with ID {id} not found.");
                }
                else
                {
                    throw;
                }
            }
        }

        public bool EntityExists(string id)
        {
            return _context.Set<ENTITY>().Any(e => e.Id == id);
        }

        public async Task<ENTITY> Get(string id)
        {
            var entity = await _context.Set<ENTITY>()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (entity == null)
            {
                throw new KeyNotFoundException($"Entity with ID {id} not found.");
            }

            return entity;
        }

        public async Task<IEnumerable<ENTITY>> GetAll()
        {
            return await _context.Set<ENTITY>().ToListAsync();
        }
    }
}
