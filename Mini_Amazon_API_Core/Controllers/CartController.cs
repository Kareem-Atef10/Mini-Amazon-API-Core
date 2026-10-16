
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mini_Amazon_API_Core.DTOs;
using Mini_Amazon_API_Core.Models;

namespace Mini_Amazon_API_Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly MDbcontext _context;

        public CartController(MDbcontext context)
        {
            _context = context;
        }

        // GET: api/cart/1
        [HttpGet("{userId:int}")]
        public async Task<IActionResult> GetCart(int userId)
        {
            var userExists = await _context.Users
                .AnyAsync(u => u.UserId == userId);

            if (!userExists)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            // A user without a cart gets an empty cart response.
            if (cart == null)
            {
                return Ok(new CartResponseDto
                {
                    CartId = null,
                    UserId = userId,
                    Items = new List<CartItemResponseDto>(),
                    TotalPrice = 0
                });
            }

            return Ok(MapCart(cart));
        }

        // POST: api/cart/1/items
        [HttpPost("{userId:int}/items")]
        public async Task<IActionResult> AddToCart(
            int userId,
            AddCartItemDto dto)
        {
            var userExists = await _context.Users
                .AnyAsync(u => u.UserId == userId);

            if (!userExists)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == dto.ProductId);

            if (product == null)
            {
                return NotFound(new
                {
                    message = "Product not found."
                });
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CartCreatedAt = DateTime.UtcNow
                };

                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var cartItem = cart.CartItems
                .FirstOrDefault(ci => ci.ProductId == dto.ProductId);

            int newQuantity = dto.Quantity;

            if (cartItem != null)
            {
                newQuantity += cartItem.Quantity;
            }

            if (newQuantity > product.ProductStock)
            {
                return BadRequest(new
                {
                    message = "Requested quantity exceeds available stock.",
                    availableStock = product.ProductStock,
                    currentCartQuantity = cartItem?.Quantity ?? 0
                });
            }

            if (cartItem != null)
            {
                // Product already exists in the cart.
                cartItem.Quantity = newQuantity;
            }
            else
            {
                cartItem = new CartItem
                {
                    CartId = cart.CartId,
                    ProductId = product.ProductId,
                    Quantity = dto.Quantity
                };

                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            // Reload the cart to retrieve its products.
            var updatedCart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstAsync(c => c.CartId == cart.CartId);

            return Ok(MapCart(updatedCart));
        }

        // PUT: api/cart/1/items/5
        [HttpPut("{userId:int}/items/{productId:int}")]
        public async Task<IActionResult> UpdateCartItem(
            int userId,
            int productId,
            UpdateCartItemDto dto)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                return NotFound(new
                {
                    message = "Cart not found."
                });
            }

            var cartItem = cart.CartItems
                .FirstOrDefault(ci => ci.ProductId == productId);

            if (cartItem == null)
            {
                return NotFound(new
                {
                    message = "Product not found in this cart."
                });
            }

            if (dto.Quantity > cartItem.Product!.ProductStock)
            {
                return BadRequest(new
                {
                    message = "Requested quantity exceeds available stock.",
                    availableStock = cartItem.Product.ProductStock
                });
            }

            cartItem.Quantity = dto.Quantity;

            await _context.SaveChangesAsync();

            return Ok(MapCart(cart));
        }

        // DELETE: api/cart/1/items/5
        [HttpDelete("{userId:int}/items/{productId:int}")]
        public async Task<IActionResult> RemoveCartItem(
            int userId,
            int productId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                return NotFound(new
                {
                    message = "Cart not found."
                });
            }

            var cartItem = cart.CartItems
                .FirstOrDefault(ci => ci.ProductId == productId);

            if (cartItem == null)
            {
                return NotFound(new
                {
                    message = "Product not found in this cart."
                });
            }

            _context.CartItems.Remove(cartItem);

            await _context.SaveChangesAsync();

            var updatedCart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstAsync(c => c.CartId == cart.CartId);

            return Ok(MapCart(updatedCart));
        }

        // DELETE: api/cart/1
        [HttpDelete("{userId:int}")]
        public async Task<IActionResult> ClearCart(int userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                return NotFound(new
                {
                    message = "Cart not found."
                });
            }

            _context.CartItems.RemoveRange(cart.CartItems);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Cart cleared successfully."
            });
        }

        // Convert the Cart entity into a response DTO.
        private static CartResponseDto MapCart(Cart cart)
        {
            var items = cart.CartItems
                .Where(ci => ci.Product != null)
                .Select(ci => new CartItemResponseDto
                {
                    ProductId = ci.ProductId,
                    ProductName = ci.Product!.ProductName,
                    ImageUrl = ci.Product.ImageUrl,
                    UnitPrice = ci.Product.ProductPrice,
                    Quantity = ci.Quantity,
                    Subtotal = ci.Product.ProductPrice * ci.Quantity
                })
                .ToList();

            return new CartResponseDto
            {
                CartId = cart.CartId,
                UserId = cart.UserId,
                Items = items,
                TotalPrice = items.Sum(i => i.Subtotal)
            };
        }
    }
}
