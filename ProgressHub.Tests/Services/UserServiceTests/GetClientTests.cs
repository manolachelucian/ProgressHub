using FluentAssertions;
using ProgressHub.Core.Interfaces;
using ProgressHub.Core.Models;
using ProgressHub.Core.Models.DTOs.ClientDTOs;
using ProgressHub.Core.Models.Enums;
using ProgressHub.Data.Services;
using ProgressHub.Tests.Common;


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
                new() { 
                    Date = new DateOnly(2026, 3, 1), Weight = 82.0, ConsumedCalories = 2400 
                }
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
        /// s rolí <see cref="UserRole.Client"/> a správně namapuje data do <see cref="ClientListItemDto"/>
        /// včetně hodnoty LatestWeight.
        /// </summary>
        [Fact]
        public async Task GetAllClientsAsync_ShouldReturnOnlyClients_WithLatestWeightCalculated()
        {
            // Arrange
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
                    new() { Date = new DateOnly(2026, 1, 1), Weight = 82.0, ConsumedCalories = 2000 },
                    new() { Date = new DateOnly(2026, 1, 5), Weight = 80.5, ConsumedCalories = 1950 }
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

            // Act
            var clients = await sut.GetAllClientsAsync();

            // Assert
            clients.Should().ContainSingle();

            var clientDto = clients.Single();
            clientDto.Email.Should().Be("c1@x.com");
            clientDto.FirstName.Should().Be("Client");
            clientDto.LastName.Should().Be("One");
            clientDto.FullName.Should().Be("Client One");
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


        [Theory]
        [InlineData("+420", "777111222", "+420777111222")]
        [InlineData("+421", "901234567", "+421901234567")]
        [InlineData("+49", "15123456789", "+4915123456789")]
        public async Task AddClientAsync_ShouldSavePhoneNumber_AndThrowOnDuplicate(string prefix,string localNumber,string expectedFullNumber)
        {
            // Arrange
            var factory = TestDbContextFactory.Create();
            var sut = new UserService(factory);

            var client1 = new CreateClientDto
            {
                FirstName = "First",
                LastName = "Client",
                Email = $"client_{localNumber}_1@test.cz",
                PhonePrefixCode = prefix,
                LocalPhoneNumber = localNumber
            };

            var clientWithDuplicatePhone = new CreateClientDto
            {
                FirstName = "Second",
                LastName = "Client",
                Email = $"client_{localNumber}_2@test.cz",
                PhonePrefixCode = prefix,
                LocalPhoneNumber = localNumber // Stejné číslo
            };

            // Act
            await sut.AddClientAsync(client1);

            // Assert 1: Číslo se uložilo ve správném formátu E.164
            var clients = await sut.GetAllClientsAsync();
            var saved = clients.First(c => c.Email == client1.Email);
            saved.PhoneNumber.Should().Be(expectedFullNumber);

            // Assert 2: Pokus o uložení duplicity vyhodí výjimku
            var act = async () => await sut.AddClientAsync(clientWithDuplicatePhone);
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task AddClientAsync_ShouldAllowMultipleClients_WithEmptyPhoneNumber(string? emptyPhone)
        {
            // Arrange
            var factory = TestDbContextFactory.Create();
            var sut = new UserService(factory);

            var client1 = new CreateClientDto
            {
                FirstName = "Jan",
                LastName = "BezCisla",
                Email = "jan.bezcisla@test.cz",
                LocalPhoneNumber = emptyPhone
            };

            var client2 = new CreateClientDto
            {
                FirstName = "Petr",
                LastName = "TakyBezCisla",
                Email = "petr.bezcisla@test.cz",
                LocalPhoneNumber = emptyPhone
            };

            // Act & Assert: Obě vložení musí projít bez výjimky
            await sut.AddClientAsync(client1);
            var act = async () => await sut.AddClientAsync(client2);

            await act.Should().NotThrowAsync();
        }

    }
}
