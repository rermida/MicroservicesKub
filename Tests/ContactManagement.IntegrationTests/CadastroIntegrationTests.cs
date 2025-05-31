using System.Net.Http.Json;
using ContactManagement.Cadastro.API.Models;
using ContactManagement.Infrastructure.Data;
using ContactManagement.Messages.Events;
using FluentAssertions;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ContactManagement.IntegrationTests
{
    public class CadastroIntegrationTests : IClassFixture<Factories.CadastroApiFactory>
    {
        private readonly HttpClient _client;
        private readonly ITestHarness _harness;
        private readonly ContactDbContext _db;

        public CadastroIntegrationTests(Factories.CadastroApiFactory factory)
        {
            _client = factory.CreateClient();
            _harness = factory.Services.GetRequiredService<ITestHarness>();
            _harness.Start().GetAwaiter().GetResult();

            var scope = factory.Services.CreateScope();
            _db = scope.ServiceProvider.GetRequiredService<ContactDbContext>();
        }

        [Fact]
        public async Task PostContact_Should_PersistInDatabase_And_PublishEvent()
        {
            // Arrange
            var req = new CreateContactRequest("Maria", "maria@ex.com", "99999999", "21");

            // Act
            var response = await _client.PostAsJsonAsync("/api/contacts", req);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
            var id = result!["id"];

            // Assert 1: evento publicado
            (await _harness.Published.Any<ContactCreated>()).Should().BeTrue("esperava-se que ContactCreated fosse publicado");

            // Assert 2: espera até o contato estar persistido
            const int maxRetries = 10;
            int retries = 0;
            Domain.Entities.Contact? contact = null;

            while (contact == null && retries++ < maxRetries)
            {
                await Task.Delay(200);
                contact = await _db.Contacts.FindAsync(id);
            }

            contact.Should().NotBeNull("esperava-se que o contato fosse persistido no banco");
            contact!.Name.Should().Be("Maria");
        }
    }
}
