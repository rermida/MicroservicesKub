using System.Threading.Tasks;
using MassTransit;
using ContactManagement.Messages.Events;
using ContactManagement.Domain.Repositories;

namespace ContactManagement.Persistencia.Worker.Consumers
{
    public class ContactUpdatedConsumer : IConsumer<ContactUpdated>
    {
        private readonly IContactRepository _repo;

        public ContactUpdatedConsumer(IContactRepository repo) =>
            _repo = repo;

        public async Task Consume(ConsumeContext<ContactUpdated> context)
        {
            var msg = context.Message;
            // 1) Buscar entidade existente
            var contact = await _repo.GetByIdAsync(msg.Id);
            if (contact is null) return;

            // 2) Atualizar propriedades
            contact.Name  = msg.Name;
            contact.Email = msg.Email;
            contact.Phone = msg.Phone;
            contact.Ddd   = msg.Ddd;

            // 3) Persistir alteração
            await _repo.UpdateAsync(contact);  // :contentReference[oaicite:0]{index=0} :contentReference[oaicite:1]{index=1}
        }
    }
}
