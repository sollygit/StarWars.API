using FluentValidation;
using StarWars.Model.Converters;
using System;
using System.Text.Json.Serialization;

namespace StarWars.Model.ViewModels
{
    public class MovieRatingView
    {
        public string ID { get; set; }
        public string MovieID { get; set; }
        public string Rated { get; set; }
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime Released { get; set; }
        public string Runtime { get; set; }
        public string Genre { get; set; }
        public string Director { get; set; }
        public string Language { get; set; }
        [JsonConverter(typeof(IntFromStringConverter))]
        public int Metascore { get; set; }
        [JsonConverter(typeof(DecimalFromStringConverter))]
        public decimal Rating { get; set; }
    }

    public class MovieRatingViewValidator : AbstractValidator<MovieRatingView>
    {
        public MovieRatingViewValidator()
        {
            RuleFor(x => x.Released)
                .NotEmpty()
                .NotNull()
                .WithMessage("Released date is required")
                .Must(date => date <= DateTime.Now)
                .WithMessage("Released date cannot be in the future");  
            RuleFor(x => x.Metascore)
                .NotEmpty()
                .NotNull()
                .WithMessage("Metascore is required");
            RuleFor(x => x.Rating)
                .NotEmpty()
                .NotNull()
                .WithMessage("Rating is required");
        }
    }
}
