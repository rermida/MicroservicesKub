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

namespace ContactManagement.E2ETests.Factories
{
    public class ExclusaoApiFactory : WebApplicationFactory<ExclusaoProgram>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<ContactDbContext>));
                services.RemoveAll(typeof(ContactDbContext));
                services.AddDbContext<ContactDbContext>(opts => opts.UseInMemoryDatabase("E2E_Db"));

                services.RemoveAll(typeof(IBusControl));
                services.RemoveAll(typeof(IPublishEndpoint));
                services.AddMassTransitTestHarness(cfg =>
                {
                    cfg.AddConsumer<ContactDeletedConsumer>();
                    cfg.UsingInMemory((ctx, cfgInMem) => cfgInMem.ConfigureEndpoints(ctx));
                });

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ContactDbContext>();
                db.Database.EnsureCreated();
            });
        }
    }
}
