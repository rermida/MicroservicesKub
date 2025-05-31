using System;
using System.Text.Json.Serialization;

namespace ContactManagement.Messages.Events
{
    // Publicado quando um novo contato é criado.
    public record ContactCreated
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Phone { get; init; } = string.Empty;
        public string Ddd { get; init; } = string.Empty;
        public DateTimeOffset OccurredAt { get; init; }

        // Este é o construtor que o System.Text.Json vai usar
        [JsonConstructor]
        public ContactCreated(
            Guid id,
            string name,
            string email,
            string phone,
            string ddd,
            DateTimeOffset occurredAt
        )
        {
            Id = id;
            Name = name;
            Email = email;
            Phone = phone;
            Ddd = ddd;
            OccurredAt = occurredAt;
        }

        // Construtor auxiliar para publicar sem precisar passar a data
        public ContactCreated(
            Guid id,
            string name,
            string email,
            string phone,
            string ddd
        ) : this(id, name, email, phone, ddd, DateTimeOffset.UtcNow)
        {
        }
    }
}
