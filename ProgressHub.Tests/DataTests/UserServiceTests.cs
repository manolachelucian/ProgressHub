using Microsoft.EntityFrameworkCore;
using ProgressHub.Core.Interfaces;
using ProgressHub.Core.Models;
using ProgressHub.Data;
using ProgressHub.Data.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgressHub.Tests.DataTests
{
    public class UserServiceTests 
    {
        private ProgressHubDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ProgressHubDbContext>()
             .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unikátní název pro izolaci testu
             .Options;

            return new ProgressHubDbContext(options);
        }


        [Fact]
        public async Task AddClientAsync_ValidClient()
        {
            using var context = CreateInMemoryDbContext();
            IUserService user =  new UserService(context);
            var client = new User
            {
                FirstName = "Petr",
                LastName = "Novák",
                Email = "petr.novak@example.com",
                TargetCalories = 2400
            };

            //ACT
            await user.AddClientAsync(client);

            //Assert
            var result = await user.GetAllClientsAsync();
            var savedUser = result.FirstOrDefault(u => u.Id == client.Id);
            
            Assert.NotNull(savedUser);

        }
        
    }
}
