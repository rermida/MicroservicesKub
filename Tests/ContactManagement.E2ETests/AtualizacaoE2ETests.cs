using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ContactManagement.Atualizacao.API.Models;
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

            _harness.Start().GetAwaiter().GetResult();
        }

        [Fact]
        public async Task PutContact_UpdatesPersistedEntity_AndPublishesEvent()
        {
            // Arrange
            var id = Guid.NewGuid();

            using (var scope = _serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ContactDbContext>();
                await db.Contacts.AddAsync(new Contact
                {
                    Id = id,
                    Name = "Original",
                    Email = "original@ex.com",
                    Phone = "12345678",
                    Ddd = "11"
                });
                await db.SaveChangesAsync();
            }

            var updateRequest = new UpdateContactRequest("Novo Nome", "novo@ex.com", "87654321", "21");

            // Act
            var response = await _atualizacaoClient.PutAsJsonAsync($"/api/contacts/{id}", updateRequest);
            response.EnsureSuccessStatusCode();

            // Esperar o evento ser processado
            (await _harness.Consumed.Any<ContactUpdated>())
                .Should().BeTrue("esperava-se que ContactUpdated fosse consumido");

            // Retry: aguardar atualização no banco
            Contact? updated = null;
            const int maxTries = 10;
            var attempts = 0;

            while (updated == null || updated.Name == "Original")
            {
                if (++attempts > maxTries)
                    break;

                await Task.Delay(200); // aguarda 200ms antes de tentar novamente

                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ContactDbContext>();
                updated = await db.Contacts.FindAsync(id);
            }

            // Assert
            updated.Should().NotBeNull("esperava-se que o contato fosse atualizado no banco");
            updated!.Name.Should().Be("Novo Nome");
            updated.Email.Should().Be("novo@ex.com");
        }
    }
}
