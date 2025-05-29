using System.Threading.Tasks;
using MassTransit;
using ContactManagement.Messages.Events;
using ContactManagement.Domain;
using ContactManagement.Infrastructure;
using ContactManagement.Infrastructure.Data;
using ContactManagement.Domain.Repositories;
using ContactManagement.Domain.Entities;

namespace ContactManagement.Persistencia.Worker.Consumers
{
    public class ContactCreatedConsumer : IConsumer<ContactCreated>
    {
        private readonly ContactDbContext _db;
        private readonly IContactRepository _repo;

        public ContactCreatedConsumer(ContactDbContext db, IContactRepository repo)
        {
            _db   = db;
            _repo = repo;
        }

        public async Task Consume(ConsumeContext<ContactCreated> context)
        {
            var msg = context.Message;

            // 1) Mapear para entidade de domínio
            var contact = new Contact
            {
                Id    = msg.Id,
                Name  = msg.Name,
                Email = msg.Email,
                Phone = msg.Phone,
                Ddd   = msg.Ddd
            };

            // 2) Usar repositório para persistir
            await _repo.AddAsync(contact);
            await _db.SaveChangesAsync();
        }
    }
}
