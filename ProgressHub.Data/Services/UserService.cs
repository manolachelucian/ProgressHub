using ProgressHub.Core.Models;
using ProgressHub.Core.Enums;

using Microsoft.EntityFrameworkCore;
using ProgressHub.Core.Interfaces;

namespace ProgressHub.Data.Services
{
    public class UserService : IUserService
    {
        private readonly IDbContextFactory<ProgressHubDbContext> _contextFactory;

        public UserService(IDbContextFactory<ProgressHubDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<User>> GetAllClientsAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.Users
                .Where(u => u.UserRole == UserRole.Client)
                .Include(u => u.DailyLogs)
                .OrderBy(u => u.LastName)
                .ToListAsync();
        }

        public async Task AddClientAsync(User newClient)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            newClient.UserRole = UserRole.Client;
            context.Users.Add(newClient);
            await context.SaveChangesAsync();

        }
        public async Task AddDailyLogAsync(DailyLog newDaylog)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var userExists = await context.Users.AnyAsync(u => u.Id == newDaylog.UserId);
            if (!userExists)
            {
                throw new KeyNotFoundException(
                    $"Cannot add daily log: no user with Id {newDaylog.UserId} exists.");
            }

            var existing = await context.DailyLog.FirstOrDefaultAsync(
                l => l.UserId == newDaylog.UserId && l.Date == newDaylog.Date);

            if (existing is null)
            {
                context.DailyLog.Add(newDaylog);
            }
            else
            {
                existing.Weight = newDaylog.Weight;
                existing.ConsumedCalories = newDaylog.ConsumedCalories;
                existing.ConsumedProteins = newDaylog.ConsumedProteins;
                existing.ConsumedCarbs = newDaylog.ConsumedCarbs;
                existing.ConsumedFats = newDaylog.ConsumedFats;
            }

            await context.SaveChangesAsync();

        }

        public async Task<User?> GetClientByIdAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Users
                .Include(u => u.DailyLogs)
                .FirstOrDefaultAsync(u => u.Id == id && u.UserRole == UserRole.Client);
        }
    }
}
