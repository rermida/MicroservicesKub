using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ContactManagement.Cadastro.API.Models;
using ContactManagement.Infrastructure.Data;
using ContactManagement.Messages.Events;
using FluentAssertions;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using ContactManagement.E2ETests.Factories;
using ContactCreated = ContactManagement.Messages.Events.ContactCreated;

namespace ContactManagement.E2ETests
{
    public class CadastroE2ETests
        : IClassFixture<Factories.CadastroApiFactory>
    {
        readonly Factories.CadastroApiFactory _factory;
        readonly HttpClient                  _client;
        readonly ITestHarness                 _harness;

        public CadastroE2ETests(Factories.CadastroApiFactory factory)
        {
            _factory = factory;
            _client  = factory.CreateClient();
            _harness = factory.Services.GetRequiredService<ITestHarness>();
            _harness.Start().GetAwaiter().GetResult();
        }

        [Fact]
        public async Task PostContact_PublishesContactCreated_AndPersistsInDb()
        {
            // Arrange
            var req = new CreateContactRequest(
                "E2E User",
                "e2e@example.com",
                "999000111",
                "21"
            );

            // Act
            var response = await _client.PostAsJsonAsync("/api/contacts", req);

            // Assert HTTP
            response.StatusCode.Should().Be(HttpStatusCode.Accepted);

            // Assert event publicado
            (await _harness.Published.Any<ContactCreated>())
                .Should().BeTrue();

            // Assert inserção no DB InMemory
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ContactDbContext>();
            var exists = await db.Contacts
                .AnyAsync(c => c.Email == "e2e@example.com");
            exists.Should().BeTrue();
        }
    }
}
