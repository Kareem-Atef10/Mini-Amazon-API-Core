using System.ComponentModel.DataAnnotations;

namespace Mini_Amazon_API_Core.DTOs
{
    public class CreateProductDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
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

        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }
    }

    public class UpdateProductDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
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

        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }
    }

    public class ProductResponseDto
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public string ProductDescription { get; set; } = string.Empty;

        public decimal ProductPrice { get; set; }

        public int ProductStock { get; set; }

        public string? ImageUrl { get; set; }

        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public DateTime ProductCreatedAt { get; set; }

        public DateTime ProductUpdatedAt { get; set; }
    }
}