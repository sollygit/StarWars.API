using AutoMapper;
using Microsoft.Extensions.Logging;
using StarWars.Interface;
using StarWars.Model;
using StarWars.Model.ViewModels;
using StarWars.Repository;
using System.Threading.Tasks;

namespace StarWars.Api.Services
{
    public class MovieService : IMovieService
    {
        private readonly ILogger<MovieService> logger;
        private readonly IMoviesRepository repo;

        public MovieService(
            ILogger<MovieService> logger,
            IMoviesRepository repo)
        {
            this.logger = logger;
            this.repo = repo;
        }

        public async Task<MovieView[]> All()
        {
            var items = await repo.All();
            return Mapper.Map<MovieView[]>(items);
        }

        public async Task<MovieView> Get(string id)
        {
            var item = await repo.GetByID(id);
            return Mapper.Map<MovieView>(item);
        }

        public async Task<MovieView> Create(Movie movie)
        {
            var item = await repo.Create(movie);
            return Mapper.Map<MovieView>(item);
        }
    }
}
