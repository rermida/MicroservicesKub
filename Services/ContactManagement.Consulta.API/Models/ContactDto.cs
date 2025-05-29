namespace ContactManagement.Consulta.API.Models
{
    public record ContactDto(
        Guid   Id,
        string Name,
        string Email,
        string Phone,
        string Ddd
    );
}
