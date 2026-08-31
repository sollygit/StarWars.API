using StarWars.Model.Converters;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StarWars.Model.ViewModels
{
    public class MovieView
    {
        public string ID { get; set; }
        public string Title { get; set; }
        [JsonConverter(typeof(IntFromStringConverter))]
        public int Year { get; set; }
        public string Poster { get; set; }
        public decimal Price { get; set; }
        public IEnumerable<MovieRatingView> MovieRatings { get; set; } = new List<MovieRatingView>();
    }
}
