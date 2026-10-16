
using System.ComponentModel.DataAnnotations;

namespace Mini_Amazon_API_Core.DTOs
{
    public class UpdateOrderStatusDto
    {
        [Required]
        public string OrderStatus { get; set; } = string.Empty;
    }
}
