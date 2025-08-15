using System.ComponentModel.DataAnnotations;

public class Product
{
    public int Id { get; set; }
    [Required]
    public required string Name { get; set; }
    public string? Description { get; set; }
}