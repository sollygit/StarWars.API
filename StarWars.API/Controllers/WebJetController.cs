using Microsoft.AspNetCore.Mvc;
using StarWars.Api.Services;
using StarWars.Model;
using System.Threading.Tasks;

namespace StarWars.Api.Controllers
{
    [ApiController]
    [ApiExplorerSettings(GroupName = "webjet")]
    [Route("api/[controller]")]
    public class WebJetController : ControllerBase
    {
        private readonly IWebJetService _webJetService;

        public WebJetController(IWebJetService webJetService)
        {
            (_webJetService) = (
                webJetService ?? throw new System.ArgumentNullException(nameof(webJetService)));
        }

        [HttpGet("{provider}")]
        public async Task<IActionResult> GetAllAsync([FromRoute] Provider provider)
        {
            var items = await _webJetService.GetAllAsync(provider);
            return new OkObjectResult(items);
        }

        [HttpGet("{provider}/{id}")]
        public async Task<IActionResult> GetAsync([FromRoute] Provider provider, [FromRoute] string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("Movie ID is required");

            var item = await _webJetService.GetAsync(provider, id);

            return new OkObjectResult(item);
        }
    }
}