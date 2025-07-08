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
            // TODO: admin route
            try
            {
                var user = await _authService.Create(dto);
                var token = _authService.GenerateToken(user.Id);
                return Ok(token);
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest("User already exits");
            }
        }

        [
            SwaggerOperation(
            Summary = "Refresh an accessToken",
            Description = "Ask to JwtService to refresh an accessToken")
        ]
        [SwaggerResponse(200, "Return the object found")]
        [SwaggerResponse(404, "User not found")]
        [Authorize]
        [HttpGet(nameof(Refresh) + "/{id}")]
        public async Task<IActionResult> Refresh(string id)
        {
            try
            {
                var user = await _authService.Refresh(id);
                return Ok(user);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
