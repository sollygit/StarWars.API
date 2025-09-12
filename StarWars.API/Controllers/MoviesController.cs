using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StarWars.Api.Services;
using StarWars.Model;
using StarWars.Model.ViewModels;
using System.Threading.Tasks;

namespace StarWars.Api.Controllers
{
    [ApiController]
    [ApiExplorerSettings(GroupName = "movies")]
    [Route("api/[controller]")]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieService _movieService;

        public MoviesController(IMovieService movieService)
        {
            _movieService = movieService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> AllAsync()
        {
            var items = await _movieService.AllAsync();
            return new OkObjectResult(items);
        }

        [Authorize]
        [HttpGet("protected")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AllProtectedAsync()
        {
            var items = await _movieService.AllAsync();
            return new OkObjectResult(items);
        }

        [Authorize("read:messages")]
        [HttpGet("secured")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AllSecuredAsync()
        {
            var items = await _movieService.AllAsync();
            return new OkObjectResult(items);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync([FromRoute] string id)
        {
            var item = await _movieService.GetAsync(id);
            if (item == null) return new NotFoundObjectResult(id);
            
            return new OkObjectResult(item);
        }

        [Authorize]
        [HttpPost()]
        public async Task<IActionResult> CreateAsync([FromBody] MovieView movieView)
        {
            var movie = Mapper.Map<Movie>(movieView);
            var item = await _movieService.CreateAsync(movie);
            if (item == null) return new BadRequestObjectResult($"Movie with ID '{movieView.ID}' already exists in DB");
            
            return new OkObjectResult(item);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync([FromRoute] string id, [FromBody] MovieView movieView)
        {
            var existingItem = await _movieService.GetAsync(id);
            if (existingItem == null) return new NotFoundObjectResult(id);
            var movie = Mapper.Map<Movie>(movieView);
            var item = await _movieService.UpdateAsync(id, movie);
            if (item == null) return new BadRequestObjectResult($"Movie with ID '{id}' could not be updated");
            return new OkObjectResult(item);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] string id)
        {
            var item = await _movieService.GetAsync(id);
            if (item == null) return new NotFoundObjectResult(id);

            item = await _movieService.DeleteAsync(id);

            if (item == null) return new BadRequestObjectResult($"Movie with ID '{id}' could not be deleted");

            return new OkObjectResult(item);
        }
    }
}