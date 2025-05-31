using System.Linq;
using ContactManagement.Infrastructure.Data;
using ContactManagement.Persistencia.Worker.Consumers;
using MassTransit;
using MassTransit.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using ExclusaoProgram = ContactManagement.Exclusao.API.Program;

namespace ContactManagement.IntegrationTests.Factories
{
    public class ExclusaoApiFactory : WebApplicationFactory<ExclusaoProgram>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Substitui DbContext com banco InMemory
                services.RemoveAll(typeof(DbContextOptions<ContactDbContext>));
                services.RemoveAll(typeof(ContactDbContext));
                services.AddDbContext<ContactDbContext>(options =>
                    options.UseInMemoryDatabase("Exclusao_E2E_Db"));

                // Substitui MassTransit com test harness
                services.RemoveAll(typeof(IBusControl));
                services.RemoveAll(typeof(IPublishEndpoint));
                services.AddMassTransitTestHarness(cfg =>
                {
                    cfg.AddConsumer<ContactDeletedConsumer>();
                    cfg.UsingInMemory((ctx, cfgInMemory) =>
                    {
                        cfgInMemory.ConfigureEndpoints(ctx);
                    });
                });

                // Inicializa banco InMemory
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ContactDbContext>();
                db.Database.EnsureCreated();
            });
        }
    }
}
