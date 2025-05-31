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

// Alias para evitar conflito com WebApplicationFactory
using ConsultaProgram = ContactManagement.Consulta.API.Program;

namespace ContactManagement.IntegrationTests.Factories
{
    public class ConsultaApiFactory : WebApplicationFactory<ConsultaProgram>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Remove DbContext registrado anteriormente
                services.RemoveAll(typeof(DbContextOptions<ContactDbContext>));
                services.RemoveAll(typeof(ContactDbContext));

                services.AddDbContext<ContactDbContext>(options =>
                    options.UseInMemoryDatabase("Consulta_E2E_Db"));

                // Substitui MassTransit por TestHarness
                services.RemoveAll(typeof(IBusControl));
                services.RemoveAll(typeof(IPublishEndpoint));
                services.AddMassTransitTestHarness(cfg =>
                {
                    cfg.AddConsumer<ContactCreatedConsumer>(); // precisa consumir para inserir
                    cfg.AddConsumer<ContactReadConsumer>();
                    cfg.UsingInMemory((ctx, cfgInMemory) =>
                    {
                        cfgInMemory.ConfigureEndpoints(ctx);
                    });
                });

                // Inicializa banco
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ContactDbContext>();
                db.Database.EnsureCreated();
            });
        }
    }
}
