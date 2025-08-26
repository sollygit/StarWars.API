using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StarWars.Interface;
using StarWars.Model;
using StarWars.Model.ViewModels;
using System;
using System.Threading.Tasks;

namespace StarWars.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieService service;

        public MoviesController(IMovieService service)
        {
            this.service = service;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> AllAsync()
        {
            var items = await service.All();
            return new OkObjectResult(items);
        }

        [Authorize]
        [HttpGet("protected")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AllProtectedAsync()
        {
            var items = await service.All();
            return new OkObjectResult(items);
        }

        [Authorize("read:messages")]
        [HttpGet("secured")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AllSecuredAsync()
        {
            var data = await Task.FromResult(new {
                statusCode = 200,
                message = "This is a secured endpoint data which is only available to authenticated users",
                timestamp = DateTime.Now,
                path = "/api/movie/all/secured",
                data = new[] {
                    new {
                        id = 1,
                        title = "Inception",
                        poster = "https://picsum.photos/id/102/640/480",
                        year = 2010,
                        price = 12.99
                    },
                    new {
                        id = 2,
                        title = "The Matrix",
                        poster = "https://picsum.photos/id/104/640/480",
                        year = 1999,
                        price = 9.99
                    }
                }
            });
            return new OkObjectResult(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get([FromRoute] string id)
        {
            var item = await service.Get(id);
            if (item == null) return new NotFoundObjectResult(id);
            
            return new OkObjectResult(item);
        }

        [Authorize]
        [HttpPost()]
        public async Task<IActionResult> Create([FromBody] MovieView movieView)
        {
            var movie = Mapper.Map<Movie>(movieView);
            var item = await service.Create(movie);
            if (item == null) return new BadRequestObjectResult($"Movie with ID '{movieView.ID}' already exists in DB");
            
            return new OkObjectResult(item);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] string id, [FromBody] MovieView movieView)
        {
            var existingItem = await service.Get(id);
            if (existingItem == null) return new NotFoundObjectResult(id);
            var movie = Mapper.Map<Movie>(movieView);
            var item = await service.Update(id, movie);
            if (item == null) return new BadRequestObjectResult($"Movie with ID '{id}' could not be updated");
            return new OkObjectResult(item);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {
            var item = await service.Get(id);
            if (item == null) return new NotFoundObjectResult(id);

            item = await service.Delete(id);

            if (item == null) return new BadRequestObjectResult($"Movie with ID '{id}' could not be deleted");

            return new OkObjectResult(item);
        }
    }
}