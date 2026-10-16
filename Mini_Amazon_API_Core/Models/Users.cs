using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mini_Amazon_API_Core.Models
{
    public class Users
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(254)]
        public string UserEmail { get; set; } = string.Empty;

        [Required]
        public string UserPasswordHash { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^(Customer|Admin)$",
        ErrorMessage = "Role must be Customer or Admin.")]
        public string UserRole { get; set; } = "Customer";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


    }
}
