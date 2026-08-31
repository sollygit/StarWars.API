using AutoMapper;
using FluentValidation;
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
        private readonly IMapper _mapper;
        private readonly IValidator<MovieView> _validator;

        public MoviesController(IMovieService movieService, IMapper mapper, IValidator<MovieView> validator)
        {
            _movieService = movieService;
            _mapper = mapper;
            _validator = validator;
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
            var result = await _validator.ValidateAsync(movieView);
            if (!result.IsValid) return BadRequest(result.Errors);

            var movie = _mapper.Map<Movie>(movieView);
            var item = await _movieService.CreateAsync(movie);
            if (item == null) return BadRequest($"Movie with ID '{movieView.ID}' already exists in DB");
            
            return new OkObjectResult(item);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync([FromRoute] string id, [FromBody] MovieView movieView)
        {
            var existingItem = await _movieService.GetAsync(id);
            if (existingItem == null) return NotFound(id);

            var result = await _validator.ValidateAsync(movieView);
            if (!result.IsValid) return BadRequest(result.Errors);

            var movie = _mapper.Map<Movie>(movieView);
            var item = await _movieService.UpdateAsync(id, movie);
            if (item == null) return BadRequest($"Movie with ID '{id}' could not be updated");
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