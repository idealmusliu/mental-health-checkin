using MentalHealth.Application.Common.Interfaces;
using MentalHealth.Domain.Entities;
using MentalHealth.Domain.Enums;
using MentalHealth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MentalHealth.Tests.Common;

public static class TestData
{
    public static readonly Guid ManagerId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid BobId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    public static readonly Guid CarolId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    public static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"mh-tests-{Guid.NewGuid()}")
            .EnableSensitiveDataLogging()
            .Options;

        return new ApplicationDbContext(options);
    }

    public static User Manager() => new() { Id = ManagerId, Name = "Alice Manager", Email = "alice@acme.test", Role = UserRole.Manager };
    public static User Bob() => new() { Id = BobId, Name = "Bob Employee", Email = "bob@acme.test", Role = UserRole.Employee };
    public static User Carol() => new() { Id = CarolId, Name = "Carol Employee", Email = "carol@acme.test", Role = UserRole.Employee };
}

public sealed class FakeDateTimeProvider(DateTime utcNow) : IDateTimeProvider
{
    public DateTime UtcNow { get; } = utcNow;
}
