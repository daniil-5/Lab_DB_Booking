using BookingSystem.Domain.Enums;

namespace BookingSystem.Domain.Entities;

public class UserActionAudit : BaseEntity
{
    public int UserId { get; set; }
    public UserActionType UserActionType { get; set; }
    public bool IsSuccess { get; set; }
}