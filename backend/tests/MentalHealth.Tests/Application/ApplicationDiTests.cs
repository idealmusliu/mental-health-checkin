using FluentAssertions;
using MentalHealth.Application;
using MentalHealth.Application.CheckIns.Commands.CreateCheckIn;
using MentalHealth.Application.CheckIns.Commands.UpdateCheckIn;
using MentalHealth.Application.CheckIns.Dtos;
using MentalHealth.Application.CheckIns.Queries.GetCheckInById;
using MentalHealth.Application.CheckIns.Queries.GetCheckIns;
using MentalHealth.Application.Common.Abstractions;
using MentalHealth.Application.Common.Interfaces;
using MentalHealth.Application.Common.Models;
using MentalHealth.Application.Common.Results;
using MentalHealth.Application.Dashboard.Dtos;
using MentalHealth.Application.Dashboard.Queries.GetDashboardStats;
using MentalHealth.Application.Users.Dtos;
using MentalHealth.Application.Users.Queries.GetUsers;
using MentalHealth.Tests.Common;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace MentalHealth.Tests.Application;

public class ApplicationDiTests
{
    private static IServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddApplication();
        services.AddScoped(_ => Mock.Of<IApplicationDbContext>());
        services.AddSingleton<IDateTimeProvider>(new FakeDateTimeProvider(DateTime.UtcNow));
        return services.BuildServiceProvider();
    }

    [Fact]
    public void AddApplication_RegistersEveryHandler()
    {
        using var scope = BuildProvider().CreateScope();
        var sp = scope.ServiceProvider;

        sp.GetService<ICommandHandler<CreateCheckInCommand, Result<CheckInDto>>>().Should().NotBeNull();
        sp.GetService<ICommandHandler<UpdateCheckInCommand, Result<CheckInDto>>>().Should().NotBeNull();
        sp.GetService<IQueryHandler<GetCheckInsQuery, Result<PagedResult<CheckInDto>>>>().Should().NotBeNull();
        sp.GetService<IQueryHandler<GetCheckInByIdQuery, Result<CheckInDto>>>().Should().NotBeNull();
        sp.GetService<IQueryHandler<GetDashboardStatsQuery, Result<DashboardStatsDto>>>().Should().NotBeNull();
        sp.GetService<IQueryHandler<GetUsersQuery, IReadOnlyList<UserDto>>>().Should().NotBeNull();
    }
}
