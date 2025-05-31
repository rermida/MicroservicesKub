using System;
using System.Text.Json.Serialization;

namespace ContactManagement.Messages.Events
{
    // Publicado sempre que um contato é lido (consulta/visualização).
    public record ContactRead
    {
        public Guid Id { get; init; }
        public DateTimeOffset OccurredAt { get; init; }

        [JsonConstructor]
        public ContactRead(
            Guid id,
            DateTimeOffset occurredAt
        )
        {
            Id         = id;
            OccurredAt = occurredAt;
        }

        // Construtor auxiliar que já preenche o timestamp.
        public ContactRead(Guid id)
            : this(id, DateTimeOffset.UtcNow)
        {
        }
    }
}
