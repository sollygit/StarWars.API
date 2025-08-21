using System.Collections.Generic;

namespace StarWars.Model
{
    public class Movie : AuditableEntity
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public int Year { get; set; }
        public string Poster { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; } = true;
        public IEnumerable<MovieRating> MovieRatings { get; set; } = new List<MovieRating>();
    }
}
