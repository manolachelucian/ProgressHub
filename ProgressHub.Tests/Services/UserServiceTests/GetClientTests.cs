using FluentAssertions;
using ProgressHub.Core.Enums;
using ProgressHub.Core.Interfaces;
using ProgressHub.Core.Models;
using ProgressHub.Data.Services;
using ProgressHub.Tests.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgressHub.Tests.Services.UserServiceTests
{
    public class GetClientTests
    {

        /// <summary>
        /// Ověřuje, že metoda <see cref="UserService.GetAllClientsAsync"/> vrátí pouze uživatele
        /// s rolí <see cref="UserRole.Client"/> a správně načte (Include) jejich přiřazené denní logy.
        /// </summary>
        [Fact]
        public async Task GetClientByIdAsync_ShouldReturnClientWithLogs_WhenClientExists()
        {
            // Arrange
            var factory = TestDbContextFactory.Create();
            int clientId;

            await using (var seed = await factory.CreateDbContextAsync())
            {
                var client = new User
                {
                    FirstName = "Jan",
                    LastName = "Novák",
                    Email = "jan@novak.cz",
                    UserRole = UserRole.Client,
                    DailyLogs = new List<DailyLog>
            {
                new() { Date = new DateOnly(2026, 3, 1), Weight = 82.0, ConsumedCalories = 2400 }
            }
                };
                seed.Users.Add(client);
                await seed.SaveChangesAsync();
                clientId = client.Id;
            }

            IUserService userService = new UserService(factory);

            // Act
            var result = await userService.GetClientByIdAsync(clientId);

            // Assert
            result.Should().NotBeNull();
            result!.FirstName.Should().Be("Jan");
            result.DailyLogs.Should().ContainSingle();
            result.DailyLogs.First().Weight.Should().Be(82.0);
        }


        /// <summary>
        /// Ověřuje, že metoda <see cref="UserService.GetClientByIdAsync"/> vrátí <c>null</c>,
        /// pokud uživatel s daným ID existuje, ale má jinou roli než <see cref="UserRole.Client"/>.
        /// </summary>
        [Fact]
        public async Task GetClientByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var factory = TestDbContextFactory.Create();
            IUserService userService = new UserService(factory);

            // Act
            var result = await userService.GetClientByIdAsync(99999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllClientsAsync_ShouldReturnEmptyList_WhenNoClientsExist()
        {
            // Arrange
            var factory = TestDbContextFactory.Create();
            IUserService userService = new UserService(factory);

            // Act
            var clients = await userService.GetAllClientsAsync();

            // Assert
            clients.Should().BeEmpty();
        }


        /// <summary>
        /// Ověřuje, že metoda <see cref="UserService.GetAllClientsAsync"/> vrátí pouze uživatele
        /// s rolí <see cref="UserRole.Client"/> a správně načte (Include) jejich přiřazené denní logy.
        /// </summary>
        [Fact]
        public async Task GetAllClientsAsync_ShouldReturnOnlyClients_WithDailyLogsIncluded()
        {
            var factory = TestDbContextFactory.Create();

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


        /// <summary>
        /// Ověřuje, že metoda <see cref="UserService.GetClientByIdAsync"/> vrátí <c>null</c>,
        /// pokud uživatel s daným ID existuje, ale má jinou roli než <see cref="UserRole.Client"/>.
        /// </summary>
        [Fact]
        public async Task GetClientByIdAsync_ShouldReturnNull_WhenUserIsNotClientRole()
        {
            var factory = TestDbContextFactory.Create();
            int coachId;

            await using (var seed = await factory.CreateDbContextAsync())
            {
                var coach = new User
                {
                    FirstName = "Coach",
                    LastName = "Only",
                    Email = "coach@x.com",
                    UserRole = UserRole.Coach,
                    TargetCalories = 0
                };
                seed.Users.Add(coach);
                await seed.SaveChangesAsync();
                coachId = coach.Id;
            }

            IUserService userService = new UserService(factory);
            var result = await userService.GetClientByIdAsync(coachId);

            result.Should().BeNull();
        }

    }
}
