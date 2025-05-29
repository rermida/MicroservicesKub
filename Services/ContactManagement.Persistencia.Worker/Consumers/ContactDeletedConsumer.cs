using System.Threading.Tasks;
using MassTransit;
using ContactManagement.Messages.Events;
using ContactManagement.Domain.Repositories;

namespace ContactManagement.Persistencia.Worker.Consumers
{
    public class ContactDeletedConsumer : IConsumer<ContactDeleted>
    {
        private readonly IContactRepository _repo;

        public ContactDeletedConsumer(IContactRepository repo) =>
            _repo = repo;

        public async Task Consume(ConsumeContext<ContactDeleted> context)
        {
            // Deletar via repositório
            await _repo.DeleteAsync(context.Message.Id);  // :contentReference[oaicite:2]{index=2} :contentReference[oaicite:3]{index=3}
        }
    }
}
