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
    public class UsersController : ControllerBase
    {

        private readonly MDbcontext _context;
        private readonly IPasswordHasher<Users> _passwordHasher;
        public UsersController(MDbcontext context, IPasswordHasher<Users> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsers()
        {
            var users = await _context.Users
                        .OrderBy(u => u.UserId)
                        .Select(u => new UserResponseDto
                        {
                            UserId = u.UserId,
                            UserName = u.UserName,
                            UserEmail = u.UserEmail,
                            UserRole = u.UserRole,
                            CreatedAt = u.CreatedAt
                        })
                        .ToListAsync();

            return Ok(users);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserResponseDto>> GetUser(int id)
        {
            if (id <= 0)
                return BadRequest(new { message = "User ID must be greater than zero." });

            var user = await _context.Users
                .Where(u => u.UserId == id)
                .Select(u => new UserResponseDto
                {
                    UserId = u.UserId,
                    UserName = u.UserName,
                    UserEmail = u.UserEmail,
                    UserRole = u.UserRole,
                    CreatedAt = u.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound(new { message = "User not found." });

            return Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult<UserResponseDto>> CreateUser(CreateUserDto dto)
        {
            var email = dto.UserEmail.Trim();

            if (await _context.Users.AnyAsync(u => u.UserEmail == email))
            {
                return Conflict(new
                {
                    message = "Email is already registered."
                });
            }

            var user = new Users
            {
                UserName = dto.UserName.Trim(),
                UserEmail = email,
                UserRole = dto.UserRole,
                CreatedAt = DateTime.UtcNow
            };

            user.UserPasswordHash = _passwordHasher.HashPassword(
                user, dto.UserPassword);

            _context.Users.Add(user);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Also handles a duplicate email inserted concurrently.
                return Conflict(new
                {
                    message = "Could not create user. Check that the email is unique."
                });
            }

            var response = new UserResponseDto
            {
                UserId = user.UserId,
                UserName = user.UserName,
                UserEmail = user.UserEmail,
                UserRole = user.UserRole,
                CreatedAt = user.CreatedAt
            };

            return CreatedAtAction(
                nameof(GetUser),
                new { id = user.UserId },
                response);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateUser(
        int id, UpdateUserDto dto)
        {
            if (id <= 0)
                return BadRequest(new { message = "User ID must be greater than zero." });

            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound(new { message = "User not found." });

            var email = dto.UserEmail.Trim();

            if (await _context.Users.AnyAsync(
                u => u.UserEmail == email && u.UserId != id))
            {
                return Conflict(new
                {
                    message = "Email is already registered."
                });
            }

            user.UserName = dto.UserName.Trim();
            user.UserEmail = email;
            user.UserRole = dto.UserRole;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(new
                {
                    message = "Could not update user. Check that the email is unique."
                });
            }

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (id <= 0)
                return BadRequest(new { message = "User ID must be greater than zero." });
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found." });
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
