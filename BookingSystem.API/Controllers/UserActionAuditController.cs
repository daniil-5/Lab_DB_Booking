using BookingSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserActionAuditController : ControllerBase
{
    private readonly IUserActionAuditService _userActionAuditService;

    public UserActionAuditController(IUserActionAuditService userActionAuditService)
    {
        _userActionAuditService = userActionAuditService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var audits = await _userActionAuditService.GetAllAsync();
        return Ok(audits);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var audit = await _userActionAuditService.GetByIdAsync(id);
        return audit == null ? NotFound() : Ok(audit);
    }

    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetByUserId(int userId)
    {
        var audits = await _userActionAuditService.GetByUserIdAsync(userId);
        return Ok(audits);
    }
}
