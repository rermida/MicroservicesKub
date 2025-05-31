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

using AtualizacaoProgram = ContactManagement.Atualizacao.API.Program;

namespace ContactManagement.IntegrationTests.Factories
{
    public class AtualizacaoApiFactory : WebApplicationFactory<AtualizacaoProgram>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                // Substitui o contexto para banco InMemory
                services.RemoveAll(typeof(DbContextOptions<ContactDbContext>));
                services.RemoveAll(typeof(ContactDbContext));
                services.AddDbContext<ContactDbContext>(opts =>
                    opts.UseInMemoryDatabase("Atualizacao_E2E_Db"));

                // Substitui o transporte real por test harness do MassTransit
                services.RemoveAll(typeof(IBusControl));
                services.RemoveAll(typeof(IPublishEndpoint));
                services.AddMassTransitTestHarness(cfg =>
                {
                    cfg.AddConsumer<ContactUpdatedConsumer>();
                    cfg.UsingInMemory((ctx, cfgInMem) =>
                    {
                        cfgInMem.ConfigureEndpoints(ctx);
                    });
                });

                // Garante que o banco seja criado
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ContactDbContext>();
                db.Database.EnsureCreated();
            });
        }
    }
}
