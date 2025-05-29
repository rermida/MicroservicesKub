using MassTransit;
using Microsoft.EntityFrameworkCore;
using ContactManagement.Domain.Repositories;
using ContactManagement.Infrastructure.Data;
using ContactManagement.Infrastructure.Repositories;
using ContactManagement.Persistencia.Worker;

var builder = WebApplication.CreateBuilder(args);

// ⚠️ Apenas adiciona SQL Server se não for ambiente de testes
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<ContactDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));
}

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ContactManagement.Persistencia.Worker.Consumers.ContactReadConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("RabbitMQ") ?? "rabbitmq://localhost", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddScoped<IContactRepository, ContactRepository>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

namespace ContactManagement.Consulta.API
{
    public partial class Program { }
}