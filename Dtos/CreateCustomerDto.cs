using System.ComponentModel.DataAnnotations;

namespace DSWIntegral.Dtos;
public class CreateCustomerDto
{
    [Required, StringLength(100)]
    public string Name    { get; set; } = null!;
    [Required, EmailAddress]
    public string Email   { get; set; } = null!;
    [Required]
    public string Address { get; set; } = null!;
}