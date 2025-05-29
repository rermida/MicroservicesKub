using System;
using System.Text.Json.Serialization;

namespace ContactManagement.Messages.Events
{
    /// <summary>
    /// Publicado sempre que um contato é lido (consulta/visualização).
    /// </summary>
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

        /// <summary>
        /// Construtor auxiliar que já preenche o timestamp.
        /// </summary>
        public ContactRead(Guid id)
            : this(id, DateTimeOffset.UtcNow)
        {
        }
    }
}
