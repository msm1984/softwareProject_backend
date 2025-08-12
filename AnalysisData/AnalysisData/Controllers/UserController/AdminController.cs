﻿using AnalysisData.Dtos.UserDto.UserDto;
using AnalysisData.Services.UserService.AdminService.Abstraction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnalysisData.Controllers.UserController;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IAdminRegisterService _adminRegisterService;

    public AdminController(IAdminService adminService, IAdminRegisterService adminRegisterService)
    {
        _adminService = adminService;
        _adminRegisterService = adminRegisterService;
    }

    [Authorize(Policy = "gold")]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegisterDto userRegisterDto)
    {
        await _adminRegisterService.RegisterByAdminAsync(userRegisterDto);
        return Ok(new { massage = "User added successfully" });
    }

    [Authorize(Policy = "gold")]
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers(int page = 0, int limit = 10)
    {
        var usersPagination = await _adminService.GetAllUserAsync(page, limit);
        var userCount = await _adminService.GetUserCountAsync();
        return Ok(new
        {
            users = usersPagination,
            count = userCount,
            thisPage = page,
        });
    }

    [Authorize(Policy = "gold")]
    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        await _adminService.DeleteUserAsync(id);
        return Ok(new { message = "User deleted successfully." });
    }

    [Authorize(Policy = "gold")]
    [HttpPut("users/{id}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateAdminDto updateAdminDto)
    {
        await _adminService.UpdateUserInformationByAdminAsync(id, updateAdminDto);
        return Ok(new { massage = "updated successfully" });
    }
}