using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mini_Amazon_API_Core.Models;
using Mini_Amazon_API_Core.DTOs;

namespace Mini_Amazon_API_Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly MDbcontext _context;

        public CategoriesController(MDbcontext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Categories>>> GetCategories()
        {
            var categories = await _context.Categories
           .AsNoTracking()
           .OrderBy(c => c.CategoryId)
           .ToListAsync();

            return Ok(categories);
        }
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Categories>> GetCategory(int id)
        {
            if (id <= 0)
                return BadRequest(new { message = "Category ID must be greater than zero." });
            var category = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CategoryId == id);
            if (category == null)
                return NotFound(new { message = "Category not found." });
            return Ok(category);
        }
        [HttpPost]
        public async Task<ActionResult<Categories>> CreateCategory(CreateCategoryDto dto)
        {
            var category = new Categories
            {
                CategoryName = dto.CategoryName.Trim(),
                CategoryDescription = dto.CategoryDescription.Trim()
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            

            return CreatedAtAction(
                nameof(GetCategory),
                new { id = category.CategoryId },
                category);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCategory(
        int id, UpdateCategoryDto dto)
        {
            if (id <= 0)
                return BadRequest(new { message = "Category ID must be greater than zero." });

            var category = await _context.Categories.FindAsync(id);

            if (category == null)
                return NotFound(new { message = "Category not found." });

            category.CategoryName = dto.CategoryName.Trim();
            category.CategoryDescription = dto.CategoryDescription.Trim();

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            if (id <= 0)
                return BadRequest(new { message = "Category ID must be greater than zero." });

            var category = await _context.Categories.FindAsync(id);

            if (category == null)
                return NotFound(new { message = "Category not found." });

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
