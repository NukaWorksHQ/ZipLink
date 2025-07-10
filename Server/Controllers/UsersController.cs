using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Common;
using Server.DTOs;
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
        private readonly UserAccessValidator _userAccessValidator;
        public UsersController(UserService userService, UserAccessValidator userAccessValidator)
        {
            _userService = userService;
            _userAccessValidator = userAccessValidator;
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
        [Authorize(Roles = "Standard, Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            try
            {
                var userClaim = _userAccessValidator.GetUserClaimStatus(User);
                _userAccessValidator.ValidateUser(User, id, needsAdminPrivileges: false);

                if (userClaim.Role is UserRole.Admin || id == userClaim.UserId)
                    return Ok(await _userService.Get(id));
                else
                    return StatusCode(403, "Permission denied");
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
                _userAccessValidator.ValidateUser(User, id, needsAdminPrivileges: true);

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
