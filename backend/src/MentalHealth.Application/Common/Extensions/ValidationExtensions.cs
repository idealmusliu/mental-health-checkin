using FluentValidation.Results;

namespace MentalHealth.Application.Common.Extensions;

public static class ValidationExtensions
{
    public static IReadOnlyDictionary<string, string[]> ToErrorDictionary(this ValidationResult result) =>
        result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(e => e.ErrorMessage).Distinct().ToArray());
}
