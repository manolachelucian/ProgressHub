using Microsoft.EntityFrameworkCore;
using ProgressHub.Core.Exceptions;
using ProgressHub.Core.Interfaces;
using ProgressHub.Core.Models;
using ProgressHub.Core.Models.Enums;
using ProgressHub.Core.Validation;

namespace ProgressHub.Data.Services
{
    public class UserService : IUserService
    {
        private readonly IDbContextFactory<ProgressHubDbContext> _contextFactory;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contextFactory"></param>
        public UserService(IDbContextFactory<ProgressHubDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="updatedClient"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public async Task UpdateClientAsync(User updatedClient)
        {
            ArgumentNullException.ThrowIfNull(updatedClient);
            var normalizedEmail = updatedClient.Email?.Trim().ToLowerInvariant() ?? string.Empty;

            // 1. Regex validace formátu
            if (!EmailValidator.IsValid(normalizedEmail))
            {
                throw new InvalidEmailFormatException(updatedClient.Email ?? string.Empty);
            }

            await using var context = await _contextFactory.CreateDbContextAsync();
            var existingUser = await context.Users
                .FirstOrDefaultAsync(u => u.Id == updatedClient.Id && u.UserRole == UserRole.Client);

            if(existingUser is null)
            {

                throw new KeyNotFoundException($"Client with ID {updatedClient.Id} was not found.");
            }

            // 2. Kontrola unikátnosti e-mailu (pokud ho změnil a nový už patří někomu jinému)
            bool emailExistsOtherUser = await context.Users
                .AnyAsync(u => u.Email.ToLower() == normalizedEmail && u.Id != updatedClient.Id);

            if (emailExistsOtherUser)
            {
                throw new DuplicateEmailException(updatedClient.Email);
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<User>> GetAllClientsAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.Users
                .Where(u => u.UserRole == UserRole.Client)
                .Include(u => u.DailyLogs)
                .OrderBy(u => u.LastName)
                .ToListAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newClient"></param>
        /// <returns></returns>
        public async Task AddClientAsync(User newClient)
        {
            ArgumentNullException.ThrowIfNull(newClient);

            var normalizedEmail = newClient.Email?.Trim().ToLowerInvariant() ?? string.Empty;
            if (!EmailValidator.IsValid(normalizedEmail))
            {
                throw new InvalidEmailFormatException(newClient.Email ?? string.Empty);
            }

            await using var context = await _contextFactory.CreateDbContextAsync();

            bool emailExist = await context.Users
                .AnyAsync(u => u.Email.ToLower() == normalizedEmail);

            if(emailExist)
            {
                throw new DuplicateEmailException(newClient.Email ?? string.Empty);
            }

            newClient.Email = normalizedEmail;
            newClient.UserRole = UserRole.Client;

            context.Users.Add(newClient);
            await context.SaveChangesAsync();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newDaylog"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<User?> GetClientByIdAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Users
                .Include(u => u.DailyLogs)
                .FirstOrDefaultAsync(u => u.Id == id && u.UserRole == UserRole.Client);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="updatedLog"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dailyLogId"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
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
