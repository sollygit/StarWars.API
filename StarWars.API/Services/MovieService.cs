using AutoMapper;
using FluentValidation;
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
        private readonly IValidator<Movie> _validator;
        private readonly IMapper _mapper;
        private readonly IMoviesRepository _repo;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MovieService(ILogger<MovieService> logger, IValidator<Movie> validator, IMapper mapper, IMoviesRepository repo, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _validator = validator;
            _mapper = mapper;
            _repo = repo;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<MovieView[]> AllAsync()
        {
            var items = await _repo.AllAsync();
            
            _logger.LogDebug("Retrieved {Count} movies from the repository.", items?.Count() ?? 0);
            _logger.LogDebug("CorrelationId: {CorrelationId}", _httpContextAccessor.HttpContext?.Items[Constants.X_CORRELATION_ID]);

            return _mapper.Map<MovieView[]>(items);
        }

        public async Task<MovieView> GetAsync(string id)
        {
            var item = await _repo.GetByIdAsync(id);
            return _mapper.Map<MovieView>(item);
        }

        public async Task<MovieView> CreateAsync(Movie movie)
        {
            var validationResult = await _validator.ValidateAsync(movie);
            if (!validationResult.IsValid) throw new ValidationException(validationResult.Errors);

            var item = await _repo.CreateAsync(movie);
            return _mapper.Map<MovieView>(item);
        }

        public async Task<MovieView> UpdateAsync(string id, Movie movie)
        {
            var validationResult = await _validator.ValidateAsync(movie);
            if (!validationResult.IsValid) throw new ValidationException(validationResult.Errors);
    
            var item = await _repo.UpdateAsync(id, movie);
            return _mapper.Map<MovieView>(item);
        }

        public async Task<MovieView> DeleteAsync(string id)
        {
            var item = await _repo.DeleteAsync(id);
            return _mapper.Map<MovieView>(item);
        }
    }
}
