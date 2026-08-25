using ProgressHub.Core.Models;
using ProgressHub.Core.Enums;

using Microsoft.EntityFrameworkCore;
using ProgressHub.Core.Interfaces;

namespace ProgressHub.Data.Services
{
    public class UserService : IUserService
    {
        private readonly ProgressHubDbContext _context;

        public UserService(ProgressHubDbContext context)
        {
            _context = context;
        }

        public async Task<List<User>> GetAllClientsAsync()
        {
            return await _context.Users
                .Where(u => u.UserRole == UserRole.Client)
                .Include(u => u.DailyLogs)
                .OrderBy(u => u.LastName)
                .ToListAsync();
        }

        public async Task AddClientAsync(User newClient)
        {
            newClient.UserRole = UserRole.Client;
            _context.Users.Add(newClient);  

            await _context.SaveChangesAsync();
        }


        public async Task AddDaily(DailyLog newDaylog)
        {
            _context.DailyLog.Add(newDaylog);
            await _context.SaveChangesAsync();
        }
    }
}
