using MentalHealth.Domain.Entities;
using MentalHealth.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MentalHealth.Infrastructure.Persistence;

// Idempotent runtime seeder (CheckIn is created via a factory, so EF HasData doesn't fit).
public static class DbSeeder
{
    public static readonly Guid ManagerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid BobId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid CarolId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid DanId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    public static async Task SeedAsync(ApplicationDbContext db, CancellationToken cancellationToken = default)
    {
        await SeedUsersAsync(db, cancellationToken);
        await SeedCheckInsAsync(db, cancellationToken);
    }

    private static async Task SeedUsersAsync(ApplicationDbContext db, CancellationToken cancellationToken)
    {
        if (await db.Users.AnyAsync(cancellationToken))
            return;

        db.Users.AddRange(
            new User { Id = ManagerId, Name = "Alice Manager", Email = "alice@acme.test", Role = UserRole.Manager },
            new User { Id = BobId, Name = "Bob Employee", Email = "bob@acme.test", Role = UserRole.Employee },
            new User { Id = CarolId, Name = "Carol Employee", Email = "carol@acme.test", Role = UserRole.Employee },
            new User { Id = DanId, Name = "Dan Employee", Email = "dan@acme.test", Role = UserRole.Employee });

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedCheckInsAsync(ApplicationDbContext db, CancellationToken cancellationToken)
    {
        if (await db.CheckIns.AnyAsync(cancellationToken))
            return;

        var baseDate = DateTime.SpecifyKind(DateTime.UtcNow.Date.AddHours(9), DateTimeKind.Utc);

        // (userId, daysAgo, mood, notes)
        var data = new (Guid UserId, int DaysAgo, int Mood, string? Notes)[]
        {
            (BobId, 13, 3, "Busy sprint, feeling a bit stretched."),
            (CarolId, 13, 4, "Good progress on the design work."),
            (DanId, 12, 2, "Struggling to focus this week."),
            (BobId, 11, 4, null),
            (CarolId, 10, 5, "Shipped the feature, great team effort!"),
            (DanId, 9, 3, "A little better today."),
            (BobId, 8, 3, "Average day."),
            (CarolId, 7, 4, "Steady and calm."),
            (DanId, 6, 2, "Tired, poor sleep."),
            (BobId, 5, 5, "Took a day off, recharged."),
            (CarolId, 4, 4, null),
            (DanId, 3, 3, "Getting back on track."),
            (BobId, 2, 4, "Productive and focused."),
            (CarolId, 1, 5, "Really enjoying the work right now."),
            (DanId, 1, 4, "Best I've felt in a while."),
            (BobId, 0, 4, "Solid start to the day."),
        };

        var checkIns = data.Select(d =>
            CheckIn.Create(d.UserId, d.Mood, d.Notes, baseDate.AddDays(-d.DaysAgo)));

        db.CheckIns.AddRange(checkIns);
        await db.SaveChangesAsync(cancellationToken);
    }
}
