using System;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Infrastructure.Repositories;
using Xunit;

namespace Test
{
    public class CommentRepositoryTests
    {
        [Fact]
        public async Task GetCommentsForTaskAsync_ShouldReturnOnlyCommentsForGivenTask()
        {
            var context = TestDbContextFactory.Create();
            var repo = new CommentRepository(context);

            var user = new User { UserName = "test_user1" };
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

            var comment1 = new Comment { Text = "Comment for Task A1", TaskaId = task1.Id, AuthorId = user.Id };
            var comment2 = new Comment { Text = "Comment for Task A2", TaskaId = task1.Id, AuthorId = user.Id };
            var comment3 = new Comment { Text = "Comment for Task B", TaskaId = task2.Id, AuthorId = user.Id };

            await repo.AddAsync(comment1);
            await repo.AddAsync(comment2);
            await repo.AddAsync(comment3);

            var commentsForTask1 = await repo.GetCommentsForTaskAsync(task1.Id);

            Assert.Equal(2, commentsForTask1.Count());
            Assert.All(commentsForTask1, c => Assert.Equal(task1.Id, c.TaskaId));
        }

        [Fact]
        public async Task GetAllCommentsAsync_ShouldReturnAllCommentsSortedByCreatedAtDescending()
        {
            var context = TestDbContextFactory.Create();
            var repo = new CommentRepository(context);

            var user = new User { UserName = "test_user2" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var task = new Taska
            {
                Title = "Task C",
                Description = "Description C",
                DueDate = DateTime.UtcNow,
                CreatorId = user.Id
            };
            context.Tasks.Add(task);
            await context.SaveChangesAsync();

            var commentOld = new Comment
            {
                Text = "Old Comment",
                CreatedAt = DateTime.UtcNow.AddMinutes(-10),
                TaskaId = task.Id,
                AuthorId = user.Id
            };
            var commentMid = new Comment
            {
                Text = "Mid Comment",
                CreatedAt = DateTime.UtcNow.AddMinutes(-5),
                TaskaId = task.Id,
                AuthorId = user.Id
            };
            var commentNew = new Comment
            {
                Text = "New Comment",
                CreatedAt = DateTime.UtcNow,
                TaskaId = task.Id,
                AuthorId = user.Id
            };

            await repo.AddAsync(commentOld);
            await repo.AddAsync(commentMid);
            await repo.AddAsync(commentNew);

            var allComments = (await repo.GetAllCommentsAsync()).ToList();

            Assert.Equal(3, allComments.Count);
            Assert.Equal("New Comment", allComments[0].Text);
            Assert.Equal("Mid Comment", allComments[1].Text);
            Assert.Equal("Old Comment", allComments[2].Text);
        }
    }
}
