using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Common;
using Server.DTOs;
using Server.Entities;
using Server.Services;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace Server.Controllers
{
    [ApiController]
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
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _userService.GetAll());
        }

        [
            SwaggerOperation(
            Summary = "Get an existing User",
            Description = "Get a user by providing the LinkId and UserId.")
        ]
        [SwaggerResponse(200, "Return the object found")]
        [SwaggerResponse(404, "User not found")]
        [Authorize(Roles = "Standard,Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            try
            {
                var authenticatedUserId = User.FindFirst("UserId")?.Value;
                if (authenticatedUserId == null)
                {
                    return StatusCode(500, "User is invalid");
                }

                var roleValue = User.FindFirst(ClaimTypes.Role)?.Value;
                if (roleValue == null)
                {
                    return StatusCode(500, "User claim Role is missing");
                }

                if (!Enum.TryParse<UserRole>(roleValue, out var userRole))
                {
                    return StatusCode(403, "Invalid role");
                }

                if (authenticatedUserId != id && userRole != UserRole.Admin)
                {
                    return StatusCode(403, "Access denied");
                }

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
            Summary = "Edit an existing User",
            Description = "Edit an user by providing the UserId and update it on the database.")
        ]
        [SwaggerResponse(200, "Return the updated object")]
        [SwaggerResponse(404, "User not found")]
        [SwaggerResponse(500, "DbUpdateConcurrencyException or a server error is thrown")]
        [Authorize(Roles = "Standard, Admin")]
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
    }
}
