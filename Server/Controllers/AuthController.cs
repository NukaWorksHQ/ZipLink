using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.DTOs;
using Server.Services;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authorization;

namespace Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly UserAccessValidator _userAccessValidator;

        public AuthController(AuthService authService, UserAccessValidator accessValidator)
        {
            _authService = authService;
            _userAccessValidator = accessValidator;
        }

        [
            SwaggerOperation(
            Summary = "Login an user",
            Description = "Login will issue a new JWT for the user")
        ]
        [SwaggerResponse(200, "Return a Bearer JWT (string)")]
        [HttpPut(nameof(Login))]
        public async Task<IActionResult> Login([FromBody] AuthDto dto)
        {
            return Ok(await _authService.Login(dto));
        }

        [
            SwaggerOperation(
            Summary = "Reset password user",
            Description = "Reset the password of an user")
        ]
        [SwaggerResponse(200, "Return a Bearer JWT (string)")]
        [Authorize]
        [HttpPut(nameof(ResetPassword))]
        public async Task<IActionResult> ResetPassword([FromBody] PasswordDto dto)
        {
            var userClaim = _userAccessValidator.GetUserClaimStatus(User);
            _userAccessValidator.ValidateUser(User, userClaim.UserId, needsAdminPrivileges: false);
            
            return Ok(await _authService.ResetPassword(userClaim.UserId, dto));
        }

        [
            SwaggerOperation(
            Summary = "Create an user (signup)",
            Description = "Login will issue a new JWT for the user")
        ]
        [SwaggerResponse(200, "Return a Bearer JWT (string)")]
        [HttpPut(nameof(Create))]
        public async Task<IActionResult> Create([FromBody] AuthDto dto)
        {
            try
            {
                var user = await _authService.Create(dto);
                var token = _authService.GenerateToken(user.Id, user.Role);
                return Ok(token);
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest("User already exits");
            }
        }
    }
}
