using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace ContactManagement.Persistencia.Worker
{
    public class Worker : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // O MassTransit vai disparar os consumers; não precisamos do loop manual.
            return Task.CompletedTask;
        }
    }
}
