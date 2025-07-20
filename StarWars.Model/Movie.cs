using System.Collections.Generic;

namespace StarWars.Model
{
    public class Movie : AuditableEntity
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Year { get; set; }
        public string Type { get; set; } = "Movie";
        public string Poster { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; } = true;
        public ICollection<MovieRating> MovieRatings { get; set; } = new List<MovieRating>();
    }
}
