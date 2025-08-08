using FluentValidation;
using Api.DTOs;

namespace Api.Validators.Auth;

public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserDtoValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required")
            .MinimumLength(3)
            .WithMessage("Username must be at least 3 characters long")
            .MaximumLength(30)
            .WithMessage("Username cannot be longer than 30 characters")
            .Matches("^[a-zA-Z0-9_-]+$")
            .WithMessage("Username can only contain letters, numbers, underscores and hyphens");
    }
}
