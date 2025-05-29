using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ContactManagement.Cadastro.API.Models;
using ContactManagement.E2ETests.Factories;
using ContactManagement.Infrastructure.Data;
using ContactManagement.Messages.Events;
using FluentAssertions;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ContactManagement.E2ETests
{
    public class CadastroE2ETests : IClassFixture<CadastroApiFactory>
    {
        private readonly CadastroApiFactory _factory;
        private readonly HttpClient _client;
        private readonly ITestHarness _harness;
        private readonly IServiceProvider _services;

        public CadastroE2ETests(CadastroApiFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
            _harness = factory.Services.GetRequiredService<ITestHarness>();
            _services = factory.Services;

            _harness.Start().GetAwaiter().GetResult();
        }

        [Fact]
        public async Task PostContact_PublishesContactCreated_AndPersistsInDb()
        {
            // Arrange
            var request = new CreateContactRequest("João", "joao@ex.com", "999999999", "11");

            // Act
            var response = await _client.PostAsJsonAsync("/api/contacts", request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
            var id = result!["id"];

            // ✅ Aguarda o consumo do evento
            (await _harness.Consumed.Any<ContactCreated>())
                .Should().BeTrue("esperava-se que ContactCreated fosse consumido");

            // 🔄 Retry até persistência real
            const int maxTries = 10;
            int tries = 0;
            ContactManagement.Domain.Entities.Contact? contact = null;

            while (contact == null && tries++ < maxTries)
            {
                await Task.Delay(200);

                using var scope = _services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ContactDbContext>();
                contact = await db.Contacts.FindAsync(id);
            }

            // ✅ Assert
            contact.Should().NotBeNull("esperava-se que o contato fosse persistido no banco");
            contact!.Name.Should().Be("João");
        }
    }
}
