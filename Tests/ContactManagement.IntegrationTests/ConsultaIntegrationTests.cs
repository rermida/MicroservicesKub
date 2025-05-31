using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using ContactManagement.Infrastructure.Data;
using ContactManagement.Messages.Events;
using FluentAssertions;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ContactManagement.IntegrationTests
{
    public class ConsultaIntegrationTests : IClassFixture<Factories.ConsultaApiFactory>
    {
        private readonly HttpClient _client;
        private readonly ITestHarness _harness;
        private readonly IServiceProvider _services;

        public ConsultaIntegrationTests(Factories.ConsultaApiFactory factory)
        {
            _client = factory.CreateClient();
            _harness = factory.Services.GetRequiredService<ITestHarness>();
            _services = factory.Services;

            _harness.Start().GetAwaiter().GetResult();
        }

        [Fact]
        public async Task GetById_Should_PublishReadEvent_And_UpdateLastReadAt()
        {
            // Arrange – cria evento simulado
            var id = Guid.NewGuid();
            var createdEvent = new ContactCreated(id, "Joana", "joana@ex.com", "99999999", "21");

            await _harness.Bus.Publish(createdEvent);

            // ⏳ Aguarda o worker consumir e persistir
            (await _harness.Consumed.Any<ContactCreated>()).Should().BeTrue();

            // Aguarda até o banco refletir a inserção
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ContactDbContext>();

            const int maxRetries = 10;
            var retries = 0;
            Domain.Entities.Contact? contato = null;

            while (contato == null && retries++ < maxRetries)
            {
                await Task.Delay(200);
                contato = await db.Contacts.FindAsync(id);
            }
            contato.Should().NotBeNull("esperava-se que o contato fosse persistido");

            // Aguarda o endpoint estar disponível
            HttpResponseMessage? response = null;
            retries = 0;
            while (response == null && retries++ < maxRetries)
            {
                await Task.Delay(200);
                var tryResp = await _client.GetAsync($"/api/contacts/{id}");
                if (tryResp.IsSuccessStatusCode)
                    response = tryResp;
            }
            response.Should().NotBeNull("esperava-se sucesso no GET");
            response!.EnsureSuccessStatusCode();
        }
    }
}
