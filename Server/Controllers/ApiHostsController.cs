using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ApiHostsController : ControllerBase
    {
        private readonly IApiHostService _apiHostService;

        public ApiHostsController(IApiHostService apiHostService)
        {
            _apiHostService = apiHostService;
        }

        [SwaggerOperation(
            Summary = "Get all configured API hosts",
            Description = "Returns a list of all configured API hosts that can be used for link creation.")]
        [SwaggerResponse(200, "Returns the list of API hosts")]
        [HttpGet]
        public IActionResult GetApiHosts()
        {
            var apiHosts = _apiHostService.GetApiHosts();
            return Ok(apiHosts);
        }
    }
}