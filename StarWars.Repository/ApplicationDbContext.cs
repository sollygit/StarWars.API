using Microsoft.EntityFrameworkCore;
using StarWars.Model;
using StarWars.Repository.Configuration;

namespace StarWars.Repository
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Movie> Movies { get; set; }
        public DbSet<MovieRating> MovieRating { get; set; }

        public ApplicationDbContext(DbContextOptions options) : base(options)
        { 
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfiguration(new MovieConfiguration());
            builder.ApplyConfiguration(new MovieRatingConfiguration());
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditEntities();
            return await base.SaveChangesAsync();
        }

        private void UpdateAuditEntities()
        {
            var modifiedEntries = ChangeTracker.Entries()
                .Where(x => x.Entity is IAuditable && (x.State == EntityState.Added || x.State == EntityState.Modified));
            foreach (var entry in modifiedEntries)
            {
                var entity = (IAuditable)entry.Entity;
                DateTime now = DateTime.UtcNow;

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedOn = now;
                }
                else
                {
                    entity.UpdatedOn = now;
                    base.Entry(entity).Property(x => x.CreatedOn).IsModified = false;
                }
            }
        }
    }
}
