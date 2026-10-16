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
    public class ProductsController : ControllerBase
    {

        private readonly MDbcontext _context;
        public ProductsController(MDbcontext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts(
            string? search,
            int? categoryId,
            decimal? minPrice,
            decimal? maxPrice,
            string sortBy = "name",
            bool descending = false,
            int page = 1,
            int pageSize = 10)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new
                {
                    message = "Page must be at least 1 and pageSize must be between 1 and 100."
                });
            }

            if (minPrice < 0 || maxPrice < 0)
            {
                return BadRequest(new
                {
                    message = "Prices cannot be negative."
                });
            }

            if (minPrice.HasValue && maxPrice.HasValue &&
                minPrice.Value > maxPrice.Value)
            {
                return BadRequest(new
                {
                    message = "minPrice cannot be greater than maxPrice."
                });
            }

            var query = _context.Products
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(p =>
                    p.ProductName.Contains(search) ||
                    p.ProductDescription.Contains(search));
            }

            if (categoryId.HasValue)
            {
                if (categoryId.Value <= 0)
                {
                    return BadRequest(new
                    {
                        message = "Category ID must be greater than zero."
                    });
                }

                query = query.Where(p =>
                    p.CategoryId == categoryId.Value);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p =>
                    p.ProductPrice >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p =>
                    p.ProductPrice <= maxPrice.Value);
            }

            IOrderedQueryable<Product> orderedQuery;

            switch (sortBy.Trim().ToLowerInvariant())
            {
                case "name":
                    orderedQuery = descending
                        ? query.OrderByDescending(p => p.ProductName)
                        : query.OrderBy(p => p.ProductName);
                    break;

                case "price":
                    orderedQuery = descending
                        ? query.OrderByDescending(p => p.ProductPrice)
                        : query.OrderBy(p => p.ProductPrice);
                    break;

                case "createdat":
                    orderedQuery = descending
                        ? query.OrderByDescending(p => p.ProductCreatedAt)
                        : query.OrderBy(p => p.ProductCreatedAt);
                    break;

                default:
                    return BadRequest(new
                    {
                        message = "sortBy must be name, price, or createdAt."
                    });
            }

            var totalCount = await query.CountAsync();

            var products = await orderedQuery
                .ThenBy(p => p.ProductId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductResponseDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    ProductDescription = p.ProductDescription,
                    ProductPrice = p.ProductPrice,
                    ProductStock = p.ProductStock,
                    ImageUrl = p.ImageUrl,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category!.CategoryName,
                    ProductCreatedAt = p.ProductCreatedAt,
                    ProductUpdatedAt = p.ProductUpdatedAt
                })
                .ToListAsync();

            return Ok(new
            {
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(
                    totalCount / (double)pageSize),
                products
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Product ID must be greater than zero."
                });
            }

            var product = await _context.Products
                .AsNoTracking()
                .Where(p => p.ProductId == id)
                .Select(p => new ProductResponseDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    ProductDescription = p.ProductDescription,
                    ProductPrice = p.ProductPrice,
                    ProductStock = p.ProductStock,
                    ImageUrl = p.ImageUrl,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category!.CategoryName,
                    ProductCreatedAt = p.ProductCreatedAt,
                    ProductUpdatedAt = p.ProductUpdatedAt
                })
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound(new
                {
                    message = "Product not found."
                });
            }

            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(
            CreateProductDto dto)
        {
            var categoryExists = await _context.Categories
                .AnyAsync(c => c.CategoryId == dto.CategoryId);

            if (!categoryExists)
            {
                return BadRequest(new
                {
                    message = "The specified category does not exist."
                });
            }

            var product = new Product
            {
                ProductName = dto.ProductName.Trim(),
                ProductDescription = dto.ProductDescription.Trim(),
                ProductPrice = dto.ProductPrice,
                ProductStock = dto.ProductStock,
                ImageUrl = dto.ImageUrl?.Trim(),
                CategoryId = dto.CategoryId,
                ProductCreatedAt = DateTime.UtcNow,
                ProductUpdatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var response = await _context.Products
                .Where(p => p.ProductId == product.ProductId)
                .Select(p => new ProductResponseDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    ProductDescription = p.ProductDescription,
                    ProductPrice = p.ProductPrice,
                    ProductStock = p.ProductStock,
                    ImageUrl = p.ImageUrl,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category!.CategoryName,
                    ProductCreatedAt = p.ProductCreatedAt,
                    ProductUpdatedAt = p.ProductUpdatedAt
                })
                .FirstAsync();

            return CreatedAtAction(
                nameof(GetProduct),
                new { id = product.ProductId },
                response);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateProduct(
            int id, UpdateProductDto dto)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Product ID must be greater than zero."
                });
            }

            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound(new
                {
                    message = "Product not found."
                });
            }

            var categoryExists = await _context.Categories
                .AnyAsync(c => c.CategoryId == dto.CategoryId);

            if (!categoryExists)
            {
                return BadRequest(new
                {
                    message = "The specified category does not exist."
                });
            }

            product.ProductName = dto.ProductName.Trim();
            product.ProductDescription = dto.ProductDescription.Trim();
            product.ProductPrice = dto.ProductPrice;
            product.ProductStock = dto.ProductStock;
            product.ImageUrl = dto.ImageUrl?.Trim();
            product.CategoryId = dto.CategoryId;
            product.ProductUpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Product ID must be greater than zero."
                });
            }

            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound(new
                {
                    message = "Product not found."
                });
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
