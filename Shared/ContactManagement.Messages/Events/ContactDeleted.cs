using System;
using System.Text.Json.Serialization;

namespace ContactManagement.Messages.Events
{
    /// <summary>
    /// Publicado quando um contato é removido.
    /// </summary>
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

        /// <summary>
        /// Construtor auxiliar que já preenche o timestamp.
        /// </summary>
        public ContactDeleted(Guid id)
            : this(id, DateTimeOffset.UtcNow)
        {
        }
    }
}
