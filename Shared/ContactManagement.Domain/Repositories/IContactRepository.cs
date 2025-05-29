using ContactManagement.Domain.Entities;

namespace ContactManagement.Domain.Repositories;

public interface IContactRepository
{
    Task<IEnumerable<Contact>> GetAllContactsAsync();
    Task<IEnumerable<Contact>> GetContactsByDDDAsync(string ddd);
    Task<Contact?> GetByIdAsync(Guid id);
    Task AddAsync(Contact contact);
    Task UpdateAsync(Contact contact);
    Task DeleteAsync(Guid id);
}
