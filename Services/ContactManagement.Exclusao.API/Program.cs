using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// 🔧 Escutar na porta 80 do container
builder.WebHost.UseUrls("http://*:80");

// MassTransit + RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("RabbitMQ")
                  ?? "rabbitmq://localhost", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
    });
});

// MVC & Swagger
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
namespace ContactManagement.Exclusao.API
{
    public partial class Program { }
}