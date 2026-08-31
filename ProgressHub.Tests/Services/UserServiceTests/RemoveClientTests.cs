using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProgressHub.Core.Interfaces;
using ProgressHub.Core.Models;
using ProgressHub.Core.Models.Enums;
using ProgressHub.Data.Services;
using ProgressHub.Tests.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgressHub.Tests.Services.UserServiceTests
{
    public class RemoveClientTests
    {

        /// <summary>
        ///  Kontrole jestli skutecne client vymazal
        /// </summary>
        /// <returns></returns>

        [Fact]
        public async Task RemoveClientAsync_ClientIsNotInTheList_WhenHeIsRemoved()
        {
            //arrange
            var factory = TestDbContextFactory.Create();
            int clientId;

            var client = new User
            {
                FirstName = "Test",
                LastName = "Test",
                Email = "test@test.cz",
                UserRole = UserRole.Client,
                DailyLogs = new List<DailyLog>{ new() { Date = new DateOnly(2026, 3, 1), Weight = 82.0, ConsumedCalories = 2400 }
            }
            };

            await using (var seed = await factory.CreateDbContextAsync())
            {
                seed.Users.Add(client);
                await seed.SaveChangesAsync();
                clientId = client.Id; // Získáme automaticky vygenerované ID
            }

            IUserService userService = new UserService(factory);

            // Act
            await userService.RemoveClientAsync(clientId);

            // Assert
            await using var context = await factory.CreateDbContextAsync();

            var deletedUser = await context.Users.FirstOrDefaultAsync(u => u.Id == clientId);
            var remainingLogs = await context.DailyLog.Where(l => l.UserId == clientId).ToListAsync();

            deletedUser.Should().BeNull();
            remainingLogs.Should().BeEmpty();
        }


        /// <summary>
        /// Ověřuje, že volání <see cref="UserService.RemoveClientAsync"/> pro neexistující ID
        /// vyhodí výjimku <see cref="KeyNotFoundException"/> (případně vrátí false dle zvoleného rozhraní).
        /// </summary>
        [Fact]
        public async Task RemoveClientAsync_ShouldThrowKeyNotFoundException_WhenClientDoesNotExist()
        {
            // Arrange
            var factory = TestDbContextFactory.Create();
            IUserService userService = new UserService(factory);

            // Act
            var act = async () => await userService.RemoveClientAsync(99999);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }
    }
}
