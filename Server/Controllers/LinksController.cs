using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Common;
using Server.DTOs;
using Server.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class LinksController : ControllerBase
    {
        private readonly LinkService _linkService;
        private readonly UserAccessValidator _userAccessValidator;

        public LinksController(LinkService linkService, UserAccessValidator userAccessValidator)
        {
            _linkService = linkService;
            _userAccessValidator = userAccessValidator;
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
            var userClaim = _userAccessValidator.GetUserClaimStatus(User);
            _userAccessValidator.ValidateUser(User, userClaim.UserId, needsAdminPrivileges: false);

            if (userClaim.Role is not UserRole.Admin)
            {
                return Ok(await _linkService.GetAll(userClaim.UserId));
            }
            return Ok(await _linkService.GetAll());
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
            try
            {
                var userClaim = _userAccessValidator.GetUserClaimStatus(User);
                _userAccessValidator.ValidateUser(User, userClaim.UserId, needsAdminPrivileges: false);

                if (userClaim.Role is UserRole.Admin)
                    return Ok(await _linkService.Get(id));
                  else
                    return Ok(await _linkService.Get(id, userClaim.UserId));
            } catch (KeyNotFoundException)
            {
                return NotFound();
            }
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
            try
            {
                var user = await _linkService.Create(dto);
                return Ok(user);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("A link with the same ID already exists.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
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
            try
            {
                var userClaim = _userAccessValidator.GetUserClaimStatus(User);
                _userAccessValidator.ValidateUser(User, userClaim.UserId, needsAdminPrivileges: false);

                if (userClaim.Role == UserRole.Admin)
                    await _linkService.Edit(id, dto);
                else
                {
                    var link = await _linkService.Get(id, userClaim.UserId);
                    if (link is null)
                        return BadRequest("Failed to edit the Link, Permission Denied");

                    await _linkService.Edit(id, dto);
                }

                return RedirectToAction(nameof(Get), id);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
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
            try
            {
                var userClaim = _userAccessValidator.GetUserClaimStatus(User);
                _userAccessValidator.ValidateUser(User, userClaim.UserId, needsAdminPrivileges: false);

                if (userClaim.Role == UserRole.Admin)
                    return Ok(await _linkService.Delete(id));
                else
                    return Ok(await _linkService.Delete(id, userClaim.UserId));
            } catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
