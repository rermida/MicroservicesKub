using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using ContactManagement.Persistencia.Worker.Consumers;
using ContactManagement.Infrastructure;
using ContactManagement.Infrastructure.Data;
using ContactManagement.Domain.Repositories;
using ContactManagement.Infrastructure.Repositories;
using ContactManagement.Persistencia.Worker;
using Prometheus;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;

var metricsHost = new HostBuilder()
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.UseKestrel(options =>
        {
            options.ListenAnyIP(8081); // Porta exclusiva para métricas
        });
        webBuilder.Configure(app =>
        {
            app.UseRouting();
            app.UseHttpMetrics(); // coleta padrão
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapMetrics(); // expõe /metrics
            });
        });
    })
    .Build();

var workerHost = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        // 1) EF Core
        services.AddDbContext<ContactDbContext>(opts =>
            opts.UseSqlServer(ctx.Configuration.GetConnectionString("SqlServer")));

        // 2) Repositório
        services.AddScoped<IContactRepository, ContactRepository>();

        // 3) MassTransit + Consumers
        services.AddMassTransit(x =>
        {
            x.AddConsumer<ContactCreatedConsumer>();
            x.AddConsumer<ContactReadConsumer>();
            x.AddConsumer<ContactUpdatedConsumer>();
            x.AddConsumer<ContactDeletedConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(ctx.Configuration.GetConnectionString("RabbitMQ") ?? "rabbitmq://localhost", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                cfg.ReceiveEndpoint("contact-created-queue", e =>
                    e.ConfigureConsumer<ContactCreatedConsumer>(context));
                cfg.ReceiveEndpoint("contact-read-queue", e =>
                    e.ConfigureConsumer<ContactReadConsumer>(context));
                cfg.ReceiveEndpoint("contact-updated-queue", e =>
                    e.ConfigureConsumer<ContactUpdatedConsumer>(context));
                cfg.ReceiveEndpoint("contact-deleted-queue", e =>
                    e.ConfigureConsumer<ContactDeletedConsumer>(context));
            });
        });

        // 4) Worker background
        services.AddHostedService<Worker>();
    })
    .Build();

    // Rodar Worker + Métricas em paralelo
    await Task.WhenAll(
        workerHost.RunAsync(),
        metricsHost.RunAsync()
    );
