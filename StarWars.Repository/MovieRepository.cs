using Microsoft.EntityFrameworkCore;
using StarWars.Model;

namespace StarWars.Repository
{
    public interface IMoviesRepository : IRepository<Movie>
    {
        Task<IEnumerable<Movie>> AllAsync();
        Task<Movie> GetByIdAsync(string id);
        Task<Movie> CreateAsync(Movie movie);
        Task<Movie> UpdateAsync(string id, Movie movie);
        Task<Movie> DeleteAsync(string id);
    }

    public class MovieRepository : BaseRepository<Movie>, IMoviesRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public MovieRepository(ApplicationDbContext context) : base(context)
        {
            _dbContext = context;
        }

        public async Task<IEnumerable<Movie>> AllAsync()
        {
            return await _dbContext.Movies
                .Include(p => p.MovieRatings)
                .OrderBy(p => p.Title)
                .ToListAsync();
        }

        public async Task<Movie> GetByIdAsync(string id)
        {
            return await _dbContext.Movies
                .Include(m => m.MovieRatings)
                .SingleOrDefaultAsync(m => m.ID == id);
        }

        public async Task<Movie> CreateAsync(Movie movie)
        {
            if (_dbContext.Movies.Any(m => m.ID == movie.ID)) return null;

            var entry = _dbContext.Add(movie);
            await _dbContext.SaveChangesAsync();
            return entry.Entity;
        }

        public async Task<Movie> UpdateAsync(string id, Movie entity)
        {
            var movie = _dbContext.Movies.Include(p => p.MovieRatings).Single(p => p.ID == id);
            var movieRatings = movie.MovieRatings;

            // Update the parent movie
            _dbContext.Entry(movie).CurrentValues.SetValues(entity);

            // Remove all existing movieRatings items
            _dbContext.MovieRating.RemoveRange(movieRatings);

            // Add the new movieRatings items
            movie.MovieRatings = entity.MovieRatings;

            // Save changes to the database
            await _dbContext.SaveChangesAsync();

            return movie;
        }

        public async Task<Movie> DeleteAsync(string id)
        {
            var item = _dbContext.Movies.Single(o => o.ID == id);
            var entry = _dbContext.Remove(item);
            await _dbContext.SaveChangesAsync();
            return entry.Entity;
        }
    }
}
