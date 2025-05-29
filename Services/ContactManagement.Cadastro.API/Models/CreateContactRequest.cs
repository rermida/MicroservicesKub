namespace ContactManagement.Cadastro.API.Models
{
    /// <summary>
    /// Payload esperado para criar um novo contato.
    /// </summary>
    public record CreateContactRequest(
        string Name,
        string Email,
        string Phone,
        string Ddd
    );
}
