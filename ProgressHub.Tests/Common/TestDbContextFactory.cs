using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProgressHub.Data;

namespace ProgressHub.Tests.Common
{
    public static class TestDbContextFactory
    {

        /// <summary>
        /// Vytvoří a nakonfiguruje instanci <see cref="IDbContextFactory{ProgressHubDbContext}"/>
        /// s unikátním názvem in-memory databáze pro izolaci jednotlivých testů.
        /// </summary>
        /// <returns>Nakonfigurovaná továrna databázového kontextu.</returns>
        /// 
        public static IDbContextFactory<ProgressHubDbContext> Create()
        {
            var services = new ServiceCollection();
            services.AddDbContextFactory<ProgressHubDbContext>(options =>
                options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

            return services.BuildServiceProvider()
                .GetRequiredService<IDbContextFactory<ProgressHubDbContext>>();
        }

    }
}
