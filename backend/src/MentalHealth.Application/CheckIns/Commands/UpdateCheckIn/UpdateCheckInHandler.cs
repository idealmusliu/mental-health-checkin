using FluentValidation;
using MentalHealth.Application.CheckIns.Dtos;
using MentalHealth.Application.Common.Abstractions;
using MentalHealth.Application.Common.Extensions;
using MentalHealth.Application.Common.Interfaces;
using MentalHealth.Application.Common.Results;
using Microsoft.EntityFrameworkCore;

namespace MentalHealth.Application.CheckIns.Commands.UpdateCheckIn;

public sealed class UpdateCheckInHandler : ICommandHandler<UpdateCheckInCommand, Result<CheckInDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<UpdateCheckInCommand> _validator;
    private readonly IDateTimeProvider _clock;

    public UpdateCheckInHandler(
        IApplicationDbContext db,
        IValidator<UpdateCheckInCommand> validator,
        IDateTimeProvider clock)
    {
        _db = db;
        _validator = validator;
        _clock = clock;
    }

    public async Task<Result<CheckInDto>> Handle(UpdateCheckInCommand command, CancellationToken cancellationToken = default)
    {
        var validation = await _validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
            return Result<CheckInDto>.Validation(validation.ToErrorDictionary());

        // Tracked fetch (no AsNoTracking) so SaveChanges persists the mutation.
        var checkIn = await _db.CheckIns
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == command.Id, cancellationToken);

        // Treat "not yours" the same as "not found" so we don't leak that the id exists.
        if (checkIn is null || (command.RestrictToUserId is { } owner && checkIn.UserId != owner))
            return Result<CheckInDto>.NotFound($"Check-in '{command.Id}' was not found.");

        checkIn.Update(command.Mood, command.Notes, _clock.UtcNow);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<CheckInDto>.Success(CheckInDto.FromEntity(checkIn));
    }
}
