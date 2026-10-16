
using System.ComponentModel.DataAnnotations;

namespace Mini_Amazon_API_Core.DTOs
{
    public class AddCartItemDto
    {
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }

        [Range(1, 1000)]
        public int Quantity { get; set; } = 1;
    }

    public class UpdateCartItemDto
    {
        [Range(1, 1000)]
        public int Quantity { get; set; }
    }

    public class CartItemResponseDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class CartResponseDto
    {
        public int? CartId { get; set; }
        public int UserId { get; set; }
        public List<CartItemResponseDto> Items { get; set; }
            = new List<CartItemResponseDto>();
        public decimal TotalPrice { get; set; }
    }
}
