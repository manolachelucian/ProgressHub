using Microsoft.EntityFrameworkCore;
using ProgressHub.Core.Enums;
using ProgressHub.Core.Interfaces;
using ProgressHub.Core.Models;
using ProgressHub.Data;
using ProgressHub.Data.Services;
using ProgressHub.Web.Components;

namespace ProgressHub.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddDbContextFactory<ProgressHubDbContext>(options =>
                options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<IUserService, UserService>();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            using (var scope = app.Services.CreateScope())
            {
                var contextFactory = scope.ServiceProvider
                    .GetRequiredService<IDbContextFactory<ProgressHubDbContext>>();

                await using var dbContext = await contextFactory.CreateDbContextAsync();
                await dbContext.Database.MigrateAsync();

                // Pokud v DB ještě nejsou žádní uživatelé, vložíme ukázková data
                if (!await dbContext.Users.AnyAsync())
                {
                    var client1 = new User
                    {
                        FirstName = "Jan",
                        LastName = "Novák",
                        Email = "jan.novak@fitness.cz",
                        UserRole = UserRole.Client,
                        HeightInCm = 182,
                        DateOfBirth = new DateOnly(1995, 5, 14),
                        TargetCalories = 2400,
                        TargetProteinGrams = 180,
                        TargetCarbsGrams = 240,
                        TargetFatsGrams = 70,
                        DailyLogs = new List<DailyLog>
            {
                new()
                {
                    Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)),
                    Weight = 84.5,
                    ConsumedCalories = 2350,
                    ConsumedProteins = 175,
                    ConsumedCarbs = 230,
                    ConsumedFats = 68,
                    Note = "Dobrý trénink nohou"
                },
                new()
                {
                    Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                    Weight = 84.1,
                    ConsumedCalories = 2410,
                    ConsumedProteins = 182,
                    ConsumedCarbs = 245,
                    ConsumedFats = 72,
                    Note = "Rest day"
                }
            }
                    };

                    var client2 = new User
                    {
                        FirstName = "Martina",
                        LastName = "Králová",
                        Email = "martina@seznam.cz",
                        UserRole = UserRole.Client,
                        HeightInCm = 168,
                        DateOfBirth = new DateOnly(1998, 11, 3),
                        TargetCalories = 1800,
                        TargetProteinGrams = 130,
                        TargetCarbsGrams = 180,
                        TargetFatsGrams = 55
                    };

                    dbContext.Users.AddRange(client1, client2);
                    await dbContext.SaveChangesAsync();
                }

            }

            await app.RunAsync();
        }
    }
}
