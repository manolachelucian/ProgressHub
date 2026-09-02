using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProgressHub.Core.Interfaces;
using ProgressHub.Core.Models;
using ProgressHub.Core.Models.DTOs.ClientDTOs;
using ProgressHub.Core.Models.DTOs.DailyLogDTOs;
using ProgressHub.Core.Models.Enums;
using ProgressHub.Data.Services;
using ProgressHub.Tests.Common;


namespace ProgressHub.Tests.Services.UserServiceTests
{
    public class AddClientTests
    {

        /// <summary>
        /// Ověřuje, že metoda <see cref="UserService.AddClientAsync"/> správně uloží nového klienta z DTO
        /// a nastaví roli <see cref="UserRole.Client"/>.
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
            // Arrange
            var factory = TestDbContextFactory.Create();
            IUserService userService = new UserService(factory);

            var clientDto = new CreateClientDto
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                TargetCalories = targetCalories,
                HeightInCm = 175,
                DateOfBirth = new DateOnly(1995, 5, 10),
                Gender = Gender.Male,
                FitnessGoal = FitnessGoal.WeightLoss
            };

            // Act
            await userService.AddClientAsync(clientDto);

            // Assert
            await using var context = await factory.CreateDbContextAsync();
            var stored = await context.Users.SingleAsync(u => u.Email == email);

            // Porovná všechny shodné property mezi User entitou a CreateClientDto a ignoruje navigační/DB vlastnosti navíc
            stored.Should().BeEquivalentTo(clientDto, options => options.ExcludingMissingMembers());

            // Explicitní ověření vynucené role
            stored.UserRole.Should().Be(UserRole.Client);
            stored.Id.Should().BeGreaterThan(0);
        }



        /// <summary>
        /// Ověřuje, že metoda <see cref="UserService.AddDailyLogAsync"/> správně vytvoří a vloží nový záznam z DTO,
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
            // Arrange
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
            var logDto = new CreateDailyLogDto
            {
                UserId = client.Id,
                Date = new DateOnly(2026, 8, 25),
                Weight = weight,
                ConsumedCalories = calories,
                ConsumedProteins = proteins,
                ConsumedCarbs = carbs,
                ConsumedFats = fats,
                TrainingType = TrainingType.FullBody,
                Note = "Great workout"
            };

            // Act
            await sut.AddDailyLogAsync(logDto);

            // Assert
            await using var context = await factory.CreateDbContextAsync();
            var stored = await context.DailyLog.SingleAsync();

            stored.Id.Should().BeGreaterThan(0);
            stored.Should().BeEquivalentTo(logDto, options => options.ExcludingMissingMembers());
        }



        /// <summary>
        /// Ověřuje, že metoda <see cref="UserService.AddDailyLogAsync"/> vyhodí výjimku <see cref="KeyNotFoundException"/>,
        /// pokud je zadáno ID uživatele v DTO, které v databázi neexistuje.
        /// </summary>
        [Fact]
        public async Task AddDailyLogAsync_ShouldThrowKeyNotFoundException_WhenUserDoesNotExist()
        {
            var factory = TestDbContextFactory.Create();
            var sut = new UserService(factory);

            var logDto = new CreateDailyLogDto
            {
                UserId = 999,
                Date = new DateOnly(2026, 8, 25),
                Weight = 80,
                ConsumedCalories = 2000,
                ConsumedProteins = 150,
                ConsumedCarbs = 200,
                ConsumedFats = 60,
                TrainingType = TrainingType.RestDay
            };

            var act = async () => await sut.AddDailyLogAsync(logDto);

            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("*999*");
        }

        /// <summary>
        /// Ověřuje upsert chování: při opakovaném volání <see cref="UserService.AddDailyLogAsync"/>
        /// pro stejného uživatele a stejné datum dojde k aktualizaci stávajícího záznamu z DTO namísto vytvoření duplicity.
        /// </summary>
        [Fact]
        public async Task AddDailyLogAsync_ShouldUpdateExisting_WhenSameUserAndDate_NotDuplicate()
        {
            // Arrange
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

            var initialDto = new CreateDailyLogDto
            {
                UserId = client.Id,
                Date = date,
                Weight = 80,
                ConsumedCalories = 2000,
                ConsumedProteins = 150,
                ConsumedCarbs = 200,
                ConsumedFats = 60,
                TrainingType = TrainingType.RestDay
            };

            var updatedDto = new CreateDailyLogDto
            {
                UserId = client.Id,
                Date = date,
                Weight = 79.5,
                ConsumedCalories = 2100,
                ConsumedProteins = 160,
                ConsumedCarbs = 210,
                ConsumedFats = 65,
                TrainingType = TrainingType.Push,
                Note = "Updated workout day"
            };

            // Act
            await sut.AddDailyLogAsync(initialDto);
            await sut.AddDailyLogAsync(updatedDto);

            // Assert
            await using var context = await factory.CreateDbContextAsync();
            var logs = await context.DailyLog.Where(l => l.UserId == client.Id).ToListAsync();

            logs.Should().ContainSingle();
            var stored = logs.Single();

            stored.Should().BeEquivalentTo(updatedDto, options => options.ExcludingMissingMembers());
        }

    }
}
