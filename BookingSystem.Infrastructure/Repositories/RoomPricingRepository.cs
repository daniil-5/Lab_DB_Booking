
using BookingSystem.Domain.Entities;
using BookingSystem.Infrastructure.Data;

namespace BookingSystem.Infrastructure.Repositories;

public class RoomPricingRepository : BaseRepository<RoomPricing>
{
    public RoomPricingRepository(DapperDbContext context) : base(context)
    {
    }
}
