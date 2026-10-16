
using System.ComponentModel.DataAnnotations;

namespace Mini_Amazon_API_Core.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        public int UserId { get; set; }

        public Users? User { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(30)]
        public string OrderStatus { get; set; } = "Pending";

        public decimal TotalAmount { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; }
            = new List<OrderItem>();
    }
}