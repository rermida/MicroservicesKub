using System;
using System.Text.Json.Serialization;

namespace ContactManagement.Messages.Events
{
    // Publicado quando um contato é removido.
    public record ContactDeleted
    {
        public Guid Id { get; init; }
        public DateTimeOffset OccurredAt { get; init; }

        [JsonConstructor]
        public ContactDeleted(
            Guid id,
            DateTimeOffset occurredAt
        )
        {
            Id         = id;
            OccurredAt = occurredAt;
        }

        // Construtor auxiliar que já preenche o timestamp.
        public ContactDeleted(Guid id)
            : this(id, DateTimeOffset.UtcNow)
        {
        }
    }
}
