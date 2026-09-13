
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<UserService> _logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contextFactory"></param>
        public UserService(IDbContextFactory<ProgressHubDbContext> contextFactory, ILogger<UserService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<User?> GetClientByIdAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                return await context.Users
                    .Include(u => u.DailyLogs)
                    .FirstOrDefaultAsync(u => u.Id == id && u.UserRole == UserRole.Client);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load client {clientId}", id);
                throw;
            }
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
                .Include(u => u.DailyLogs )
                .FirstOrDefaultAsync(u => u.Id == clientId && u.UserRole == UserRole.Client);

            if (client is null)
            {
                _logger.LogWarning("Attempted to remove client {ClientId}, but no matching client was found.", clientId);
                throw new KeyNotFoundException($"Client with ID {clientId} was not found.");
            }

            try
            {
                context.Users.Remove(client);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove client {ClientId}", clientId);
                throw;
            }
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
                _logger.LogWarning("Attempted to update client {ClientId}, but no matching client was found.", dto.Id);
                throw new KeyNotFoundException($"Client with ID {dto.Id} was not found.");
            }

            bool emailExistsOtherUser = await context.Users
                .AnyAsync(u => u.Email.ToLower() == normalizedEmail && u.Id != dto.Id);

            if (emailExistsOtherUser)
            {
                _logger.LogInformation("Rejected client update for Id {ClientId}: email {Email} already in use.", dto.Id, normalizedEmail);
                throw new DuplicateEmailException(dto.Email);
            }

            if (!string.IsNullOrWhiteSpace(dto.FullPhoneNumber))
            {
                var phoneExists = await context.Users
                    .AnyAsync(u => u.PhoneNumber == dto.FullPhoneNumber && u.Id != dto.Id);

                if (phoneExists)
                {
                    _logger.LogInformation($"Phone: {dto.FullPhoneNumber} is rejected, phone number is already in use ");
                    throw new InvalidOperationException($"Phone number '{dto.FullPhoneNumber}' is already assigned to another client.");
                }
            }

            // Ošetření konzistence: Trim jmen a uložení normalizovaného e-mailu
            existingUser.FirstName = dto.FirstName.Trim();
            existingUser.LastName = dto.LastName.Trim();
            existingUser.Email = normalizedEmail; 
            existingUser.DateOfBirth = dto.DateOfBirth;
            existingUser.PhoneNumber = dto.FullPhoneNumber;
            existingUser.Gender = dto.Gender;
            existingUser.FitnessGoal = dto.FitnessGoal;
            existingUser.HeightInCm = dto.HeightInCm;
            existingUser.TargetCalories = dto.TargetCalories;
            existingUser.TargetProteinGrams = dto.TargetProteinGrams;
            existingUser.TargetCarbsGrams = dto.TargetCarbsGrams;
            existingUser.TargetFatsGrams = dto.TargetFatsGrams;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save updated profile for client {ClientId}", dto.Id);
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<ClientListItemDto>> GetAllClientsAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                
                return await context.Users
                   .Where(u => u.UserRole == UserRole.Client)
                   .OrderBy(u => u.LastName)
                   .Select(u => new ClientListItemDto
                   {
                       Id = u.Id,
                       FirstName = u.FirstName,
                       LastName = u.LastName,
                       Email = u.Email,
                       PhoneNumber = u.PhoneNumber,
                       CreatedAt = u.CreatedAt
                   })
                   .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load client list.");
                throw;
            }
           
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
                _logger.LogInformation("Rejected new client: invalid email format ({Email}).", dto.Email);
                throw new InvalidEmailFormatException(dto.Email ?? string.Empty);
            }

            await using var context = await _contextFactory.CreateDbContextAsync();

            bool emailExist = await context.Users
                .AnyAsync(u => u.Email.ToLower() == normalizedEmail);

            if(emailExist)
            {
                _logger.LogInformation("Rejected new client: email {Email} already in use.", normalizedEmail);
                throw new DuplicateEmailException(dto.Email ?? string.Empty);
            }

            if (!string.IsNullOrWhiteSpace(dto.FullPhoneNumber))
            {
                var phoneExists = await context.Users
                    .AnyAsync(u => u.PhoneNumber == dto.FullPhoneNumber);

                if (phoneExists)
                {
                    _logger.LogInformation("Rejected new client: Phone number {Phone number} already in use.", dto.FullPhoneNumber);
                    throw new InvalidOperationException($"Phone number '{dto.FullPhoneNumber}' is already assigned to another client.");
                }
            }

            var clientEntity = new User
            {
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Email = normalizedEmail,
                DateOfBirth = dto.DateOfBirth,
                CreatedAt = dto.CreatedAt,
                PhoneNumber = dto.FullPhoneNumber,
                Gender = dto.Gender,
                FitnessGoal = dto.FitnessGoal,
                HeightInCm = dto.HeightInCm,
                TargetCalories = dto.TargetCalories,
                TargetProteinGrams = dto.TargetProteinGrams,
                TargetCarbsGrams = dto.TargetCarbsGrams,
                TargetFatsGrams = dto.TargetFatsGrams,
                UserRole = UserRole.Client
            };


            try
            {
                context.Users.Add(clientEntity);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save new client with email {Email}", normalizedEmail);
                throw;
            }

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
                _logger.LogWarning("Attempted to add a daily log for non-existent user {UserId}.", dto.UserId);
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

            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save daily log for user {UserId} on {Date}", dto.UserId, dto.Date);
                throw;
            }

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
                _logger.LogWarning("Attempted to update daily log {LogId}, but it was not found.", dto.Id);
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

            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save updated daily log {LogId}", dto.Id);
                throw;
            }
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

            var log = await context.DailyLog.FirstOrDefaultAsync(l => l.Id == dailyLogId);

            if (log is null)
            {
                _logger.LogWarning("Attempted to remove daily log {LogId}, but it was not found.", dailyLogId);
                throw new KeyNotFoundException($"DailyLog with ID {dailyLogId} was not found.");
            }

            try
            {
                context.DailyLog.Remove(log);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove daily log {LogId}", dailyLogId);
                throw;
            }
        }


   
    }
}
