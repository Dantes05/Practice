using Domain.Entities;
using Infrastructure.Repositories;
using Xunit;

namespace Test
{
    public class BaseRepositoryTests
    {
        [Fact]
        public async Task AddAsync_Should_Add_Entity()
        {
            var context = TestDbContextFactory.Create();
            var repo = new BaseRepository<User>(context);

            var user = new User { UserName = "add_test" };
            await repo.AddAsync(user);

            var fetched = await repo.GetByIdAsync(user.Id);
            Assert.NotNull(fetched);
            Assert.Equal("add_test", fetched.UserName);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_If_Not_Exists()
        {
            var context = TestDbContextFactory.Create();
            var repo = new BaseRepository<User>(context);

            var result = await repo.GetByIdAsync("non_existing_id");
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_All_Entities()
        {
            var context = TestDbContextFactory.Create();
            var repo = new BaseRepository<User>(context);

            await repo.AddAsync(new User { UserName = "user1" });
            await repo.AddAsync(new User { UserName = "user2" });

            var all = (await repo.GetAllAsync()).ToList();
            Assert.True(all.Count >= 2);
            Assert.Contains(all, u => u.UserName == "user1");
            Assert.Contains(all, u => u.UserName == "user2");
        }

        [Fact]
        public async Task UpdateAsync_Should_Modify_Entity()
        {
            var context = TestDbContextFactory.Create();
            var repo = new BaseRepository<User>(context);

            var user = new User { UserName = "before_update" };
            await repo.AddAsync(user);

            user.UserName = "after_update";
            await repo.UpdateAsync(user);

            var updated = await repo.GetByIdAsync(user.Id);
            Assert.Equal("after_update", updated.UserName);
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_Entity()
        {
            var context = TestDbContextFactory.Create();
            var repo = new BaseRepository<User>(context);

            var user = new User { UserName = "to_delete" };
            await repo.AddAsync(user);

            await repo.DeleteAsync(user);

            var deleted = await repo.GetByIdAsync(user.Id);
            Assert.Null(deleted);
        }
    }
}
