using System;
using System.Linq;
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
using ContactRead = ContactManagement.Messages.Events.ContactRead;

namespace ContactManagement.E2ETests
{
    public class ConsultaE2ETests :
        IClassFixture<Factories.CadastroApiFactory>,
        IClassFixture<Factories.ConsultaApiFactory>
    {
        private readonly HttpClient       _cadastroClient;
        private readonly HttpClient       _consultaClient;
        private readonly ITestHarness     _harness;
        private readonly ContactDbContext _db;

        public ConsultaE2ETests(
            Factories.CadastroApiFactory cadastroFactory,
            Factories.ConsultaApiFactory consultaFactory)
        {
            _cadastroClient = cadastroFactory.CreateClient();
            _consultaClient = consultaFactory.CreateClient();

            _harness = consultaFactory.Services.GetRequiredService<ITestHarness>();
            _harness.Start().GetAwaiter().GetResult();

            var scope = consultaFactory.Services.CreateScope();
            _db = scope.ServiceProvider.GetRequiredService<ContactDbContext>();
        }

        [Fact]
        public async Task GetById_PublishesContactRead_AndUpdatesLastReadAt()
        {
            // 1) Arrange: cria um contato
            var createReq = new CreateContactRequest(
                "Consulta Teste",
                "consulta@teste.com",
                "87654321",
                "11"
            );
            var postResponse = await _cadastroClient.PostAsJsonAsync("/api/contacts", createReq);
            await _harness.Start();

            var exists = await _harness.Consumed.Any<ContactCreated>();
            exists.Should().BeTrue("evento ContactCreated deveria ter sido consumido");

            await Task.Delay(500); // pequena espera para o consumidor persistir

            postResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);

            var location = postResponse.Headers.Location.ToString();
            var id       = location.Split('/').Last();

            // 2) Act: client de consulta faz GET por ID
            var getResponse = await _consultaClient.GetAsync($"/api/contacts/{id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // 3) Assert: evento ContactRead publicado
            (await _harness.Published.Any<ContactRead>())
                .Should().BeTrue();

            // 4) Assert: LastReadAt populado no banco
            var contact = await _db.Contacts.SingleAsync(c => c.Id == Guid.Parse(id));
            contact.LastReadAt.Should().NotBeNull();
            contact.LastReadAt.Value.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        }
    }
}
