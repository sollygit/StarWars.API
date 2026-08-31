using FluentValidation;
using StarWars.Model;
using System;

namespace StarWars.API.Validators
{
    public class MovieRatingValidator : AbstractValidator<MovieRating>
    {
        public MovieRatingValidator()
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
