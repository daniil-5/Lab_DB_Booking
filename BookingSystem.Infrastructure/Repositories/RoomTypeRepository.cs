
using BookingSystem.Domain.Entities;
using BookingSystem.Infrastructure.Data;

namespace BookingSystem.Infrastructure.Repositories;

public class RoomTypeRepository : BaseRepository<RoomType>
{
    public RoomTypeRepository(DapperDbContext context) : base(context)
    {
    }
}
