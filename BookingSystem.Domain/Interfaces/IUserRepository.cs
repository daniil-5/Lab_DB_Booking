using BookingSystem.Domain.Entities;
namespace BookingSystem.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User> GetByEmailAsync(string email);
}