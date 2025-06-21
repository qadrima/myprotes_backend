using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CrudApiDemo.Models;
using CrudApiDemo.Data;
using CrudApiDemo.Responses;
using Helpers.UsersHelper;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;
using CrudApiDemo.Models.Dto;

namespace CrudApiDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController(AppDbContext context, IConfiguration configuration) : ControllerBase
    {
        private readonly AppDbContext _context = context;
        private readonly IConfiguration _configuration = configuration;

        // Me
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub);

            if (userIdClaim == null)
                return NotFound(new ApiResponse<User>(401, "User ID not found in token", null, "Unauthorized"));

            if (!int.TryParse(userIdClaim.Value, out var userId))
                return NotFound(new ApiResponse<User>(401, "Invalid user ID in token", null, "Unauthorized"));

            var user = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => new UserDetail // Assuming UserMeDto exists in CrudApiDemo.Models.Dto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Status = u.Status,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound(new ApiResponse<UserDetail>(401, "Authentication required", null, "Unauthorized"));

            return Ok(new ApiResponse<UserDetail>(200, "Success", user));
        }

        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDetail>>> GetUsers()
        {
            var users = await _context.Users
                .OrderByDescending(u => u.Id)
                .Select(u => new UserDetail
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Status = u.Status,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt
                })
                .ToListAsync();

            return Ok(new ApiResponse<List<UserDetail>>(200, "Success", users));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDetail>> GetById(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new ApiResponse<UserDetail>(404, "User not found", null, "NotFound"));
            }

            var userDetail = new UserDetail
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Status = user.Status,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };

            return Ok(new ApiResponse<UserDetail>(200, "Success", userDetail));
        }

        [HttpPost]
        public async Task<ActionResult<User>> Create(User user)
        {
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                return BadRequest(new ApiResponse<User>(400, "Email already exists", null, "DuplicateEmail"));
            }

            if (user.Status != "Active" && user.Status != "Inactive")
            {
                return BadRequest(new ApiResponse<User>(400, "Status must be either 'Active' or 'Inactive'.", null, "Invalid"));
            }

            var passwordHasher = new PasswordHasher<User>();
            user.PasswordHash = passwordHasher.HashPassword(user, user.PasswordHash);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = user.Id },
                new ApiResponse<User>(201, "User created", user)
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateDto dto)
        {
            if (dto.Status != "Active" && dto.Status != "Inactive")
            {
                return BadRequest(new ApiResponse<User>(400, "Status must be either 'Active' or 'Inactive'.", null, "Invalid"));
            }

            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound(new ApiResponse<User>(401, "User not found.", null, "Not Found"));

            // Update fields
            user.Name = dto.Name;
            user.Email = dto.Email;
            user.PhoneNumber = dto.PhoneNumber;
            user.Status = dto.Status;
            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            var userDetail = new UserDetail
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Status = user.Status,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };

            return Ok(new ApiResponse<UserDetail>(200, "User updated successfully.", userDetail));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new ApiResponse<object>(404, "User not found", null, "NotFound"));
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object>(200, "User deleted", null));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null)
                return Unauthorized(new ApiResponse<User>(401, "Invalid email or password", null, "Unauthorized"));

            var hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(user, user.PasswordHash, loginDto.Password);

            if (result != PasswordVerificationResult.Success)
                return Unauthorized(new ApiResponse<User>(401, "Invalid email or password", null, "Unauthorized"));

            var token = UsersHelper.GenerateJwtToken(user, _configuration);

            return Ok(new
            {
                token,
                user = new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    user.Status
                }
            });
        }

    }
}
