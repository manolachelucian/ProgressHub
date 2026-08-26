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


        //remove client method
        public async Task RemoveClientAsync(int clientId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var client = await context.Users
                .Include(u => u.DailyLogs)
                .FirstOrDefaultAsync(u => u.Id == clientId && u.UserRole == UserRole.Client);

            if (client is null)
            {
                throw new KeyNotFoundException($"Client with ID {clientId} was not found.");
            }

            context.Users.Remove(client);
            await context.SaveChangesAsync();
        }


        public async Task UpdateClientAsync(User updatedClient)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Id == updatedClient.Id && u.UserRole == UserRole.Client);

            if(existingUser is null)
            {

                throw new KeyNotFoundException($"Client with ID {updatedClient.Id} was not found.");
            }

            // Aktualizace profilových údajů
            existingUser.FirstName = updatedClient.FirstName;
            existingUser.LastName = updatedClient.LastName;
            existingUser.Email = updatedClient.Email;
            existingUser.DateOfBirth = updatedClient.DateOfBirth;
            existingUser.Gender = updatedClient.Gender;
            existingUser.FitnessGoal = updatedClient.FitnessGoal;
            existingUser.HeightInCm = updatedClient.HeightInCm;

            // Aktualizace makro cílů
            existingUser.TargetCalories = updatedClient.TargetCalories;
            existingUser.TargetProteinGrams = updatedClient.TargetProteinGrams;
            existingUser.TargetCarbsGrams = updatedClient.TargetCarbsGrams;
            existingUser.TargetFatsGrams = updatedClient.TargetFatsGrams;

            await context.SaveChangesAsync();
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


        public async Task UpdateDailyLogAsync(DailyLog updatedLog)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var existingLog = await context.DailyLog
                .FirstOrDefaultAsync(l => l.Id == updatedLog.Id);

            if (existingLog is null)
            {
                throw new KeyNotFoundException($"DailyLog with ID {updatedLog.Id} was not found.");
            }

            // Aktualizace hodnot v záznamu
            existingLog.Date = updatedLog.Date;
            existingLog.Weight = updatedLog.Weight;
            existingLog.ConsumedCalories = updatedLog.ConsumedCalories;
            existingLog.ConsumedProteins = updatedLog.ConsumedProteins;
            existingLog.ConsumedCarbs = updatedLog.ConsumedCarbs;
            existingLog.ConsumedFats = updatedLog.ConsumedFats;
            existingLog.Note = updatedLog.Note;

            await context.SaveChangesAsync();
        }

        public async Task RemoveDailyLogAsync(int dailyLogId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var log = await context.DailyLog
                .FirstOrDefaultAsync(l => l.Id == dailyLogId);

            if (log is null)
            {
                throw new KeyNotFoundException($"DailyLog with ID {dailyLogId} was not found.");
            }

            context.DailyLog.Remove(log);
            await context.SaveChangesAsync();
        }
    }
}
