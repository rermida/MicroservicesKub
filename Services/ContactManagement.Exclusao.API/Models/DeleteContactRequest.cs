namespace ContactManagement.Exclusao.API.Models
{
    public record DeleteContactRequest(
        string Name,
        string Email,
        string Phone,
        string Ddd
    );
}
