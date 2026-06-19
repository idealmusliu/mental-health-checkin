using FluentAssertions;
using MentalHealth.Api.Contracts;
using MentalHealth.Api.Controllers;
using MentalHealth.Application.CheckIns.Commands.CreateCheckIn;
using MentalHealth.Application.CheckIns.Commands.UpdateCheckIn;
using MentalHealth.Application.CheckIns.Dtos;
using MentalHealth.Application.CheckIns.Queries.GetCheckInById;
using MentalHealth.Application.CheckIns.Queries.GetCheckIns;
using MentalHealth.Application.Common.Abstractions;
using MentalHealth.Application.Common.Interfaces;
using MentalHealth.Application.Common.Models;
using MentalHealth.Application.Common.Results;
using MentalHealth.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace MentalHealth.Tests.Api;

public class CheckInsControllerTests
{
    private readonly Mock<ICommandHandler<CreateCheckInCommand, Result<CheckInDto>>> _create = new();
    private readonly Mock<ICommandHandler<UpdateCheckInCommand, Result<CheckInDto>>> _update = new();
    private readonly Mock<IQueryHandler<GetCheckInsQuery, Result<PagedResult<CheckInDto>>>> _list = new();
    private readonly Mock<IQueryHandler<GetCheckInByIdQuery, Result<CheckInDto>>> _getById = new();
    private readonly Mock<ICurrentUser> _currentUser = new();

    private CheckInsController BuildController() =>
        ApiTestSetup.Configure(new CheckInsController(
            _create.Object, _update.Object, _list.Object, _getById.Object, _currentUser.Object));

    private void SignInAsEmployee(Guid userId)
    {
        _currentUser.SetupGet(c => c.IsAuthenticated).Returns(true);
        _currentUser.SetupGet(c => c.IsManager).Returns(false);
        _currentUser.SetupGet(c => c.Role).Returns(UserRole.Employee);
        _currentUser.SetupGet(c => c.UserId).Returns(userId);
    }

    private void SignInAsManager()
    {
        _currentUser.SetupGet(c => c.IsAuthenticated).Returns(true);
        _currentUser.SetupGet(c => c.IsManager).Returns(true);
        _currentUser.SetupGet(c => c.Role).Returns(UserRole.Manager);
    }

    private static CheckInDto SampleDto(Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        UserId = TestDataIds.Bob,
        UserName = "Bob Employee",
        Mood = 4,
        Notes = "ok",
        CreatedAt = DateTime.UtcNow
    };

