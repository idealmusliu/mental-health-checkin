using FluentValidation;

namespace MentalHealth.Application.CheckIns.Queries.GetCheckIns;

public sealed class GetCheckInsValidator : AbstractValidator<GetCheckInsQuery>
{
    public const int MaxPageSize = 100;

    public GetCheckInsValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be 1 or greater.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, MaxPageSize)
            .WithMessage($"PageSize must be between 1 and {MaxPageSize}.");

        RuleFor(x => x)
            .Must(q => q.From <= q.To)
            .When(x => x.From.HasValue && x.To.HasValue)
            .WithName("dateRange")
            .WithMessage("'from' must be earlier than or equal to 'to'.");
    }
}
