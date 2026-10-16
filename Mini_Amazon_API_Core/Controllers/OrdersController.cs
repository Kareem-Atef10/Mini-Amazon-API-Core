
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mini_Amazon_API_Core.DTOs;
using Mini_Amazon_API_Core.Models;

namespace Mini_Amazon_API_Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly MDbcontext _context;

        public OrdersController(MDbcontext context)
        {
            _context = context;
        }

        // POST: api/orders/1
        [HttpPost("{userId:int}")]
        public async Task<IActionResult> PlaceOrder(int userId)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            var userExists = await _context.Users
                .AnyAsync(u => u.UserId == userId);

            if (!userExists)
            {
                return NotFound(new { message = "User not found." });
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                return BadRequest(new
                {
                    message = "Your cart is empty."
                });
            }

            // Validate all items before changing stock.
            foreach (var item in cart.CartItems)
            {
                if (item.Product == null)
                {
                    return BadRequest(new
                    {
                        message = $"Product {item.ProductId} was not found."
                    });
                }

                if (item.Quantity <= 0)
                {
                    return BadRequest(new
                    {
                        message = "Product quantities must be greater than zero."
                    });
                }

                if (item.Quantity > item.Product.ProductStock)
                {
                    return BadRequest(new
                    {
                        message = $"Insufficient stock for {item.Product.ProductName}.",
                        availableStock = item.Product.ProductStock,
                        requestedQuantity = item.Quantity
                    });
                }
            }

            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                OrderStatus = "Pending",
                TotalAmount = 0
            };

            foreach (var item in cart.CartItems)
            {
                var product = item.Product!;

                var orderItem = new OrderItem
                {
                    ProductId = product.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = product.ProductPrice
                };

                order.OrderItems.Add(orderItem);

                order.TotalAmount +=
                    product.ProductPrice * item.Quantity;

                product.ProductStock -= item.Quantity;
            }

            _context.Orders.Add(order);

            // Save the order, order items, and stock changes.
            _context.CartItems.RemoveRange(cart.CartItems);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var response = new OrderResponseDto
            {
                OrderId = order.OrderId,
                UserId = order.UserId,
                OrderDate = order.OrderDate,
                OrderStatus = order.OrderStatus,
                TotalAmount = order.TotalAmount,
                Items = order.OrderItems.Select(oi =>
                    new OrderItemResponseDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.Product!.ProductName,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        Subtotal = oi.UnitPrice * oi.Quantity
                    }).ToList()
            };

            return CreatedAtAction(
                nameof(GetOrder),
                new { userId = userId, orderId = order.OrderId },
                response);
        }

        // GET: api/orders/1/5
        [HttpGet("{userId:int}/{orderId:int}")]
        public async Task<IActionResult> GetOrder(
            int userId,
            int orderId)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o =>
                    o.UserId == userId &&
                    o.OrderId == orderId);

            if (order == null)
            {
                return NotFound(new
                {
                    message = "Order not found for this user."
                });
            }

            var response = new OrderResponseDto
            {
                OrderId = order.OrderId,
                UserId = order.UserId,
                OrderDate = order.OrderDate,
                OrderStatus = order.OrderStatus,
                TotalAmount = order.TotalAmount,
                Items = order.OrderItems.Select(oi =>
                    new OrderItemResponseDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.Product?.ProductName
                            ?? "Deleted product",
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        Subtotal = oi.UnitPrice * oi.Quantity
                    }).ToList()
            };

            return Ok(response);
        }

        // GET: api/orders/1
        [HttpGet("{userId:int}")]
        public async Task<IActionResult> GetUserOrders(int userId)
        {
            var userExists = await _context.Users
                .AnyAsync(u => u.UserId == userId);

            if (!userExists)
            {
                return NotFound(new { message = "User not found." });
            }

            var orders = await _context.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderResponseDto
                {
                    OrderId = o.OrderId,
                    UserId = o.UserId,
                    OrderDate = o.OrderDate,
                    OrderStatus = o.OrderStatus,
                    TotalAmount = o.TotalAmount,
                    Items = o.OrderItems.Select(oi =>
                        new OrderItemResponseDto
                        {
                            ProductId = oi.ProductId,
                            ProductName = oi.Product!.ProductName,
                            Quantity = oi.Quantity,
                            UnitPrice = oi.UnitPrice,
                            Subtotal = oi.UnitPrice * oi.Quantity
                        }).ToList()
                })
                .ToListAsync();

            return Ok(orders);
        }
        // GET: api/orders
        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _context.Orders
                .AsNoTracking()
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderResponseDto
                {
                    OrderId = o.OrderId,
                    UserId = o.UserId,
                    OrderDate = o.OrderDate,
                    OrderStatus = o.OrderStatus,
                    TotalAmount = o.TotalAmount,

                    Items = o.OrderItems.Select(oi => new OrderItemResponseDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.Product != null
                            ? oi.Product.ProductName
                            : "Unknown Product",
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        Subtotal = oi.Quantity * oi.UnitPrice
                    }).ToList()
                })
                .ToListAsync();

            return Ok(orders);
        }

        //Admin Order Status Management
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(
            int id,
            int? userId,
            [FromBody] UpdateOrderStatusDto dto)
        {
            // 1. Find the user and verify their role
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId.Value);

            if (user == null || user.UserRole != "Admin")
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "Only administrators can update order status."
                });
            }

            // 2. Validate the requested status
            string[] allowedStatuses =
            {
                 "Pending",
                 "Processing",
                 "Shipped",
                 "Delivered",
                 "Cancelled"
            };

            string? status = allowedStatuses.FirstOrDefault(
                s => s.Equals(
                    dto.OrderStatus?.Trim(),
                    StringComparison.OrdinalIgnoreCase));

            if (status == null)
            {
                return BadRequest(new
                {
                    message = "Invalid order status.",
                    allowedStatuses
                });
            }

            // 3. Find the order
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound(new
                {
                    message = $"Order with ID {id} was not found."
                });
            }

            // 4. Update and save
            order.OrderStatus = status;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Order status updated successfully.",
                orderId = order.OrderId,
                orderStatus = order.OrderStatus
            });
        }
    }
}