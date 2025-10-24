using BookingSystem.Application.Interfaces;
using BookingSystem.Domain.Enums;
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

    [HttpGet("date-range")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetByDateRange([FromQuery] DateTimeOffset startDate, [FromQuery] DateTimeOffset endDate)
    {
        var audits = await _userActionAuditService.GetByDateRangeAsync(startDate, endDate);
        return Ok(audits);
    }

    [HttpGet("most-recent")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetMostRecentUserActions()
    {
        var audits = await _userActionAuditService.GetMostRecentUserActionsAsync();
        return Ok(audits);
    }

    [HttpGet("user/{userId:int}/actions/{actionType}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetActionsByUserAndType(int userId, UserActionType actionType)
    {
        var audits = await _userActionAuditService.GetActionsByUserAndTypeAsync(userId, actionType);
        return Ok(audits);
    }
}
