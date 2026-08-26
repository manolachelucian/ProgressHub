using FluentAssertions;
using Microsoft.EntityFrameworkCore;
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
    public class AddClientTests
    {

        /// <summary>
        /// Ověřuje, že metoda <see cref="UserService.AddClientAsync"/> správně uloží nového klienta
        /// a vynutí roli <see cref="UserRole.Client"/> bez ohledu na původně zadanou roli.
        /// </summary>
        /// <param name="firstName">Křestní jméno klienta.</param>
        /// <param name="lastName">Příjmení klienta.</param>
        /// <param name="email">Emailová adresa klienta.</param>
        /// <param name="targetCalories">Cílový denní kalorický příjem.</param>
        [Theory]
        [InlineData("Alice", "Smith", "alice@example.com", 1800)]
        [InlineData("Bob", "Jones", "bob@example.com", 2600)]
        public async Task AddClientAsync_ShouldPersistClient_AndForceClientRole(
            string firstName, string lastName, string email, int targetCalories)
        {
            //arrange
            var factory = TestDbContextFactory.Create();
            IUserService userService = new UserService(factory);

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



        /// <summary>
        /// Ověřuje, že metoda <see cref="UserService.AddDailyLogAsync"/> správně vytvoří a vloží nový záznam,
        /// pokud pro daného klienta a datum ještě žádný log neexistuje.
        /// </summary>
        /// <param name="weight">Váha klienta v kg.</param>
        /// <param name="calories">Zkonzumované kalorie.</param>
        /// <param name="proteins">Příjem bílkovin v gramech.</param>
        /// <param name="carbs">Příjem sacharidů v gramech.</param>
        /// <param name="fats">Příjem tuků v gramech.</param>
        [Theory]
        [InlineData(70.5, 2000, 150, 200, 60)]
        [InlineData(85.2, 2600, 180, 260, 80)]
        public async Task AddDailyLogAsync_ShouldInsert_WhenNoExistingLogForDate(
            double weight, int calories, int proteins, int carbs, int fats)
        {
            var factory = TestDbContextFactory.Create();
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

        /// <summary>
        /// Ověřuje, že metoda <see cref="UserService.AddDailyLogAsync"/> vyhodí výjimku <see cref="KeyNotFoundException"/>,
        /// pokud je zadáno ID uživatele, které v databázi neexistuje.
        /// </summary>
        [Fact]
        public async Task AddDailyLogAsync_ShouldThrowKeyNotFoundException_WhenUserDoesNotExist()
        {
            var factory = TestDbContextFactory.Create();
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

        /// <summary>
        /// Ověřuje upsert chování: při opakovaném volání <see cref="UserService.AddDailyLogAsync"/>
        /// pro stejného uživatele a stejné datum dojde k aktualizaci stávajícího záznamu namísto vytvoření duplicity.
        /// </summary>
        [Fact]
        public async Task AddDailyLogAsync_ShouldUpdateExisting_WhenSameUserAndDate_NotDuplicate()
        {
            var factory = TestDbContextFactory.Create();
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
