using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class CommentConfiguration : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Text)
                .IsRequired()
                .HasMaxLength(500);

            builder.HasOne(c => c.Author)
                .WithMany()
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Taska)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.TaskaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasData(
                new Comment
                {
                    Id = "3fa85f64-5717-4562-b3fc-2c963f66afa9",
                    Text = "Please add more details to the task description",
                    CreatedAt = DateTime.UtcNow,
                    AuthorId = "740bc7d9-dd55-4ac0-83f3-709d820968e7",
                    TaskaId = "3fa85f64-5717-4562-b3fc-2c963f66afa7"
                }
            );
        }
    }
}