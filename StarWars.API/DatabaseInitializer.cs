using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StarWars.Model;
using StarWars.Repository;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace StarWars.Api
{
    public interface IDatabaseInitializer
    {
        Task SeedAsync(string rootPath);
    }

    public class DatabaseInitializer : IDatabaseInitializer
    {
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public DatabaseInitializer(IOptions<JsonOptions> jsonOptions, ApplicationDbContext context, ILogger<DatabaseInitializer> logger)
        {
            _jsonOptions = jsonOptions.Value.JsonSerializerOptions;
            _context = context;
            _logger = logger;
        }

        public async Task SeedAsync(string rootPath)
        {
            await SeedItems(rootPath);
            await _context.SaveChangesAsync()
                .ContinueWith(task => {
                    if (task.IsFaulted) {
                        var exceptions = task.Exception.Flatten();
                        var exception = exceptions.InnerExceptions.FirstOrDefault();
                        _logger.LogError(exception, "SeedAsync exception was thrown");
                    }
                });
        }

        // Seed DB from JSON file for soft startup
        private async Task SeedItems(string rootPath)
        {
            if (!await _context.Movies.AnyAsync())
            {
                var path = Path.Combine(rootPath, "movies.json");
                var json = await File.ReadAllTextAsync(path);
                var items = JsonSerializer.Deserialize<IEnumerable<Movie>>(json, _jsonOptions) ?? [];

                await _context.Movies.AddRangeAsync(items);
                _logger.LogInformation("Seeded {Count} items successfully!", items.Count());
            }
        }
    }
}
