using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Common;
using Shared.DTOs;
using Server.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace Server.Controllers
{
    [ApiController]
    [Route("/")]
    public class LinkRedirector : ControllerBase
    {
        private readonly LinkService _linkService;

        public LinkRedirector(LinkService linkService)
        {
            _linkService = linkService;
        }

        [
            SwaggerOperation(
            Summary = "Redirect to the Target",
            Description = "Get a link by providing the LinkId and redirect to the Target link.")
        ]
        [SwaggerResponse(301, "Redirect to the target")]
        [SwaggerResponse(404, "Link not found")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            try
            {
                var link = await _linkService.Get(id);
                if (link == null || string.IsNullOrEmpty(link.Target))
                    return StatusCode(404, "Link not found");
               
                return RedirectPermanent(link.Target);
            } catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
