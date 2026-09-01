using ProgressHub.Core.Models;
using ProgressHub.Core.Models.DTOs.ClientDTOs;
using ProgressHub.Core.Models.DTOs.DailyLogDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgressHub.Core.Interfaces
{
    public interface IUserService
    {
        //client
        Task<List<ClientListItemDto>> GetAllClientsAsync();
        Task<User?> GetClientByIdAsync(int id);

        //CRUD
        Task AddClientAsync(CreateClientDto dto);
        Task UpdateClientAsync(UpdateClientDto dto);

        Task RemoveClientAsync(int clientId);


        //Daily logs

        Task AddDailyLogAsync(CreateDailyLogDto dto);

        Task UpdateDailyLogAsync(UpdateDailyLogDto dto);
        Task RemoveDailyLogAsync(int dailyLogId);
    }
}
