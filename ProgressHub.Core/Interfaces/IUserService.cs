using ProgressHub.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgressHub.Core.Interfaces
{
    public interface IUserService
    {
        Task<List<User>> GetAllClientsAsync();
        Task AddClientAsync(User newClient);
        Task<User?> GetClientByIdAsync(int id);
        Task AddDailyLogAsync(DailyLog newDaylog);

        Task RemoveClientAsync(int clientId);

        Task UpdateClientAsync(User updatedClient);

        Task UpdateDailyLogAsync(DailyLog updatedLog);
        Task RemoveDailyLogAsync(int dailyLogId);
    }
}
