using System;
using System.Text.Json.Serialization;

namespace ContactManagement.Messages.Events
{
    // Publicado quando um contato existente é alterado.
    public record ContactUpdated
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Phone { get; init; } = string.Empty;
        public string Ddd { get; init; } = string.Empty;
        public DateTimeOffset OccurredAt { get; init; }

        [JsonConstructor]
        public ContactUpdated(
            Guid id,
            string name,
            string email,
            string phone,
            string ddd,
            DateTimeOffset occurredAt
        )
        {
            Id         = id;
            Name       = name;
            Email      = email;
            Phone      = phone;
            Ddd        = ddd;
            OccurredAt = occurredAt;
        }

        // Construtor auxiliar que já preenche o timestamp.
        public ContactUpdated(
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
