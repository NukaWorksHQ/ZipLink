using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.DTOs;
using Server.Entities;
using Server.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace Server.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
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
            // TODO: admin route
            return Ok(await _userService.GetAll());
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
            try
            {
                var user = await _userService.Get(id);
                return Ok(user);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
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
            try
            {
                User user = await _userService.Create(dto);
                HttpContext.Response.Cookies.Append("UserId", user.Id);
                return Ok(user);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("A user with the same ID already exists.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
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
            try
            {
                var user = await _userService.Edit(id, dto);
                return Ok(user);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "An error occurred while updating the user.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
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
            try
            {
                var user = await _userService.Delete(id);
                return Ok(user);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
