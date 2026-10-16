using System.ComponentModel.DataAnnotations;

namespace Mini_Amazon_API_Core.Models
{
    public class Cart
    {

        [Key]
        public int CartId { get; set; }

        public int UserId { get; set; }
        public Users? User { get; set; }

        public ICollection<CartItem> CartItems { get; set; }
            = new List<CartItem>();

        public DateTime CartCreatedAt { get; set; } = DateTime.UtcNow;
    }
}
