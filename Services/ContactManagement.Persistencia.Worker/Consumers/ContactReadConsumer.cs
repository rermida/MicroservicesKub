using System.Threading.Tasks;
using MassTransit;
using ContactManagement.Messages.Events;
using ContactManagement.Domain.Repositories;

namespace ContactManagement.Persistencia.Worker.Consumers
{
    public class ContactReadConsumer : IConsumer<ContactRead>
    {
        private readonly IContactRepository _repo;

        public ContactReadConsumer(IContactRepository repo)
        {
            _repo = repo;
        }

        public async Task Consume(ConsumeContext<ContactRead> context)
        {
            var msg = context.Message;

            // 1) Busca a entidade
            var contact = await _repo.GetByIdAsync(msg.Id);
            if (contact is null) 
                return;

            // 2) Atualiza LastReadAt
            contact.LastReadAt = msg.OccurredAt;

            // 3) Persiste a alteração
            await _repo.UpdateAsync(contact);
        }
    }
}
