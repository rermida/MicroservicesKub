namespace ContactManagement.Cadastro.API.Models
{
    // Payload esperado para criar um novo contato.
    public record CreateContactRequest(
        string Name,
        string Email,
        string Phone,
        string Ddd
    );
}
