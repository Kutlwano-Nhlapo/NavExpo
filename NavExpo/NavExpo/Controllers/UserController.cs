using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NavExpo.DTOs;
using NavExpo.Models;
using NavExpo.Services;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;

    public UsersController(UserService userService)
    {
        _userService = userService;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        try
        {
            var users = await _userService.GetAsync();

            // Remove password from response
            var userDtos = users.Select(u => new UserDTO
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Age = u.Age,
                Role = u.Role
            }).ToList();

            return Ok(ApiResponse<object>.SuccessResponse(new { users = userDtos }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse($"Failed to fetch users: {ex.Message}"));
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(string id)
    {
        try
        {
            var user = await _userService.GetAsync(id);
            if (user == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
            }

            var userDto = new UserDTO
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Age = user.Age,
                Role = user.Role
            };

            return Ok(ApiResponse<UserDTO>.SuccessResponse(userDto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse($"Failed to fetch user: {ex.Message}"));
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] User updatedUser)
    {
        try
        {
            var user = await _userService.GetAsync(id);
            if (user == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
            }

            // Only allow users to update their own profile unless they're admin
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (userRole != "Admin" && userId != id)
            {
                return Forbid();
            }

            updatedUser.Id = id;
            updatedUser.Password = user.Password; // Don't update password here
            updatedUser.CreatedAt = user.CreatedAt;

            await _userService.UpdateAsync(id, updatedUser);

            var userDto = new UserDTO
            {
                Id = updatedUser.Id,
                Name = updatedUser.Name,
                Email = updatedUser.Email,
                Age = updatedUser.Age,
                Role = updatedUser.Role
            };

            return Ok(ApiResponse<UserDTO>.SuccessResponse(userDto, "User updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse($"Failed to update user: {ex.Message}"));
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        try
        {
            var user = await _userService.GetAsync(id);
            if (user == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
            }

            await _userService.RemoveAsync(id);

            return Ok(ApiResponse<object>.SuccessResponse(null, "User deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse($"Failed to delete user: {ex.Message}"));
        }
    }
}