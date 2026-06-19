using FluentValidation;
using MentalHealth.Application.CheckIns.Dtos;
using MentalHealth.Application.Common.Abstractions;
using MentalHealth.Application.Common.Extensions;
using MentalHealth.Application.Common.Interfaces;
using MentalHealth.Application.Common.Results;
using MentalHealth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MentalHealth.Application.CheckIns.Commands.CreateCheckIn;

public sealed class CreateCheckInHandler : ICommandHandler<CreateCheckInCommand, Result<CheckInDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<CreateCheckInCommand> _validator;
    private readonly IDateTimeProvider _clock;

    public CreateCheckInHandler(
        IApplicationDbContext db,
        IValidator<CreateCheckInCommand> validator,
        IDateTimeProvider clock)
    {
        _db = db;
        _validator = validator;
        _clock = clock;
    }

    public async Task<Result<CheckInDto>> Handle(CreateCheckInCommand command, CancellationToken cancellationToken = default)
    {
        var validation = await _validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
            return Result<CheckInDto>.Validation(validation.ToErrorDictionary());

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);

        if (user is null)
            return Result<CheckInDto>.NotFound($"User '{command.UserId}' was not found.");

        var checkIn = CheckIn.Create(command.UserId, command.Mood, command.Notes, _clock.UtcNow);

        _db.CheckIns.Add(checkIn);
        await _db.SaveChangesAsync(cancellationToken);

        var dto = CheckInDto.FromEntity(checkIn) with { UserName = user.Name };
        return Result<CheckInDto>.Success(dto);
    }
}
