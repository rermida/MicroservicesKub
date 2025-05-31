using System.Net.Http;
using ContactManagement.Infrastructure.Data;
using ContactManagement.Messages.Events;
using FluentAssertions;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ContactManagement.IntegrationTests
{
    public class ExclusaoIntegrationTests : IClassFixture<Factories.ExclusaoApiFactory>
    {
        private readonly HttpClient _client;
        private readonly ITestHarness _harness;
        private readonly ContactDbContext _db;

        public ExclusaoIntegrationTests(Factories.ExclusaoApiFactory factory)
        {
            _client = factory.CreateClient();
            _harness = factory.Services.GetRequiredService<ITestHarness>();
            _harness.Start().GetAwaiter().GetResult();

            var scope = factory.Services.CreateScope();
            _db = scope.ServiceProvider.GetRequiredService<ContactDbContext>();
        }

        [Fact]
        public async Task DeleteContact_Should_PublishDeletedEvent()
        {
            var id = Guid.NewGuid();
            await _db.Contacts.AddAsync(new Domain.Entities.Contact(id, "Pedro", "pedro@ex.com", "11112222", "31"));
            await _db.SaveChangesAsync();

            var response = await _client.DeleteAsync($"/api/contacts/{id}");
            response.EnsureSuccessStatusCode();

            (await _harness.Published.Any<ContactDeleted>()).Should().BeTrue();
        }
    }
}
