using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ContactManagement.Cadastro.API.Models;
using ContactManagement.Infrastructure.Data;
using FluentAssertions;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using ContactDeleted = ContactManagement.Messages.Events.ContactDeleted;

namespace ContactManagement.E2ETests
{
    public class ExclusaoE2ETests :
        IClassFixture<Factories.CadastroApiFactory>,
        IClassFixture<Factories.ExclusaoApiFactory>
    {
        private readonly HttpClient      _cadastroClient;
        private readonly HttpClient      _exclusaoClient;
        private readonly ITestHarness    _harness;
        private readonly ContactDbContext _db;

        public ExclusaoE2ETests(
            Factories.CadastroApiFactory  cadastroFactory,
            Factories.ExclusaoApiFactory exclusaoFactory)
        {
            _cadastroClient = cadastroFactory.CreateClient();
            _exclusaoClient = exclusaoFactory.CreateClient();

            _harness = exclusaoFactory.Services.GetRequiredService<ITestHarness>();
            _harness.Start().GetAwaiter().GetResult();

            var scope = exclusaoFactory.Services.CreateScope();
            _db = scope.ServiceProvider.GetRequiredService<ContactDbContext>();
        }

        [Fact]
        public async Task DeleteContact_RemovesEntity_AndPublishesEvent()
        {
            // 1) Arrange: cria um contato
            var createReq = new CreateContactRequest(
                "Excluir Teste",
                "excluir@teste.com",
                "12345678",
                "21"
            );
            var postResponse = await _cadastroClient.PostAsJsonAsync("/api/contacts", createReq);
            postResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);

            var location = postResponse.Headers.Location.ToString();          // ex: /api/contacts/{id}
            var id       = location.Split('/').Last();

            // 2) Act: deleta
            var deleteResponse = await _exclusaoClient.DeleteAsync($"/api/contacts/{id}");
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // 3) Assert: evento ContactDeleted publicado
            (await _harness.Published.Any<ContactDeleted>())
                .Should().BeTrue();

            // 4) Assert: registro removido do banco
            var exists = await _db.Contacts.AnyAsync(c => c.Id.ToString() == id);
            exists.Should().BeFalse();
        }
    }
}
