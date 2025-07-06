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
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [
            SwaggerOperation(
            Summary = "(admin) Gets all users created",
            Description = "GetAll will return an array of users that belongs to the right user.")
        ]
        [SwaggerResponse(200, "Return an array of users")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.Users.ToListAsync());
        }

        [
            SwaggerOperation(
            Summary = "Get an existing User",
            Description = "Get a user by providing the LinkId and UserId.")
        ]
        [SwaggerResponse(200, "Return the object found")]
        [SwaggerResponse(404, "User not found")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [
            SwaggerOperation(
            Summary = "Create a new User",
            Description = "Create a new User by the user and store it to the database.")
        ]
        [SwaggerResponse(200, "Return the created object")]
        [SwaggerResponse(409, "A user with the same ID already exists.")]
        [SwaggerResponse(500, "DbUpdateConcurrencyException or a server error is thrown")]
        [HttpPut]
        public async Task<IActionResult> Create([FromBody] UserCreateDto dto)
        {
            var user = new User
            {
                Username = dto.Username,
                HashedPassword = dto.HashedPassword,
            };
            
            try
            {
                _context.Add(user);

                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
            } catch (DbUpdateConcurrencyException)
            {
                if (UserExists(user.Id))
                {
                    return Conflict("A user with the same ID already exists.");
                }
                else
                {
                    throw;
                }
            }
        }

        [
            SwaggerOperation(
            Summary = "Edit an existing User",
            Description = "Edit an user by providing the UserId and update it on the database.")
        ]
        [SwaggerResponse(200, "Return the updated object")]
        [SwaggerResponse(404, "User not found")]
        [SwaggerResponse(500, "DbUpdateConcurrencyException or a server error is thrown")]
        [HttpPost("{id}")]
        public async Task<IActionResult> Edit(string id, [FromBody] UserUpdateDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null || id != user.Id)
            {
                return NotFound();
            }
            
            user.Username = dto.Username;
            user.HashedPassword = dto.HashedPassword;

            // Resync timestamps
            user.UpdatedAt = DateTime.UtcNow;

            try
            {
                _context.Users.Add(user);
                _context.Update(user);

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(user.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction(nameof(Get), new { id = user.Id });
        }

        [
            SwaggerOperation(
            Summary = "Delete a User",
            Description = "Delete a user by providing the LinkId and UserId and delete it on the database.")
        ]
        [SwaggerResponse(200, "Return 200 code if success")]
        [SwaggerResponse(404, "User not found")]
        [HttpDelete]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _context.Users.FindAsync(id);
         
            if (user != null)
            {
                _context.Users.Remove(user);
            } else
            {
                return NotFound();
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
