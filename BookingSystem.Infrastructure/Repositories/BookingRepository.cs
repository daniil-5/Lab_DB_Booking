
using BookingSystem.Domain.Entities;
using BookingSystem.Infrastructure.Data;

namespace BookingSystem.Infrastructure.Repositories;

public class BookingRepository : BaseRepository<Booking>
{
    public BookingRepository(DapperDbContext context) : base(context)
    {
    }
}