    [Fact]
    public async Task Create_AsEmployee_Returns201AndSubmitsForThemselves()
    {
        SignInAsEmployee(TestDataIds.Bob);
        var dto = SampleDto();

        CreateCheckInCommand? captured = null;
        _create.Setup(h => h.Handle(It.IsAny<CreateCheckInCommand>(), It.IsAny<CancellationToken>()))
               .Callback<CreateCheckInCommand, CancellationToken>((c, _) => captured = c)
               .ReturnsAsync(Result<CheckInDto>.Success(dto));
        var controller = BuildController();

        var response = await controller.Create(new CreateCheckInRequest(4, "ok"), default);

        var created = response.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(CheckInsController.GetById));
        created.RouteValues!["id"].Should().Be(dto.Id);
        captured!.UserId.Should().Be(TestDataIds.Bob);
    }

    [Fact]
    public async Task Create_WhenUnauthenticated_Returns401()
    {
        var controller = BuildController();

        var response = await controller.Create(new CreateCheckInRequest(4, "ok"), default);

        response.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(401);
        _create.Verify(h => h.Handle(It.IsAny<CreateCheckInCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Create_AsManager_IsForbidden()
    {
        SignInAsManager();
        var controller = BuildController();

        var response = await controller.Create(new CreateCheckInRequest(4, "x"), default);

        response.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(403);
        _create.Verify(h => h.Handle(It.IsAny<CreateCheckInCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetById_WhenHandlerReturnsNotFound_Returns404Problem()
    {
        _getById.Setup(h => h.Handle(It.IsAny<GetCheckInByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<CheckInDto>.NotFound("Check-in not found."));
        var controller = BuildController();

        var response = await controller.GetById(Guid.NewGuid(), default);

        var objectResult = response.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(404);
        objectResult.Value.Should().BeOfType<ProblemDetails>();
    }

    [Fact]
    public async Task GetById_AsEmployee_ScopesLookupToOwnUserId()
    {
        SignInAsEmployee(TestDataIds.Bob);

        GetCheckInByIdQuery? captured = null;
        _getById.Setup(h => h.Handle(It.IsAny<GetCheckInByIdQuery>(), It.IsAny<CancellationToken>()))
                .Callback<GetCheckInByIdQuery, CancellationToken>((q, _) => captured = q)
                .ReturnsAsync(Result<CheckInDto>.Success(SampleDto()));
        var controller = BuildController();

        await controller.GetById(Guid.NewGuid(), default);

        captured!.RestrictToUserId.Should().Be(TestDataIds.Bob);
    }

    [Fact]
    public async Task GetById_AsManager_DoesNotScope()
    {
        SignInAsManager();

        GetCheckInByIdQuery? captured = null;
        _getById.Setup(h => h.Handle(It.IsAny<GetCheckInByIdQuery>(), It.IsAny<CancellationToken>()))
                .Callback<GetCheckInByIdQuery, CancellationToken>((q, _) => captured = q)
                .ReturnsAsync(Result<CheckInDto>.Success(SampleDto()));
        var controller = BuildController();

        await controller.GetById(Guid.NewGuid(), default);

        captured!.RestrictToUserId.Should().BeNull();
    }

    [Fact]
    public async Task List_AsEmployee_ForcesFilterToOwnUserId()
    {
        SignInAsEmployee(TestDataIds.Bob);

        GetCheckInsQuery? captured = null;
        _list.Setup(h => h.Handle(It.IsAny<GetCheckInsQuery>(), It.IsAny<CancellationToken>()))
             .Callback<GetCheckInsQuery, CancellationToken>((q, _) => captured = q)
             .ReturnsAsync(Result<PagedResult<CheckInDto>>.Success(
                 new PagedResult<CheckInDto>(Array.Empty<CheckInDto>(), 1, 10, 0)));

        var controller = BuildController();

        await controller.List(userId: TestDataIds.Carol, from: null, to: null, page: 1, pageSize: 10, default);

        captured!.UserId.Should().Be(TestDataIds.Bob);
    }

    [Fact]
    public async Task List_AsManager_KeepsRequestedUserFilter()
    {
        SignInAsManager();

        GetCheckInsQuery? captured = null;
        _list.Setup(h => h.Handle(It.IsAny<GetCheckInsQuery>(), It.IsAny<CancellationToken>()))
             .Callback<GetCheckInsQuery, CancellationToken>((q, _) => captured = q)
             .ReturnsAsync(Result<PagedResult<CheckInDto>>.Success(
                 new PagedResult<CheckInDto>(Array.Empty<CheckInDto>(), 1, 10, 0)));

        var controller = BuildController();

        await controller.List(userId: TestDataIds.Carol, from: null, to: null, page: 1, pageSize: 10, default);

        captured!.UserId.Should().Be(TestDataIds.Carol);
    }

    [Fact]
    public async Task Update_WhenUnauthenticated_Returns401()
    {
        var controller = BuildController();

        var response = await controller.Update(Guid.NewGuid(), new UpdateCheckInRequest(4, "x"), default);

        response.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(401);
        _update.Verify(h => h.Handle(It.IsAny<UpdateCheckInCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_AsManager_IsForbidden()
    {
        SignInAsManager();
        var controller = BuildController();

        var response = await controller.Update(Guid.NewGuid(), new UpdateCheckInRequest(4, "x"), default);

        response.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(403);
        _update.Verify(h => h.Handle(It.IsAny<UpdateCheckInCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_AsEmployee_ScopesToOwnUserId()
    {
        SignInAsEmployee(TestDataIds.Bob);

        UpdateCheckInCommand? captured = null;
        _update.Setup(h => h.Handle(It.IsAny<UpdateCheckInCommand>(), It.IsAny<CancellationToken>()))
               .Callback<UpdateCheckInCommand, CancellationToken>((c, _) => captured = c)
               .ReturnsAsync(Result<CheckInDto>.Success(SampleDto()));
        var controller = BuildController();

        await controller.Update(Guid.NewGuid(), new UpdateCheckInRequest(5, "edited"), default);

        captured!.RestrictToUserId.Should().Be(TestDataIds.Bob);
    }

    private static class TestDataIds
    {
        public static readonly Guid Bob = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        public static readonly Guid Carol = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    }
}
