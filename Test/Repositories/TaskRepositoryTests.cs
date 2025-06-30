using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories;
using Xunit;

namespace Test
{
    public class TaskRepositoryTests
    {
        [Fact]
        public async Task ExistsAsync_Test()
        {
            var context = TestDbContextFactory.Create();
            var repo = new TaskRepository(context);

            var user = new User { UserName = "u1" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var task = new Taska
            {
                Title = "T",
                Description = "D",
                DueDate = DateTime.UtcNow,
                CreatorId = user.Id
            };
            await repo.AddAsync(task);
            await context.SaveChangesAsync(); 

            var exists = await repo.ExistsAsync(task.Id);
            Assert.True(exists);
        }

        [Fact]
        public async Task FindAsync_Test()
        {
            var context = TestDbContextFactory.Create();
            var repo = new TaskRepository(context);

            var user = new User { UserName = "u2" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            await repo.AddAsync(new Taska
            {
                Title = "Task 1",
                Description = "A",
                DueDate = DateTime.UtcNow,
                CreatorId = user.Id
            });

            await repo.AddAsync(new Taska
            {
                Title = "Task 2",
                Description = "B",
                DueDate = DateTime.UtcNow,
                CreatorId = user.Id
            });

            await context.SaveChangesAsync(); 

            Expression<Func<Taska, bool>> filter = t => t.Title.Contains("1");
            var results = await repo.FindAsync(filter);

            Assert.Single(results);
        }

        [Fact]
        public async Task GetFilteredAndSortedAsync_Test()
        {
            var context = TestDbContextFactory.Create();
            var repo = new TaskRepository(context);

            var user = new User { UserName = "u3" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var task1 = new Taska
            {
                Title = "A",
                Description = "D",
                DueDate = DateTime.UtcNow.AddDays(1),
                Priority = TaskPriority.Medium,
                CreatorId = user.Id
            };
            var task2 = new Taska
            {
                Title = "B",
                Description = "D",
                DueDate = DateTime.UtcNow.AddDays(2),
                Priority = TaskPriority.High,
                CreatorId = user.Id
            };

            await repo.AddAsync(task1);
            await repo.AddAsync(task2);
            await context.SaveChangesAsync(); 

            var result = await repo.GetFilteredAndSortedAsync(
                t => t.Description == "D",
                "priority",
                true,
                1,
                10
            );

            Assert.Equal(2, result.Count());
            Assert.Equal(TaskPriority.High, result.First().Priority);
        }
    }
}
