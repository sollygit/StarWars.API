using Microsoft.AspNetCore.Mvc;
using StarWars.Api.Services;
using System.Threading.Tasks;

namespace StarWars.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebJetController : ControllerBase
    {
        private readonly IWebJetService _webJetService;

        public WebJetController(IWebJetService webJetService)
        {
            _webJetService = webJetService;
        }

        [HttpGet("{provider}")]
        public async Task<IActionResult> GetAllAsync([FromRoute] string provider)
        {
            if (string.IsNullOrEmpty(provider))
                return BadRequest("Provider is required");

            var items = await _webJetService.GetAllAsync(provider);

            return new OkObjectResult(items);
        }

        [HttpGet("{provider}/{id}")]
        public async Task<IActionResult> GetAsync([FromRoute] string provider, [FromRoute] string id)
        {
            if (string.IsNullOrEmpty(provider))
                return BadRequest("Provider is required");

            if (string.IsNullOrEmpty(id))
                return BadRequest("Movie ID is required");

            var item = await _webJetService.GetAsync(provider, id);

            return new OkObjectResult(item);
        }
    }
}