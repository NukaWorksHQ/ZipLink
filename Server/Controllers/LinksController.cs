using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Contexts;
using Server.DTOs;
using Server.Entities;
using Swashbuckle.AspNetCore.Annotations;

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

        [
            SwaggerOperation(
            Summary = "Gets all links created by the user",
            Description = "GetAll will return an array of links that belongs to the right user.")
        ]
        [SwaggerResponse(200, "Return an array of links")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.Links.ToListAsync());
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
            var link = await _context.Links
                .FirstOrDefaultAsync(m => m.Id == id);

            if (link == null)
            {
                return NotFound();
            }

            return Ok(link);
        }

        [
            SwaggerOperation(
            Summary = "Create a new Link",
            Description = "Create a new Link by the user and store it to the database.")
        ]
        [SwaggerResponse(200, "Return the created object")]
        [SwaggerResponse(409, "A link with the same ID already exists.")]
        [SwaggerResponse(500, "DbUpdateConcurrencyException or a server error is thrown")]
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

        [
            SwaggerOperation(
            Summary = "Edit an existing Link",
            Description = "Edit a link by providing the LinkId and UserId and update it on the database.")
        ]
        [SwaggerResponse(200, "Return the updated object")]
        [SwaggerResponse(404, "Link not found")]
        [SwaggerResponse(500, "DbUpdateConcurrencyException or a server error is thrown")]
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

        [
            SwaggerOperation(
            Summary = "Delete a Link",
            Description = "Delete a link by providing the LinkId and UserId and delete it on the database.")
        ]
        [SwaggerResponse(200, "Return 200 code if success")]
        [SwaggerResponse(404, "Link not found")]
        [HttpDelete]
        public async Task<IActionResult> Delete(string id)
        {
            var link = await _context.Links.FindAsync(id);
         
            if (link != null)
            {
                _context.Links.Remove(link);
            } else
            {
                return NotFound();
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
