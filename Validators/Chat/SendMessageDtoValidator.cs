using FluentValidation;
using Api.DTOs.Chat;

namespace Api.Validators.Chat;

public class SendMessageDtoValidator : AbstractValidator<SendMessageDto>
{
    public SendMessageDtoValidator()
    {
        RuleFor(x => x.ChatRoomId)
            .GreaterThan(0)
            .WithMessage("Chat room ID must be greater than 0");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Message content is required")
            .MinimumLength(1)
            .WithMessage("Message content cannot be empty")
            .MaximumLength(1000)
            .WithMessage("Message content cannot be longer than 1000 characters");
    }
}
