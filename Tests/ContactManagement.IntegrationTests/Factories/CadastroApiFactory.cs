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

using CadastroProgram = ContactManagement.Cadastro.API.Program;

namespace ContactManagement.IntegrationTests.Factories
{
    public class CadastroApiFactory : WebApplicationFactory<CadastroProgram>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Substitui o banco real por InMemory
                services.RemoveAll(typeof(DbContextOptions<ContactDbContext>));
                services.RemoveAll(typeof(ContactDbContext));
                services.AddDbContext<ContactDbContext>(opts =>
                    opts.UseInMemoryDatabase("Cadastro_E2E_Db"));

                // Remove MassTransit real e injeta test harness
                services.RemoveAll(typeof(IBusControl));
                services.RemoveAll(typeof(IPublishEndpoint));
                services.AddMassTransitTestHarness(cfg =>
                {
                    cfg.AddConsumer<ContactCreatedConsumer>();
                    cfg.UsingInMemory((ctx, cfgInMem) =>
                    {
                        cfgInMem.ConfigureEndpoints(ctx);
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
