using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mini_Amazon_API_Core.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }
        [Required]
        [StringLength(100)]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string ProductDescription { get; set; } = string.Empty;

        [Range(0.01, 10000000)]
        public decimal ProductPrice { get; set; }

        [Range(0, 1000000)]
        public int ProductStock { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        public int CategoryId { get; set; }

        public Categories? Category{ get; set; }

        public DateTime ProductCreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime ProductUpdatedAt { get; set; } = DateTime.UtcNow;

    }
}
