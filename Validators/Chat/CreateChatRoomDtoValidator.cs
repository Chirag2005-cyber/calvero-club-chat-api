using FluentValidation;
using Api.DTOs.Chat;

namespace Api.Validators.Chat;

public class CreateChatRoomDtoValidator : AbstractValidator<CreateChatRoomDto>
{
    public CreateChatRoomDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Room name is required")
            .MinimumLength(2)
            .WithMessage("Room name must be at least 2 characters long")
            .MaximumLength(100)
            .WithMessage("Room name cannot be longer than 100 characters")
            .Matches("^[a-zA-Z0-9\\s_-]+$")
            .WithMessage("Room name can only contain letters, numbers, spaces, underscores and hyphens");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(3)
            .WithMessage("Password must be at least 3 characters long")
            .MaximumLength(50)
            .WithMessage("Password cannot be longer than 50 characters");
    }
}
