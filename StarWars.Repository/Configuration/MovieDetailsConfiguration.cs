﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarWars.Model;

namespace StarWars.Repository.Configuration
{
    internal class MovieDetailsConfiguration : IEntityTypeConfiguration<MovieDetails>
    {
        public void Configure(EntityTypeBuilder<MovieDetails> builder)
        {
            builder.Property(m => m.ID).IsRequired();
            builder.Property(m => m.Title).IsRequired().HasMaxLength(100);
            builder.Property(m => m.Poster).HasMaxLength(500);
            builder.Property(m => m.Price).HasColumnType("decimal(18,2)");
            builder.Property(m => m.Rating).HasColumnType("decimal(18,2)");
            builder.ToTable("MovieDetails");
        }
    }
}