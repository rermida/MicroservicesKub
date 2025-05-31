using System.Net.Http.Json;
using ContactManagement.Atualizacao.API.Models;
using ContactManagement.Infrastructure.Data;
using ContactManagement.Messages.Events;
using FluentAssertions;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ContactManagement.IntegrationTests
{
    public class AtualizacaoIntegrationTests : IClassFixture<Factories.AtualizacaoApiFactory>
    {
        private readonly HttpClient _client;
        private readonly ITestHarness _harness;
        private readonly ContactDbContext _db;

        public AtualizacaoIntegrationTests(Factories.AtualizacaoApiFactory factory)
        {
            _client = factory.CreateClient();
            _harness = factory.Services.GetRequiredService<ITestHarness>();
            _harness.Start().GetAwaiter().GetResult();

            var scope = factory.Services.CreateScope();
            _db = scope.ServiceProvider.GetRequiredService<ContactDbContext>();
        }

        [Fact]
        public async Task PutContact_Should_UpdateInDatabase_And_PublishEvent()
        {
            // Arrange: cria contato inicial
            var id = Guid.NewGuid();
            await _db.Contacts.AddAsync(new Domain.Entities.Contact(id, "Carlos", "carlos@ex.com", "12345678", "21"));
            await _db.SaveChangesAsync();

            var req = new UpdateContactRequest("Carlos Silva", "carlos.silva@ex.com", "87654321", "11");

            // Act: dispara PUT (que publica o evento)
            var response = await _client.PutAsJsonAsync($"/api/contacts/{id}", req);
            response.EnsureSuccessStatusCode();

            // Assert 1: evento foi publicado
            (await _harness.Published.Any<ContactUpdated>()).Should().BeTrue("esperava-se que ContactUpdated fosse publicado");

            // Assert 2: espera até que o worker aplique a atualização
            const int maxRetries = 10;
            int retries = 0;
            Domain.Entities.Contact? updated = null;

            while (retries++ < maxRetries)
            {
                await Task.Delay(200);
                updated = await _db.Contacts.FindAsync(id);
                if (updated?.Name == "Carlos Silva" && updated.Email == "carlos.silva@ex.com")
                    break;
            }

            updated.Should().NotBeNull("esperava-se que o contato fosse atualizado");
            updated!.Name.Should().Be("Carlos Silva");
            updated.Email.Should().Be("carlos.silva@ex.com");
        }
    }
}
