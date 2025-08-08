using FluentValidation;
using Api.DTOs.Chat;

namespace Api.Validators.Chat;

public class JoinChatRoomDtoValidator : AbstractValidator<JoinChatRoomDto>
{
    public JoinChatRoomDtoValidator()
    {
        RuleFor(x => x.ChatRoomId)
            .GreaterThan(0)
            .WithMessage("Chat room ID must be greater than 0");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(3)
            .WithMessage("Password must be at least 3 characters long")
            .MaximumLength(50)
            .WithMessage("Password cannot be longer than 50 characters");
    }
}
