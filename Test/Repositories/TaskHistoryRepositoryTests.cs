using System;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Infrastructure.Repositories;
using Xunit;

namespace Test
{
    public class TaskHistoryRepositoryTests
    {
        [Fact]
        public async Task GetByTaskIdAsync_ShouldReturnOnlyHistoryOfSpecifiedTask()
        {
            var context = TestDbContextFactory.Create();
            var repo = new TaskHistoryRepository(context);

            var user = new User { UserName = "history_user" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var task1 = new Taska
            {
                Title = "Task A",
                Description = "Description A",
                DueDate = DateTime.UtcNow,
                CreatorId = user.Id
            };
            var task2 = new Taska
            {
                Title = "Task B",
                Description = "Description B",
                DueDate = DateTime.UtcNow,
                CreatorId = user.Id
            };
            context.Tasks.AddRange(task1, task2);
            await context.SaveChangesAsync();

            var h1 = new TaskHistory
            {
                TaskaId = task1.Id,
                ChangedField = "Title",
                OldValue = "Old",
                NewValue = "New",
                ChangedById = user.Id,
                ChangedAt = DateTime.UtcNow.AddMinutes(-1)
            };
            var h2 = new TaskHistory
            {
                TaskaId = task1.Id,
                ChangedField = "Status",
                OldValue = "New",
                NewValue = "InProgress",
                ChangedById = user.Id,
                ChangedAt = DateTime.UtcNow
            };

            var h3 = new TaskHistory
            {
                TaskaId = task2.Id,
                ChangedField = "Description",
                OldValue = "Old desc",
                NewValue = "New desc",
                ChangedById = user.Id,
                ChangedAt = DateTime.UtcNow
            };

            await repo.AddAsync(h1);
            await repo.AddAsync(h2);
            await repo.AddAsync(h3);

            var result = (await repo.GetByTaskIdAsync(task1.Id)).ToList();

            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.Equal(task1.Id, r.TaskaId));
            Assert.Equal("Status", result[0].ChangedField); 
            Assert.Equal("Title", result[1].ChangedField);  
        }
    }
}
