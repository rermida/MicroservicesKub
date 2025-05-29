namespace ContactManagement.Atualizacao.API.Models
{
    public record UpdateContactRequest(
        string Name,
        string Email,
        string Phone,
        string Ddd
    );
}
