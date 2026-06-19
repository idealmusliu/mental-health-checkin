using FluentAssertions;
using MentalHealth.Api.Controllers;
using MentalHealth.Application.Common.Abstractions;
using MentalHealth.Application.Common.Interfaces;
using MentalHealth.Application.Common.Results;
using MentalHealth.Application.Dashboard.Dtos;
using MentalHealth.Application.Dashboard.Queries.GetDashboardStats;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace MentalHealth.Tests.Api;

public class DashboardControllerTests
{
    private readonly Mock<IQueryHandler<GetDashboardStatsQuery, Result<DashboardStatsDto>>> _stats = new();
    private readonly Mock<ICurrentUser> _currentUser = new();

    private DashboardController BuildController() =>
        ApiTestSetup.Configure(new DashboardController(_stats.Object, _currentUser.Object));

    private void SignInAsManager()
    {
        _currentUser.SetupGet(c => c.IsAuthenticated).Returns(true);
        _currentUser.SetupGet(c => c.IsManager).Returns(true);
    }

    private void SignInAsEmployee()
    {
        _currentUser.SetupGet(c => c.IsAuthenticated).Returns(true);
        _currentUser.SetupGet(c => c.IsManager).Returns(false);
    }

    [Fact]
    public async Task GetStats_WhenUnauthenticated_Returns401()
    {
        var controller = BuildController();

        var response = await controller.GetStats(userId: null, from: null, to: null, tzOffsetMinutes: 0, default);

        response.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(401);
        _stats.Verify(h => h.Handle(It.IsAny<GetDashboardStatsQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetStats_AsEmployee_IsForbidden()
    {
        SignInAsEmployee();
        var controller = BuildController();

        var response = await controller.GetStats(userId: null, from: null, to: null, tzOffsetMinutes: 0, default);

        response.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(403);
        _stats.Verify(h => h.Handle(It.IsAny<GetDashboardStatsQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetStats_AsManager_ReturnsOk()
    {
        SignInAsManager();
        _stats.Setup(h => h.Handle(It.IsAny<GetDashboardStatsQuery>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(Result<DashboardStatsDto>.Success(new DashboardStatsDto()));
        var controller = BuildController();

        var response = await controller.GetStats(userId: null, from: null, to: null, tzOffsetMinutes: 0, default);

        response.Should().BeOfType<OkObjectResult>();
        _stats.Verify(h => h.Handle(It.IsAny<GetDashboardStatsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
