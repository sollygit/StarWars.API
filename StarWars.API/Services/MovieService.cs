using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using StarWars.Model;
using StarWars.Model.ViewModels;
using StarWars.Repository;
using System.Linq;
using System.Threading.Tasks;

namespace StarWars.Api.Services
{
    public interface IMovieService
    {
        Task<MovieView[]> AllAsync();
        Task<MovieView> GetAsync(string id);
        Task<MovieView> CreateAsync(Movie movie);
        Task<MovieView> UpdateAsync(string id, Movie movie);
        Task<MovieView> DeleteAsync(string id);
    }

    public class MovieService : IMovieService
    {
        private readonly ILogger<MovieService> _logger;
        private readonly IMoviesRepository _repo;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MovieService(ILogger<MovieService> logger, IMoviesRepository repo, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _repo = repo;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<MovieView[]> AllAsync()
        {
            var items = await _repo.AllAsync();
            
            _logger.LogDebug("Retrieved {Count} movies from the repository.", items?.Count() ?? 0);
            _logger.LogDebug("CorrelationId: {CorrelationId}", _httpContextAccessor.HttpContext?.Items[Constants.X_CORRELATION_ID]);

            return Mapper.Map<MovieView[]>(items);
        }

        public async Task<MovieView> GetAsync(string id)
        {
            var item = await _repo.GetByIdAsync(id);
            return Mapper.Map<MovieView>(item);
        }

        public async Task<MovieView> CreateAsync(Movie movie)
        {
            var item = await _repo.CreateAsync(movie);
            return Mapper.Map<MovieView>(item);
        }

        public async Task<MovieView> UpdateAsync(string id, Movie movie)
        {
            var item = await _repo.UpdateAsync(id, movie);
            return Mapper.Map<MovieView>(item);
        }

        public async Task<MovieView> DeleteAsync(string id)
        {
            var item = await _repo.DeleteAsync(id);
            return Mapper.Map<MovieView>(item);
        }
    }
}
