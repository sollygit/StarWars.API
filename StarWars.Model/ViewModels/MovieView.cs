using FluentValidation;
using System.Collections.Generic;

namespace StarWars.Model.ViewModels
{
    public class MovieView
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public int Year { get; set; }
        public string Poster { get; set; }
        public decimal Price { get; set; }
        public IEnumerable<MovieRatingView> MovieRatings { get; set; } = new List<MovieRatingView>();
    }

    public class MovieViewValidator : AbstractValidator<MovieView>
    {
        public MovieViewValidator()
        {
            RuleFor(x => x.ID).NotEmpty().WithMessage("ID cannot be empty");
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title cannot be empty")
                .MaximumLength(100).WithMessage("Title maximum length is 100");
            RuleForEach(x => x.MovieRatings).SetValidator(new MovieRatingViewValidator());
        }
    }
}
