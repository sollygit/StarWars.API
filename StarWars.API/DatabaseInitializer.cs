using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StarWars.Api.Settings;
using StarWars.Common;
using StarWars.Repository;
using System.Linq;
using System.Threading.Tasks;

namespace StarWars.Api
{
    public interface IDatabaseInitializer
    {
        Task SeedAsync();
    }

    public class DatabaseInitializer : IDatabaseInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly MovieCodeSettings _settings;
        private readonly ILogger _logger;

        public DatabaseInitializer(
            ApplicationDbContext context, 
            MovieCodeSettings settings,
            ILogger<DatabaseInitializer> logger)
        {
            _context = context;
            _settings = settings;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            await SeedItems();
            await _context.SaveChangesAsync()
                .ContinueWith(task => {
                    if (task.IsFaulted) {
                        var exceptions = task.Exception.Flatten();
                        var exception = exceptions.InnerExceptions.FirstOrDefault();
                        _logger.LogError(exception, "SeedAsync exception was thrown");
                    }
                });
        }

        // Seed DB with fake items for soft startup
        private async Task SeedItems()
        {
            if (!await _context.Movies.AnyAsync())
            {
                var items = MovieFaker.Generate(10, _settings.Alphabet);
                await _context.Movies.AddRangeAsync(items);
                _logger.LogInformation("SeedItems success");
            }
        }
    }
}
