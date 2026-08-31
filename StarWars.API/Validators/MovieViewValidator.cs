using FluentValidation;
using StarWars.Model.ViewModels;

namespace StarWars.API.Validators
{
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
