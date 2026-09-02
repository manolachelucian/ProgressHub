using Mapster;
using Microsoft.EntityFrameworkCore;
using ProgressHub.Core.Exceptions;
using ProgressHub.Core.Interfaces;
using ProgressHub.Core.Models;
using ProgressHub.Core.Models.DTOs.ClientDTOs;
using ProgressHub.Core.Models.DTOs.DailyLogDTOs;
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
        /// <param name="clientId"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public async Task RemoveClientAsync(int clientId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            // Načteme pouze samotného klienta bez zbytečného .Include(u => u.DailyLogs)
            var client = await context.Users
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
        public async Task UpdateClientAsync(UpdateClientDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var normalizedEmail = dto.Email?.Trim().ToLowerInvariant() ?? string.Empty;

            if (!EmailValidator.IsValid(normalizedEmail))
            {
                throw new InvalidEmailFormatException(dto.Email ?? string.Empty);
            }

            await using var context = await _contextFactory.CreateDbContextAsync();

            var existingUser = await context.Users
                .FirstOrDefaultAsync(u => u.Id == dto.Id && u.UserRole == UserRole.Client);

            if (existingUser is null)
            {
                throw new KeyNotFoundException($"Client with ID {dto.Id} was not found.");
            }

            bool emailExistsOtherUser = await context.Users
                .AnyAsync(u => u.Email.ToLower() == normalizedEmail && u.Id != dto.Id);

            if (emailExistsOtherUser)
            {
                throw new DuplicateEmailException(dto.Email);
            }

            // Ošetření konzistence: Trim jmen a uložení normalizovaného e-mailu
            existingUser.FirstName = dto.FirstName.Trim();
            existingUser.LastName = dto.LastName.Trim();
            existingUser.Email = normalizedEmail; // <-- opraveno z dto.Email
            existingUser.DateOfBirth = dto.DateOfBirth;
            existingUser.Gender = dto.Gender;
            existingUser.FitnessGoal = dto.FitnessGoal;
            existingUser.HeightInCm = dto.HeightInCm;
            existingUser.TargetCalories = dto.TargetCalories;
            existingUser.TargetProteinGrams = dto.TargetProteinGrams;
            existingUser.TargetCarbsGrams = dto.TargetCarbsGrams;
            existingUser.TargetFatsGrams = dto.TargetFatsGrams;

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<ClientListItemDto>> GetAllClientsAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.Users
            .Where(u => u.UserRole == UserRole.Client)
            .OrderBy(u => u.LastName)
            .Select(u => new ClientListItemDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                TargetCalories = u.TargetCalories,
                LatestWeight = u.DailyLogs
                    .OrderByDescending(l => l.Date)
                    .Select(l => (double?)l.Weight)
                    .FirstOrDefault()
            })
            .ToListAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newClient"></param>
        /// <returns></returns>
        public async Task AddClientAsync(CreateClientDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var normalizedEmail = dto.Email?.Trim().ToLowerInvariant() ?? string.Empty;
            if (!EmailValidator.IsValid(normalizedEmail))
            {
                throw new InvalidEmailFormatException(dto.Email ?? string.Empty);
            }

            await using var context = await _contextFactory.CreateDbContextAsync();

            bool emailExist = await context.Users
                .AnyAsync(u => u.Email.ToLower() == normalizedEmail);

            if(emailExist)
            {
                throw new DuplicateEmailException(dto.Email ?? string.Empty);
            }

            var clientEntity = new User
            {
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Email = normalizedEmail,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                FitnessGoal = dto.FitnessGoal,
                HeightInCm = dto.HeightInCm,
                TargetCalories = dto.TargetCalories,
                TargetProteinGrams = dto.TargetProteinGrams,
                TargetCarbsGrams = dto.TargetCarbsGrams,
                TargetFatsGrams = dto.TargetFatsGrams,
                UserRole = UserRole.Client
            };
            context.Users.Add(clientEntity);
            await context.SaveChangesAsync();

        }

        ///---------------------------------- Daily Log Management --------------------------------

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newDaylog"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public async Task AddDailyLogAsync(CreateDailyLogDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            await using var context = await _contextFactory.CreateDbContextAsync();

            var userExists = await context.Users.AnyAsync(u => u.Id == dto.UserId);
            if (!userExists)
            {
                throw new KeyNotFoundException($"Cannot add daily log: no user with Id {dto.UserId} exists.");
            }

            var existing = await context.DailyLog.FirstOrDefaultAsync(
                l => l.UserId == dto.UserId && l.Date == dto.Date);

            if (existing is null)
            {
                var newLog = new DailyLog
                {
                    UserId = dto.UserId,
                    Date = dto.Date,
                    Weight = dto.Weight,
                    ConsumedCalories = dto.ConsumedCalories,
                    ConsumedProteins = dto.ConsumedProteins,
                    ConsumedCarbs = dto.ConsumedCarbs,
                    ConsumedFats = dto.ConsumedFats,
                    TrainingType = dto.TrainingType,
                    Note = dto.Note
                };
                context.DailyLog.Add(newLog);
            }
            else
            {
                existing.Weight = dto.Weight;
                existing.ConsumedCalories = dto.ConsumedCalories;
                existing.ConsumedProteins = dto.ConsumedProteins;
                existing.ConsumedCarbs = dto.ConsumedCarbs;
                existing.ConsumedFats = dto.ConsumedFats;
                existing.TrainingType = dto.TrainingType;
                existing.Note = dto.Note;
            }

            await context.SaveChangesAsync();

        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="updatedLog"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public async Task UpdateDailyLogAsync(UpdateDailyLogDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            await using var context = await _contextFactory.CreateDbContextAsync();

            var existingLog = await context.DailyLog.FirstOrDefaultAsync(l => l.Id == dto.Id);
            if (existingLog is null)
            {
                throw new KeyNotFoundException($"DailyLog with ID {dto.Id} was not found.");
            }

            existingLog.Date = dto.Date;
            existingLog.Weight = dto.Weight;
            existingLog.ConsumedCalories = dto.ConsumedCalories;
            existingLog.ConsumedProteins = dto.ConsumedProteins;
            existingLog.ConsumedCarbs = dto.ConsumedCarbs;
            existingLog.ConsumedFats = dto.ConsumedFats;
            existingLog.TrainingType = dto.TrainingType;
            existingLog.Note = dto.Note;

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
