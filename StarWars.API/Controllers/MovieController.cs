﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StarWars.Interface;
using StarWars.Model;
using StarWars.Model.ViewModels;
using System.Threading.Tasks;

namespace StarWars.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MovieController : ControllerBase
    {
        private readonly IMovieService service;

        public MovieController(IMovieService service)
        {
            this.service = service;
        }

        [HttpGet()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> AllAsync()
        {
            var items = await service.All();
            return new OkObjectResult(items);
        }

        [HttpGet("protected")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AllProtectedAsync()
        {
            var items = await service.All();
            return new OkObjectResult(items);
        }

        [HttpGet("secured")]
        [Authorize("read:messages")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AllSecuredAsync()
        {
            var items = await service.All();
            return new OkObjectResult(items);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get([FromRoute] string id)
        {
            var item = await service.Get(id);
            if (item == null) return new NotFoundObjectResult(id);
            
            return new OkObjectResult(item);
        }

        [HttpPost()]
        public async Task<IActionResult> Create([FromBody] MovieView movieView)
        {
            var movie = Mapper.Map<Movie>(movieView);
            var item = await service.Create(movie);
            if (item == null) return new BadRequestObjectResult($"ID '{movieView.ID}' already exists in DB");
            
            return new OkObjectResult(item);
        }
    }
}