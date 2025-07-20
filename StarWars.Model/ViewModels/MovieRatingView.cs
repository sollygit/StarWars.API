using FluentValidation;

namespace StarWars.Model.ViewModels
{
    public class MovieRatingView
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Year { get; set; }
        public string Type { get; set; }
        public string Poster { get; set; }
        public decimal Price { get; set; }
        public string Rated { get; set; }
        public string Released { get; set; }
        public string Runtime { get; set; }
        public string Genre { get; set; }
        public string Director { get; set; }
        public string Writer { get; set; }
        public string Language { get; set; }
        public string Country { get; set; }
        public string Awards { get; set; }
        public int Metascore { get; set; }
        public decimal Rating { get; set; }
        public string Votes { get; set; }
    }

    public class MovieRatingViewValidator : AbstractValidator<MovieRatingView>
    {
        public MovieRatingViewValidator()
        {
            RuleFor(x => x.Price)
                .NotEmpty()
                .NotNull()
                .WithMessage("Price is required");
            RuleFor(x => x.Metascore)
                .NotEmpty()
                .NotNull()
                .WithMessage("Metascore is required");
            RuleFor(x => x.Rating)
                .NotEmpty()
                .NotNull()
                .WithMessage("Rating is required");
            RuleFor(x => x.Votes)
                .NotEmpty()
                .NotNull()
                .WithMessage("Votes is required");
        }
    }
}
