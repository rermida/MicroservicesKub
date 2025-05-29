using System.ComponentModel.DataAnnotations;

namespace ContactManagement.Domain.Entities;

public class Contact
{
    // Construtor sem parâmetros (EF Core precisa dele)
    public Contact() { }

    // Construtor parametrizado para uso no Consumer
    public Contact(Guid id, string name, string email, string phone, string ddd)
    {
        Id    = id;
        Name  = name;
        Email = email;
        Phone = phone;
        Ddd   = ddd;
    }

    public Guid Id { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório.")]
    [MaxLength(100, ErrorMessage = "O nome não pode ter mais de 100 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "O formato do e-mail é inválido.")]
    [MaxLength(255, ErrorMessage = "O e-mail não pode ter mais de 255 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "O número de telefone é obrigatório.")]
    [RegularExpression(@"^\d{8,9}$", ErrorMessage = "O telefone deve conter 8 ou 9 dígitos inteiros.")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "O DDD é obrigatório.")]
    [RegularExpression(@"^\d{2}$", ErrorMessage = "O DDD deve conter exatamente dois números.")]
    [MaxLength(2)]
    public string Ddd { get; set; } = string.Empty;

    /// <summary>
    /// Última vez em que este contato foi lido via API de consulta.
    /// </summary>
    public DateTimeOffset? LastReadAt { get; set; }
}
