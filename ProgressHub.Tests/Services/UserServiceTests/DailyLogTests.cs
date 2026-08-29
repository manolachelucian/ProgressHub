using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProgressHub.Core.Enums;
using ProgressHub.Core.Models;
using ProgressHub.Data.Services;
using ProgressHub.Tests.Common;


namespace ProgressHub.Tests.Services.UserServiceTests
{
    public class DailyLogTests
    {
        [Fact]
        public async Task UpdateDailyLogAsync_ShouldUpdateLogValues_WhenLogExists()
        {
            // Arrange
            var factory = TestDbContextFactory.Create();
            int logId;

            var client = new User
            {
                FirstName = "Jan",
                LastName = "Novák",
                Email = "jan@novak.cz",
                UserRole = UserRole.Client,
                DailyLogs = new List<DailyLog>
            {
                new() { Date = new DateOnly(2026, 3, 1), Weight = 80.0, ConsumedCalories = 2200 }
            }
            };

            await using (var seed = await factory.CreateDbContextAsync())
            {
                seed.Users.Add(client);
                await seed.SaveChangesAsync();
                logId = client.DailyLogs.First().Id;
            }

            var sut = new UserService(factory);

            var updatedLog = new DailyLog
            {
                Id = logId,
                Date = new DateOnly(2026, 3, 1),
                Weight = 79.2,
                ConsumedCalories = 2100,
                ConsumedProteins = 160,
                ConsumedCarbs = 210,
                ConsumedFats = 65,
                Note = "Updated note"
            };

            // Act
            await sut.UpdateDailyLogAsync(updatedLog);

            // Assert
            await using var context = await factory.CreateDbContextAsync();
            var stored = await context.DailyLog.SingleAsync(l => l.Id == logId);

            stored.Weight.Should().Be(79.2);
            stored.ConsumedCalories.Should().Be(2100);
            stored.ConsumedProteins.Should().Be(160);
            stored.Note.Should().Be("Updated note");
        }

        [Fact]
        public async Task RemoveDailyLogAsync_ShouldDeleteLog_WhenLogExists()
        {
            // Arrange
            var factory = TestDbContextFactory.Create();
            int logId;

            var client = new User
            {
                FirstName = "Petr",
                LastName = "Svoboda",
                Email = "petr@svoboda.cz",
                UserRole = UserRole.Client,
                DailyLogs = new List<DailyLog>
            {
                new() { Date = new DateOnly(2026, 3, 1), Weight = 85.0, ConsumedCalories = 2500 }
            }
            };

            await using (var seed = await factory.CreateDbContextAsync())
            {
                seed.Users.Add(client);
                await seed.SaveChangesAsync();
                logId = client.DailyLogs.First().Id;
            }

            var sut = new UserService(factory);

            // Act
            await sut.RemoveDailyLogAsync(logId);

            // Assert
            await using var context = await factory.CreateDbContextAsync();
            var stored = await context.DailyLog.FirstOrDefaultAsync(l => l.Id == logId);

            stored.Should().BeNull();
        }
    }
}
