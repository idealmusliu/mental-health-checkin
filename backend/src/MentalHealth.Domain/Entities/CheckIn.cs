namespace MentalHealth.Domain.Entities;

public class CheckIn
{
    public const int MinMood = 1;
    public const int MaxMood = 5;
    public const int MaxNotesLength = 1000;

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public int Mood { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public User? User { get; private set; }

    // Private so creation goes through Create(); EF uses this ctor to materialize.
    private CheckIn() { }

    public static CheckIn Create(Guid userId, int mood, string? notes, DateTime createdAtUtc)
    {
        var checkIn = new CheckIn
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = createdAtUtc
        };
        checkIn.SetMood(mood);
        checkIn.SetNotes(notes);
        return checkIn;
    }

    public void Update(int mood, string? notes, DateTime updatedAtUtc)
    {
        SetMood(mood);
        SetNotes(notes);
        UpdatedAt = updatedAtUtc;
    }

    private void SetMood(int mood)
    {
        if (mood is < MinMood or > MaxMood)
            throw new ArgumentOutOfRangeException(nameof(mood), $"Mood must be between {MinMood} and {MaxMood}.");

        Mood = mood;
    }

    private void SetNotes(string? notes)
    {
        if (notes is { Length: > MaxNotesLength })
            throw new ArgumentException($"Notes cannot exceed {MaxNotesLength} characters.", nameof(notes));

        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }
}
