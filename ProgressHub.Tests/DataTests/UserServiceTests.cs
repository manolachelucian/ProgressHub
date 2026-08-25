using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProgressHub.Core.Enums;
using ProgressHub.Core.Interfaces;
using ProgressHub.Core.Models;
using ProgressHub.Data;
using ProgressHub.Data.Services;
using Xunit;


namespace ProgressHub.Tests.DataTests
{
    public class UserServiceTests 
    {

        private static IDbContextFactory<ProgressHubDbContext> CreateFactory()
        {
            var services = new ServiceCollection();
            services.AddDbContextFactory<ProgressHubDbContext>(options =>
                options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

            return services.BuildServiceProvider()
                .GetRequiredService<IDbContextFactory<ProgressHubDbContext>>();
        }

        [Theory]
        [InlineData("Alice", "Smith", "alice@example.com", 1800)]
        [InlineData("Bob", "Jones", "bob@example.com", 2600)]
        public async Task AddClientAsync_ShouldPersistClient_AndForceClientRole(
            string firstName, string lastName, string email, int targetCalories)
        {
            //arrange
            var factory = CreateFactory();
            IUserService userService =  new UserService(factory);

            var expectedClient = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                TargetCalories = targetCalories,
                UserRole = UserRole.Coach // deliberately wrong on purpose
            };

            //ACT
            await userService.AddClientAsync(expectedClient);

            //Assert

            await using var context = await factory.CreateDbContextAsync();
            var stored = await context.Users.SingleAsync(u => u.Email == email);

            stored.Should().BeEquivalentTo(expectedClient, options => options.Excluding(u => u.Id)
            .Excluding(u => u.UserRole));
             
            stored.UserRole.Should().Be(UserRole.Client);

        }

        [Fact]
        public async Task GetAllClientsAsync_ShouldReturnOnlyClients_WithDailyLogsIncluded()
        {
            var factory = CreateFactory();

            await using (var seed = await factory.CreateDbContextAsync())
            {
                seed.Users.AddRange(
                    new User
                    {
                        FirstName = "Client",
                        LastName = "One",
                        Email = "c1@x.com",
                        UserRole = UserRole.Client,
                        TargetCalories = 2000,
                        DailyLogs = new List<DailyLog>
                        {
                            new() { Date = new DateOnly(2026, 1, 1), Weight = 80, ConsumedCalories = 2000 }
                        }
                    },
                    new User
                    {
                        FirstName = "Coach",
                        LastName = "Two",
                        Email = "coach@x.com",
                        UserRole = UserRole.Coach,
                        TargetCalories = 0
                    });
                await seed.SaveChangesAsync();
            }

            var sut = new UserService(factory);
            var clients = await sut.GetAllClientsAsync();

            clients.Should().ContainSingle(u => u.Email == "c1@x.com");
            clients.Single().DailyLogs.Should().ContainSingle();
        }

        [Fact]
        public async Task GetClientByIdAsync_ShouldReturnNull_WhenUserIsNotClientRole()
        {
            var factory = CreateFactory();
            int coachId;

            await using (var seed = await factory.CreateDbContextAsync())
            {
                var coach = new User
                {
                    FirstName = "Coach", LastName = "Only", Email = "coach@x.com",
                    UserRole = UserRole.Coach, TargetCalories = 0
                };
                seed.Users.Add(coach);
                await seed.SaveChangesAsync();
                coachId = coach.Id;
            }

            IUserService userService = new UserService(factory);
            var result = await userService.GetClientByIdAsync(coachId);

            result.Should().BeNull();
        }

        [Theory]
        [InlineData(70.5, 2000, 150, 200, 60)]
        [InlineData(85.2, 2600, 180, 260, 80)]
        public async Task AddDailyLogAsync_ShouldInsert_WhenNoExistingLogForDate(
            double weight, int calories, int proteins, int carbs, int fats)
        {
            var factory = CreateFactory();
            var client = new User
            {
                FirstName = "Client",
                LastName = "X",
                Email = "clientx@x.com",
                UserRole = UserRole.Client,
                TargetCalories = 2000
            };

            await using (var seed = await factory.CreateDbContextAsync())
            {
                seed.Users.Add(client);
                await seed.SaveChangesAsync();
            }

            var sut = new UserService(factory);
            var log = new DailyLog
            {
                UserId = client.Id,
                Date = new DateOnly(2026, 8, 25),
                Weight = weight,
                ConsumedCalories = calories,
                ConsumedProteins = proteins,
                ConsumedCarbs = carbs,
                ConsumedFats = fats
            };

            await sut.AddDailyLogAsync(log);

            await using var context = await factory.CreateDbContextAsync();
            var stored = await context.DailyLog.SingleAsync();

            stored.Should().BeEquivalentTo(log, options => options.Excluding(l => l.Id));
        }


        [Fact]
        public async Task AddDailyLogAsync_ShouldThrowKeyNotFoundException_WhenUserDoesNotExist()
        {
            var factory = CreateFactory();
            var sut = new UserService(factory);

            var log = new DailyLog
            {
                UserId = 999,
                Date = new DateOnly(2026, 8, 25),
                Weight = 80,
                ConsumedCalories = 2000,
                ConsumedProteins = 150,
                ConsumedCarbs = 200,
                ConsumedFats = 60
            };

            var act = async () => await sut.AddDailyLogAsync(log);

            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        

        [Fact]
        public async Task AddDailyLogAsync_ShouldUpdateExisting_WhenSameUserAndDate_NotDuplicate()
        {
            var factory = CreateFactory();
            var client = new User
            {
                FirstName = "Client",
                LastName = "Y",
                Email = "clienty@x.com",
                UserRole = UserRole.Client,
                TargetCalories = 2000
            };

            await using (var seed = await factory.CreateDbContextAsync())
            {
                seed.Users.Add(client);
                await seed.SaveChangesAsync();
            }

            var sut = new UserService(factory);
            var date = new DateOnly(2026, 8, 25);

            await sut.AddDailyLogAsync(new DailyLog
            {
                UserId = client.Id,
                Date = date,
                Weight = 80,
                ConsumedCalories = 2000,
                ConsumedProteins = 150,
                ConsumedCarbs = 200,
                ConsumedFats = 60
            });
            await sut.AddDailyLogAsync(new DailyLog
            {
                UserId = client.Id,
                Date = date,
                Weight = 79.5,
                ConsumedCalories = 2100,
                ConsumedProteins = 160,
                ConsumedCarbs = 210,
                ConsumedFats = 65
            });

            await using var context = await factory.CreateDbContextAsync();
            var logs = await context.DailyLog.Where(l => l.UserId == client.Id).ToListAsync();

            logs.Should().ContainSingle();
            logs.Single().Weight.Should().Be(79.5);
            logs.Single().ConsumedCalories.Should().Be(2100);
        }



    }
}
