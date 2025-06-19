using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class TaskConfiguration : IEntityTypeConfiguration<Taska>
    {
        public void Configure(EntityTypeBuilder<Taska> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.Description)
                .HasMaxLength(500);

            builder.Property(t => t.Status)
                .IsRequired()
                .HasConversion(
                    v => v.ToString(),
                    v => (TaskaStatus)Enum.Parse(typeof(TaskaStatus), v))  
                .HasMaxLength(20);

            builder.Property(t => t.Priority)
                .IsRequired()
                .HasConversion(
                    v => v.ToString(),
                    v => (TaskPriority)Enum.Parse(typeof(TaskPriority), v))
                .HasMaxLength(20);

            builder.HasOne(t => t.Creator)
                .WithMany()
                .HasForeignKey(t => t.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Assignee)
                .WithMany()
                .HasForeignKey(t => t.AssigneeId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            builder.HasData(
                new Taska
                {
                    Id = "3fa85f64-5717-4562-b3fc-2c963f66afa7",
                    Title = "Implement authentication",
                    Description = "Implement JWT authentication for the API",
                    Status = TaskaStatus.InProgress,
                    Priority = TaskPriority.High,
                    DueDate = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatorId = "740bc7d9-dd55-4ac0-83f3-709d820968e7",
                    AssigneeId = "740bc7d9-dd55-4ac0-83f3-709d820968e7"
                }
            );
        }
    }
}