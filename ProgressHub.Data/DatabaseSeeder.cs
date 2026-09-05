using Bogus;
using ProgressHub.Core.Models;
using ProgressHub.Core.Models.Enums;
namespace ProgressHub.Data
{
    public class DatabaseSeeder
    {

        public static List<User> GenerateClients(int clientCount , int daysOfLogs)
        {
            var random = new Random(42);
            var clientFaker = new Faker<User>()
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
                .RuleFor(u => u.Gender, f => f.PickRandom(Gender.Male,Gender.Female))
                .RuleFor(u => u.CreatedAt, f => f.Date.Recent(45))
                .RuleFor(u => u.HeightInCm, f => f.Random.Int(155, 205))
                .RuleFor(u => u.DateOfBirth, f => DateOnly.FromDateTime(f.Date.Between(DateTime.UtcNow.AddYears(-42), DateTime.UtcNow.AddYears(-20))))
                .RuleFor(u => u.FitnessGoal, f => f.PickRandom<FitnessGoal>())
                .RuleFor(u => u.TargetCalories, 1200)
                .RuleFor(u => u.TargetProteinGrams, 160)
                .RuleFor(u => u.TargetCarbsGrams, 220)
                .RuleFor(u => u.TargetFatsGrams, 70);


            var clients = clientFaker.Generate(clientCount);


            foreach (var client in clients)
            {
                double startWeight = client.Gender == Gender.Male ? random.Next(80, 100) : random.Next(60, 75);
                var today = DateOnly.FromDateTime(DateTime.Today);

                for (int i = daysOfLogs; i >= 0; i--)
                {
                    
                    if (random.NextDouble() < 0.15 && i != 0) continue;

                    var logDate = today.AddDays(-i);
                    
                    
                    startWeight += (random.NextDouble() - 0.55) * 0.4;

                    client.DailyLogs.Add(new DailyLog
                    {
                        Date = logDate,
                        Weight = Math.Round(startWeight, 1),
                        ConsumedCalories = random.Next(1700, 3000),
                        ConsumedProteins = random.Next(140, 220),
                        ConsumedCarbs = random.Next(180, 250),
                        ConsumedFats = random.Next(50, 80),
                        TrainingType = (TrainingType)random.Next(0, 8),
                        Note = random.NextDouble() > 0.7 ? "Dobrý trénink, energie ok" : null
                    });
                }
            }
            return clients;
        }
    }
}
