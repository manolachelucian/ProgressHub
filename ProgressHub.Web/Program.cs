using Microsoft.EntityFrameworkCore;
using ProgressHub.Core.Interfaces;
using ProgressHub.Core.Models;
using ProgressHub.Core.Models.Enums;
using ProgressHub.Core.Services;
using ProgressHub.Core.Services.MacroCalculator;
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
            builder.Services.AddSingleton<IMacroCalculatorService, MacroCalculator>();
            builder.Services.AddSingleton<IClientAnalyticsService, ClientAnalyticsService>();

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

                // Pokud je DB prázdná, vygenerujeme data přes tvůj DatabaseSeeder
                if (app.Environment.IsDevelopment() && !await dbContext.Users.AnyAsync())
                {
                    var seededClients = DatabaseSeeder.GenerateClients(30,90);
                    await dbContext.Users.AddRangeAsync(seededClients);
                    await dbContext.SaveChangesAsync();
                }

            }
            await app.RunAsync();
        }
    }
}
