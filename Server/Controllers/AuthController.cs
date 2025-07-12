using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.DTOs;
using Server.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
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
            // TODO: admin route
            return Ok(await _authService.Login(dto));
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
