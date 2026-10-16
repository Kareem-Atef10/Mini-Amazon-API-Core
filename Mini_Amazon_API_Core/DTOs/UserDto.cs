using System.ComponentModel.DataAnnotations;

namespace Mini_Amazon_API_Core.DTOs
{
    public class CreateUserDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(254)]
        public string UserEmail { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string UserPassword { get; set; } = string.Empty;

        [RegularExpression("^(Customer|Admin)$")]
        public string UserRole { get; set; } = "Customer";
    }
    public class UpdateUserDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(254)]
        public string UserEmail { get; set; } = string.Empty;

        [RegularExpression("^(Customer|Admin)$")]
        public string UserRole { get; set; } = "Customer";
    }

    public class UserResponseDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
