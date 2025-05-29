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

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        // 1) EF Core – DbContext
        services.AddDbContext<ContactDbContext>(opts =>
            opts.UseSqlServer(
                ctx.Configuration.GetConnectionString("SqlServer")
            )
        );

        // 2) Repositório
        services.AddScoped<IContactRepository, ContactRepository>();

        // 3) MassTransit + Consumer
        services.AddMassTransit(x =>
        {
            x.AddConsumer<ContactCreatedConsumer>();
            x.AddConsumer<ContactReadConsumer>();
            x.AddConsumer<ContactUpdatedConsumer>();
            x.AddConsumer<ContactDeletedConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(ctx.Configuration.GetConnectionString("RabbitMQ")
                          ?? "rabbitmq://localhost", h =>
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
        // hosted service é registrado automaticamente

        // 4) Registrar o Worker
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
