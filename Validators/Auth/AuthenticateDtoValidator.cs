using FluentValidation;
using Api.DTOs;

namespace Api.Validators.Auth;

public class AuthenticateDtoValidator : AbstractValidator<AuthenticateDto>
{
    public AuthenticateDtoValidator()
    {
        RuleFor(x => x.Identity)
            .NotEmpty()
            .WithMessage("Identity is required")
            .MinimumLength(3)
            .WithMessage("Identity must be at least 3 characters long")
            .MaximumLength(50)
            .WithMessage("Identity cannot be longer than 50 characters");
    }
}
