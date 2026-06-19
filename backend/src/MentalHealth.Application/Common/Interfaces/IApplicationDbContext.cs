using MentalHealth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MentalHealth.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<CheckIn> CheckIns { get; }
    DbSet<User> Users { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
