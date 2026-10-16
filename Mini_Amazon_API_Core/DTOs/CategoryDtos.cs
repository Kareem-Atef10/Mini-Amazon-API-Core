using System.ComponentModel.DataAnnotations;

namespace Mini_Amazon_API_Core.DTOs
{
    public class CreateCategoryDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string CategoryName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string CategoryDescription { get; set; } = string.Empty;
    }

    public class UpdateCategoryDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string CategoryName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string CategoryDescription { get; set; } = string.Empty;
    }
}
