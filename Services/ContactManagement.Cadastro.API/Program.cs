using MassTransit;
using ContactManagement.Messages.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// 🔧 Força escuta na porta 80 dentro do container
builder.WebHost.UseUrls("http://*:80");

// Configura MassTransit + RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("RabbitMQ") 
                 ?? "rabbitmq://localhost", h =>
        {
            // usuáriio/senha padrão
            h.Username("guest");
            h.Password("guest");
        });
        // opcional: configurar exchange name, retry, etc.
    });
});

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
namespace ContactManagement.Cadastro.API
{
    public partial class Program { }
}