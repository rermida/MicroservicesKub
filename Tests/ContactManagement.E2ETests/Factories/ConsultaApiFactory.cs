using System;
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

// alias para evitar conflito
using ConsultaProgram = ContactManagement.Consulta.API.Program;

namespace ContactManagement.E2ETests.Factories
{
    public class ConsultaApiFactory : WebApplicationFactory<ConsultaProgram>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                // 🧹 Remove todos os serviços relacionados a DbContextOptions<ContactDbContext>
                var dbContextDescriptors = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions<ContactDbContext>))
                    .ToList();
                foreach (var descriptor in dbContextDescriptors)
                    services.Remove(descriptor);

                // ✅ Substitui pelo InMemory
                services.AddDbContext<ContactDbContext>(options =>
                {
                    options.UseInMemoryDatabase("E2E_Db_Consulta");
                });

                // MassTransit test harness
                services.RemoveAll(typeof(IBusControl));
                services.RemoveAll(typeof(IPublishEndpoint));
                services.AddMassTransitTestHarness(cfg =>
                {
                    cfg.AddConsumer<ContactCreatedConsumer>(); // necessário para gravar o contato no banco
                    cfg.AddConsumer<ContactReadConsumer>();
                    cfg.UsingInMemory((ctx, cfgInMemory) =>
                    {
                        cfgInMemory.ConfigureEndpoints(ctx);
                    });
                });

                // Cria o banco
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ContactDbContext>();
                db.Database.EnsureCreated();
            });
        }
    }
}
