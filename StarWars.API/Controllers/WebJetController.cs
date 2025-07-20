using Microsoft.AspNetCore.Mvc;
using StarWars.Interface;
using System.Threading.Tasks;

namespace StarWars.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebJetController : ControllerBase
    {
        private readonly IWebJetService service;

        public WebJetController(IWebJetService service)
        {
            this.service = service;
        }

        [HttpGet("{provider}")]
        public async Task<IActionResult> GetAll([FromRoute] string provider)
        {
            if (string.IsNullOrEmpty(provider))
                return BadRequest("Provider is required");

            var items = await service.GetAll(provider);

            return new OkObjectResult(items);
        }

        [HttpGet("{provider}/{id}")]
        public async Task<IActionResult> Get([FromRoute] string provider, [FromRoute] string id)
        {
            if (string.IsNullOrEmpty(provider))
                return BadRequest("Provider is required");

            if (string.IsNullOrEmpty(id))
                return BadRequest("Movie ID is required");

            var item = await service.Get(provider, id);

            return new OkObjectResult(item);
        }
    }
}