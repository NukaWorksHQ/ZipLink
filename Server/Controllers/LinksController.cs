using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Contexts;
using Server.DTOs;
using Server.Entities;

namespace Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LinksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LinksController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.Links.ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var link = await _context.Links
                .FirstOrDefaultAsync(m => m.Id == id);

            if (link == null)
            {
                return NotFound();
            }

            return Ok(link);
        }

        [HttpPut]
        public async Task<IActionResult> Create([FromBody] LinkCreateDto dto)
        {
            var link = new Link
            {
                UserId = dto.UserId, // TODO: Validate UserId exists
                Target = dto.Target
            };
            
            try
            {
                _context.Add(link);

                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(Get), new { id = link.Id }, link);
            } catch (DbUpdateConcurrencyException)
            {
                if (LinkExists(link.Id))
                {
                    return Conflict("A link with the same ID already exists.");
                }
                else
                {
                    throw;
                }
            }
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> Edit(string id, [FromBody] LinkUpdateDto dto)
        {
            var link = await _context.Links.FindAsync(id);
            if (link == null || id != link.Id)
            {
                return NotFound();
            }
            
            link.Target = dto.Target;

            // Resync timestamps
            link.UpdatedAt = DateTime.UtcNow;

            try
            {
                _context.Links.Add(link);
                _context.Update(link);

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LinkExists(link.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction(nameof(Get), new { id = link.Id });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string id)
        {
            var link = await _context.Links.FindAsync(id);
         
            if (link != null)
            {
                _context.Links.Remove(link);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LinkExists(string id)
        {
            return _context.Links.Any(e => e.Id == id);
        }
    }
}
