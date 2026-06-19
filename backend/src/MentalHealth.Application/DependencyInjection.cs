using FluentValidation;
using MentalHealth.Application.CheckIns.Commands.CreateCheckIn;
using MentalHealth.Application.CheckIns.Commands.UpdateCheckIn;
using MentalHealth.Application.CheckIns.Dtos;
using MentalHealth.Application.CheckIns.Queries.GetCheckInById;
using MentalHealth.Application.CheckIns.Queries.GetCheckIns;
using MentalHealth.Application.Common.Abstractions;
using MentalHealth.Application.Common.Models;
using MentalHealth.Application.Common.Results;
using MentalHealth.Application.Dashboard.Dtos;
using MentalHealth.Application.Dashboard.Queries.GetDashboardStats;
using MentalHealth.Application.Users.Dtos;
using MentalHealth.Application.Users.Queries.GetUsers;
using Microsoft.Extensions.DependencyInjection;

namespace MentalHealth.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<CreateCheckInCommand, Result<CheckInDto>>, CreateCheckInHandler>();
        services.AddScoped<ICommandHandler<UpdateCheckInCommand, Result<CheckInDto>>, UpdateCheckInHandler>();

        services.AddScoped<IQueryHandler<GetCheckInsQuery, Result<PagedResult<CheckInDto>>>, GetCheckInsHandler>();
        services.AddScoped<IQueryHandler<GetCheckInByIdQuery, Result<CheckInDto>>, GetCheckInByIdHandler>();
        services.AddScoped<IQueryHandler<GetDashboardStatsQuery, Result<DashboardStatsDto>>, GetDashboardStatsHandler>();
        services.AddScoped<IQueryHandler<GetUsersQuery, IReadOnlyList<UserDto>>, GetUsersHandler>();

        services.AddScoped<IValidator<CreateCheckInCommand>, CreateCheckInValidator>();
        services.AddScoped<IValidator<UpdateCheckInCommand>, UpdateCheckInValidator>();
        services.AddScoped<IValidator<GetCheckInsQuery>, GetCheckInsValidator>();

        return services;
    }
}
