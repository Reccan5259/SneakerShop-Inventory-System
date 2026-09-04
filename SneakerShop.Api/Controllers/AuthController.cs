using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SneakerShop.Api.Data;
using SneakerShop.Api.DTOs;
using SneakerShop.Api.Models;

namespace SneakerShop.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly SneakerShopDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AuthController(
            SneakerShopDbContext context,
            IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register(
            RegisterRequest request)
        {
            string username = request.Username.Trim();
            string email = request.Email.Trim().ToLower();

            bool usernameExists = await _context.Users.AnyAsync(
                user => user.Username.ToLower() == username.ToLower());

            if (usernameExists)
            {
                return Conflict(new AuthResponse
                {
                    Success = false,
                    Message = "Username is already registered."
                });
            }

            bool emailExists = await _context.Users.AnyAsync(
                user => user.Email.ToLower() == email);

            if (emailExists)
            {
                return Conflict(new AuthResponse
                {
                    Success = false,
                    Message = "Email address is already registered."
                });
            }

            var user = new User
            {
                FullName = request.FullName.Trim(),
                Username = username,
                Email = email,
                Role = "Staff",
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash =
                _passwordHasher.HashPassword(user, request.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new AuthResponse
            {
                Success = true,
                Message = "Registration successful.",
                UserId = user.Id,
                FullName = user.FullName,
                Username = user.Username,
                Role = user.Role
            });
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(
            LoginRequest request)
        {
            string username = request.Username.Trim().ToLower();

            var user = await _context.Users.FirstOrDefaultAsync(
                existingUser =>
                    existingUser.Username.ToLower() == username);

            if (user == null)
            {
                return Unauthorized(new AuthResponse
                {
                    Success = false,
                    Message = "Invalid username or password."
                });
            }

            PasswordVerificationResult result =
                _passwordHasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    request.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new AuthResponse
                {
                    Success = false,
                    Message = "Invalid username or password."
                });
            }

            if (result ==
                PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.PasswordHash =
                    _passwordHasher.HashPassword(
                        user,
                        request.Password);

                await _context.SaveChangesAsync();
            }

            return Ok(new AuthResponse
            {
                Success = true,
                Message = "Login successful.",
                UserId = user.Id,
                FullName = user.FullName,
                Username = user.Username,
                Role = user.Role
            });
        }
    }
}