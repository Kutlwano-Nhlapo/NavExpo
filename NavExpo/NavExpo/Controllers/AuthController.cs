using Microsoft.AspNetCore.Mvc;
using NavExpo.DTOs;
using NavExpo.Models;
using NavExpo.Services;

namespace NavExpo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly AuthService _authService;

        public AuthController(UserService userService, AuthService authService)
        {
            _userService = userService;
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Register model)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _userService.GetByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("User with this email already exists"));
                }

                // Create new user
                var user = new User
                {
                    Name = model.Name,
                    Email = model.Email,
                    Password = _authService.HashPassword(model.Password),
                    Age = model.Age,
                    Role = model.Role,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _userService.CreateAsync(user);

                // Generate token
                var token = _authService.GenerateJwtToken(user);

                var response = new AuthResponse
                {
                    Token = token,
                    User = new UserDTO
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Email = user.Email,
                        Age = user.Age,
                        Role = user.Role
                    }
                };

                return Ok(ApiResponse<AuthResponse>.SuccessResponse(response, "Registration successful"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse($"Registration failed: {ex.Message}"));
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Login model)
        {
            try
            {
                // Find user by email
                var user = await _userService.GetByEmailAsync(model.Email);
                if (user == null)
                {
                    return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid email or password"));
                }

                // Verify password
                if (!_authService.VerifyPassword(model.Password, user.Password))
                {
                    return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid email or password"));
                }

                // Generate token
                var token = _authService.GenerateJwtToken(user);

                var response = new AuthResponse
                {
                    Token = token,
                    User = new UserDTO
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Email = user.Email,
                        Age = user.Age,
                        Role = user.Role
                    }
                };

                return Ok(ApiResponse<AuthResponse>.SuccessResponse(response, "Login successful"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse($"Login failed: {ex.Message}"));
            }
        }

        [HttpGet("verify")]
        public async Task<IActionResult> VerifyToken()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid token"));
                }

                var user = await _userService.GetAsync(userId);
                if (user == null)
                {
                    return Unauthorized(ApiResponse<object>.ErrorResponse("User not found"));
                }

                var userDto = new UserDTO
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Age = user.Age,
                    Role = user.Role
                };

                return Ok(ApiResponse<object>.SuccessResponse(new { user = userDto }, "Token verified"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse($"Verification failed: {ex.Message}"));
            }
        }
    }
}
