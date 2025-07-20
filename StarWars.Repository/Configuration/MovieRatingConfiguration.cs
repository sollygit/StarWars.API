using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarWars.Model;

namespace StarWars.Repository.Configuration
{
    internal class MovieRatingConfiguration : IEntityTypeConfiguration<MovieRating>
    {
        public void Configure(EntityTypeBuilder<MovieRating> builder)
        {
            builder.HasKey(m => m.ID).HasName("PK_MovieRating");
            builder.Property(m => m.ID).IsRequired().HasDefaultValueSql("NEWID()");
            builder.Property(m => m.MovieID).IsRequired();
            builder.Property(m => m.Price).HasColumnType("decimal(18,2)");
            builder.Property(m => m.Votes).IsRequired().HasMaxLength(100);
            builder.Property(m => m.Rating).HasColumnType("decimal(18,2)");
            builder.ToTable("MovieRating");
        }
    }
}
