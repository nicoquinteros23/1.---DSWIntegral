using System.ComponentModel.DataAnnotations;

namespace DSWIntegral.Dtos;
public class UpdateCustomerDto
{
    [Required]
    public Guid   Id      { get; set; }
    [Required, StringLength(100)]
    public string Name    { get; set; } = null!;
    [Required, EmailAddress]
    public string Email   { get; set; } = null!;
    [Required]
    public string Address { get; set; } = null!;
}