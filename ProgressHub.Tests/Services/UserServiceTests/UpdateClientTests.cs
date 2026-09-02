using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProgressHub.Core.Models;
using ProgressHub.Core.Models.DTOs.ClientDTOs;
using ProgressHub.Core.Models.Enums;
using ProgressHub.Data.Services;
using ProgressHub.Tests.Common;


namespace ProgressHub.Tests.Services.UserServiceTests
{
    public class UpdateClientTests
    {
        /// <summary>
        /// Ověřuje, že metoda <see cref="UserService.UpdateClientAsync"/> správně aktualizuje
        /// profilové údaje i makro cíle existujícího klienta v databázi z předaného DTO.
        /// </summary>
        [Fact]
        public async Task UpdateClientAsync_ShouldUpdateProfileAndMacros_WhenClientExists()
        {
            // Arrange
            var factory = TestDbContextFactory.Create();
            int clientId;

            var originalClient = new User
            {
                FirstName = "Původní",
                LastName = "Jméno",
                Email = "puvodni@test.cz",
                UserRole = UserRole.Client,
                DateOfBirth = new DateOnly(1995, 1, 1),
                Gender = Gender.Male,
                FitnessGoal = FitnessGoal.WeightLoss,
                HeightInCm = 180,
                TargetCalories = 2000,
                TargetProteinGrams = 150,
                TargetCarbsGrams = 200,
                TargetFatsGrams = 60
            };

            await using (var seed = await factory.CreateDbContextAsync())
            {
                seed.Users.Add(originalClient);
                await seed.SaveChangesAsync();
                clientId = originalClient.Id;
            }

            var sut = new UserService(factory);

            var updateDto = new UpdateClientDto
            {
                Id = clientId,
                FirstName = "Nové",
                LastName = "Příjmení",
                Email = "nove@test.cz",
                DateOfBirth = new DateOnly(1996, 5, 20),
                Gender = Gender.Female,
                FitnessGoal = FitnessGoal.MuscleGain,
                HeightInCm = 182,
                TargetCalories = 2500,
                TargetProteinGrams = 180,
                TargetCarbsGrams = 260,
                TargetFatsGrams = 75
            };

            // Act
            await sut.UpdateClientAsync(updateDto);

            // Assert
            await using var context = await factory.CreateDbContextAsync();
            var stored = await context.Users.SingleAsync(u => u.Id == clientId);

            // Zkontroluje, že všechna pole z UpdateClientDto se promítla do entity
            stored.Should().BeEquivalentTo(updateDto, options => options.ExcludingMissingMembers());

            // Ověří, že role nebyla změněna a zůstala Client
            stored.UserRole.Should().Be(UserRole.Client);
        }


        /// <summary>
        /// Ověřuje, že metoda <see cref="UserService.UpdateClientAsync"/> vyhodí výjimku
        /// <see cref="KeyNotFoundException"/>, pokud zadané ID klienta v DTO v databázi neexistuje.
        /// </summary>
        [Fact]
        public async Task UpdateClientAsync_ShouldThrowKeyNotFoundException_WhenClientDoesNotExist()
        {
            // Arrange
            var factory = TestDbContextFactory.Create();
            var sut = new UserService(factory);

            var nonExistentClientDto = new UpdateClientDto
            {
                Id = 99999,
                FirstName = "Neexistující",
                LastName = "Klient",
                Email = "none@test.cz"
            };

            // Act
            var act = async () => await sut.UpdateClientAsync(nonExistentClientDto);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("*99999*");
        }

        /// <summary>
        /// Ověřuje bezpečnostní pravidlo: metoda <see cref="UserService.UpdateClientAsync"/> vyhodí výjimku
        /// <see cref="KeyNotFoundException"/>, pokud se pokusíme aktualizovat uživatele s rolí trenéra (Coach).
        /// </summary>
        [Fact]
        public async Task UpdateClientAsync_ShouldThrowKeyNotFoundException_WhenUserIsNotClient()
        {
            // Arrange
            var factory = TestDbContextFactory.Create();
            int coachId;

            await using (var seed = await factory.CreateDbContextAsync())
            {
                var coach = new User
                {
                    FirstName = "Trenér",
                    LastName = "Hlavní",
                    Email = "trener@gym.cz",
                    UserRole = UserRole.Coach
                };
                seed.Users.Add(coach);
                await seed.SaveChangesAsync();
                coachId = coach.Id;
            }

            var sut = new UserService(factory);

            var updateAttemptDto = new UpdateClientDto
            {
                Id = coachId,
                FirstName = "Hacknutý",
                LastName = "Profil",
                Email = "hack@gym.cz"
            };

            // Act
            var act = async () => await sut.UpdateClientAsync(updateAttemptDto);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage($"*{coachId}*");
        }
    }
}
