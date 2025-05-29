using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ContactManagement.Atualizacao.API.Models;
using FluentAssertions;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ContactManagement.E2ETests
{
    public class AtualizacaoE2ETests :
        IClassFixture<Factories.CadastroApiFactory>,
        IClassFixture<Factories.AtualizacaoApiFactory>
    {
        private readonly HttpClient _cadastroClient;
        private readonly HttpClient _atualizacaoClient;
        private readonly ITestHarness _harness;
        private readonly IServiceProvider _serviceProvider;

        public AtualizacaoE2ETests(
            Factories.CadastroApiFactory cad,
            Factories.AtualizacaoApiFactory atu)
        {
            _cadastroClient = cad.CreateClient();
            _atualizacaoClient = atu.CreateClient();
            _harness = atu.Services.GetRequiredService<ITestHarness>();
            _serviceProvider = atu.Services;
        }

        [Fact]
        public async Task PutContact_UpdatesPersistedEntity_AndPublishesEvent()
        {
            // 1) Cria um contato via cadastro
            var post = await _cadastroClient.PostAsJsonAsync("/api/contacts", new
            {
                Name = "Before",
                Email = "before@ex.com",
                Phone = "1234",
                Ddd = "11"
            });
            post.StatusCode.Should().Be(HttpStatusCode.Accepted);
            var id = (await post.Content.ReadFromJsonAsync<Dictionary<string, string>>())!["id"];

            // 2) Limpa mensagens pendentes e start harness
            await _harness.Start();

            // 3) PUT para alterar
            var updateReq = new UpdateContactRequest("After", "after@ex.com", "5678", "11");
            var put = await _atualizacaoClient.PutAsJsonAsync($"/api/contacts/{id}", updateReq);
            put.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // 4) Evento publicado?
            (await _harness.Published.Any<ContactManagement.Messages.Events.ContactUpdated>()).Should().BeTrue();

            // 5) Checa no DB com novo escopo
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<Infrastructure.Data.ContactDbContext>();
            var entity = await db.Contacts.FindAsync(Guid.Parse(id));

            entity.Should().NotBeNull();
            entity!.Name.Should().Be("After");
            entity.Email.Should().Be("after@ex.com");
        }
    }
}
