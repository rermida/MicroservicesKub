using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ContactManagement.Cadastro.API.Models;
using ContactManagement.Domain.Entities;
using ContactManagement.E2ETests.Factories;
using ContactManagement.Infrastructure.Data;
using ContactManagement.Messages.Events;
using FluentAssertions;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ContactManagement.E2ETests
{
    public class ConsultaE2ETests :
        IClassFixture<CadastroApiFactory>,
        IClassFixture<ConsultaApiFactory>
    {
        private readonly HttpClient _cadastroClient;
        private readonly HttpClient _consultaClient;
        private readonly ITestHarness _harness;
        private readonly IServiceProvider _consultaServices;

        public ConsultaE2ETests(
            CadastroApiFactory cadastroFactory,
            ConsultaApiFactory consultaFactory)
        {
            _cadastroClient = cadastroFactory.CreateClient();
            _consultaClient = consultaFactory.CreateClient();
            _harness = consultaFactory.Services.GetRequiredService<ITestHarness>();
            _consultaServices = consultaFactory.Services;

            _harness.Start().GetAwaiter().GetResult();
        }

        [Fact]
        public async Task GetById_PublishesContactRead_AndUpdatesLastReadAt()
        {
            // Arrange: Cria o contato via API de Cadastro
            var request = new CreateContactRequest("Carlos", "carlos@ex.com", "123123123", "21");
            var response = await _cadastroClient.PostAsJsonAsync("/api/contacts", request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
            var id = result!["id"];

            // 🔄 Aguarda persistência no banco da Consulta
            const int maxRetries = 10;
            int retries = 0;
            Contact? contact = null;

            while (contact == null && retries++ < maxRetries)
            {
                await Task.Delay(200);

                using var scope = _consultaServices.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ContactDbContext>();
                contact = await db.Contacts.FindAsync(id);
            }

            contact.Should().NotBeNull("esperava-se que o contato fosse persistido no banco da Consulta");

            // Act - consulta pela API de Consulta
            var consultaResponse = await _consultaClient.GetAsync($"/api/contacts/{id}");
            consultaResponse.EnsureSuccessStatusCode();

            // Assert - evento de leitura foi consumido
            (await _harness.Consumed.Any<ContactRead>())
                .Should().BeTrue("esperava-se que ContactRead fosse consumido");

            // Assert - campo LastReadAt atualizado
            using (var scope = _consultaServices.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ContactDbContext>();
                var reloaded = await db.Contacts.FindAsync(id);
                reloaded!.LastReadAt.Should().NotBeNull("esperava-se que LastReadAt fosse atualizado após leitura");
            }
        }
    }
}
