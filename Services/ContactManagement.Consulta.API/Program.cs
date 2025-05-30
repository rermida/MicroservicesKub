using MassTransit;
using Microsoft.EntityFrameworkCore;
using ContactManagement.Domain.Repositories;
using ContactManagement.Infrastructure.Data;
using ContactManagement.Infrastructure.Repositories;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// 🔧 Escutar na porta 80 do container
builder.WebHost.UseUrls("http://*:80");

// ⚠️ Apenas adiciona SQL Server se não for ambiente de testes
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<ContactDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));
}

builder.Services.AddMassTransit(x =>
{
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

// Middleware Prometheus + Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseRouting();

// 🔎 Middleware para métricas HTTP (latência, status, etc)
app.UseHttpMetrics(); // <--- coleta automáticas por endpoint e status

app.UseAuthorization();

// Map Controllers e Endpoint /metrics
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapMetrics(); // <--- Prometheus scrape target
});

app.Run();

namespace ContactManagement.Consulta.API
{
    public partial class Program { }
}