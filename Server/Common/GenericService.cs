using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Contexts;
using Server.Entities;

namespace Server.Common
{
    public class GenericService<CTX, ENTITY, C, U> : IGenericService<ENTITY, C, U>
        where CTX : DbContext
        where ENTITY: BaseEntity
        where C : class
        where U : class
    {
        private readonly CTX _context;
        private readonly IMapper _mapper;

        public GenericService(CTX context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(_context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(_mapper));
        }

        public Task<IActionResult> Create(C dto)
        {
            throw new NotImplementedException();
        }

        public async Task<IActionResult> Delete(string id)
        {
            var entity = await _context.Set<ENTITY>().FindAsync(id);

            if (entity != null)
            {
                _context.Set<ENTITY>().Remove(entity);
                
                await _context.SaveChangesAsync();
                return new OkObjectResult(entity);
            } else
            {
                throw new KeyNotFoundException($"Entity with ID {id} not found.");
            }
        }

        public async Task<IActionResult> Edit(string id, U dto)
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
                return new OkObjectResult(entity);
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

        public async Task<IActionResult> Get(string id)
        {
            var entity = await _context.Set<ENTITY>()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (entity == null)
            {
                throw new KeyNotFoundException($"Entity with ID {id} not found.");
            }

            return new OkObjectResult(entity);
        }

        public async Task<IEnumerable<ENTITY>> GetAll()
        {
            return await _context.Set<ENTITY>().ToListAsync();
        }
    }
}
