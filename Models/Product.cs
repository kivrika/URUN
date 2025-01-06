using System.ComponentModel.DataAnnotations;
namespace URUN.Models;
public class Product
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name alanı zorunludur.")]
    [StringLength(100, ErrorMessage = "Name alanı en fazla 100 karakter olabilir.")]
     public string Name { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Price sıfırdan büyük olmalıdır.")]
    public decimal Price { get; set; }

     [Range(0, int.MaxValue, ErrorMessage = "Stock sıfırdan küçük olamaz.")]
     public int Stock { get; set; }
     [Required(ErrorMessage = "CategoryId alanı zorunludur.")]
     public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public Guid CreatedBy { get; set; } = Guid.NewGuid();
}
