using MentalHealth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MentalHealth.Infrastructure.Persistence.Configurations;

public sealed class CheckInConfiguration : IEntityTypeConfiguration<CheckIn>
{
    public void Configure(EntityTypeBuilder<CheckIn> builder)
    {
        builder.ToTable("check_ins");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Mood).IsRequired();

        builder.Property(c => c.Notes).HasMaxLength(CheckIn.MaxNotesLength);

        builder.Property(c => c.CreatedAt).IsRequired();

        builder.HasOne(c => c.User)
            .WithMany(u => u.CheckIns)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.UserId);
        builder.HasIndex(c => c.CreatedAt);
    }
}
