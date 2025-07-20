using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarWars.Model;

namespace StarWars.Repository.Configuration
{
    internal class MovieConfiguration : IEntityTypeConfiguration<Movie>
    {
        public void Configure(EntityTypeBuilder<Movie> builder)
        {
            builder.HasKey(m => m.ID).HasName("PK_Movie");
            builder.Property(m => m.ID).IsRequired();
            builder.Property(m => m.Title).IsRequired().HasMaxLength(100);
            builder.Property(m => m.Poster).HasMaxLength(500);
            builder.Property(m => m.Price).HasColumnType("decimal(18,2)");
            builder.ToTable("Movie");
        }
    }
}
