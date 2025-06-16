using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class TaskHistoryConfiguration : IEntityTypeConfiguration<TaskHistory>
    {
        public void Configure(EntityTypeBuilder<TaskHistory> builder)
        {
            builder.HasKey(th => th.Id);

            builder.Property(th => th.ChangedField)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasOne(th => th.Taska)
                .WithMany(t => t.History)
                .HasForeignKey(th => th.TaskaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(th => th.ChangedBy)
                .WithMany()
                .HasForeignKey(th => th.ChangedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasData(
                new TaskHistory
                {
                    Id = "3fa85f64-5717-4562-b3fc-2c963f66afb0",
                    TaskaId = "3fa85f64-5717-4562-b3fc-2c963f66afa7",
                    ChangedField = "Status",
                    OldValue = "New",
                    NewValue = "InProgress",
                    ChangedAt = DateTime.UtcNow,
                    ChangedById = "740bc7d9-dd55-4ac0-83f3-709d820968e7"
                }
            );
        }
    }
}