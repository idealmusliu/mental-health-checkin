using FluentValidation;
using MentalHealth.Domain.Entities;

namespace MentalHealth.Application.CheckIns.Commands.UpdateCheckIn;

public sealed class UpdateCheckInValidator : AbstractValidator<UpdateCheckInCommand>
{
    public UpdateCheckInValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Check-in id is required.");

        RuleFor(x => x.Mood)
            .InclusiveBetween(CheckIn.MinMood, CheckIn.MaxMood)
            .WithMessage($"Mood must be between {CheckIn.MinMood} and {CheckIn.MaxMood}.");

        RuleFor(x => x.Notes)
            .MaximumLength(CheckIn.MaxNotesLength)
            .WithMessage($"Notes cannot exceed {CheckIn.MaxNotesLength} characters.");
    }
}
