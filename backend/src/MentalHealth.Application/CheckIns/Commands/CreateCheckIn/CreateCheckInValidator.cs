using FluentValidation;
using MentalHealth.Domain.Entities;

namespace MentalHealth.Application.CheckIns.Commands.CreateCheckIn;

public sealed class CreateCheckInValidator : AbstractValidator<CreateCheckInCommand>
{
    public CreateCheckInValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(x => x.Mood)
            .InclusiveBetween(CheckIn.MinMood, CheckIn.MaxMood)
            .WithMessage($"Mood must be between {CheckIn.MinMood} and {CheckIn.MaxMood}.");

        RuleFor(x => x.Notes)
            .MaximumLength(CheckIn.MaxNotesLength)
            .WithMessage($"Notes cannot exceed {CheckIn.MaxNotesLength} characters.");
    }
}
