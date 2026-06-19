namespace MentalHealth.Application.Common;

// Normalises nullable date filters to UTC for timestamptz comparisons.
internal static class UtcRange
{
    // Inclusive lower bound: the supplied instant, as UTC.
    public static DateTime? Start(DateTime? value) => ToUtc(value);

    // Exclusive upper bound for an inclusive `to` date: the start of the next day,
    // so an afternoon check-in on the `to` date still matches (CreatedAt < End).
    public static DateTime? End(DateTime? value) => ToUtc(value)?.Date.AddDays(1);

    private static DateTime? ToUtc(DateTime? value)
    {
        if (value is null) return null;
        return value.Value.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
            : value.Value.ToUniversalTime();
    }
}
