
using BookingSystem.Domain.Entities;
using BookingSystem.Infrastructure.Data;

namespace BookingSystem.Infrastructure.Repositories;

public class HotelPhotoRepository : BaseRepository<HotelPhoto>
{
    public HotelPhotoRepository(DapperDbContext context) : base(context)
    {
    }
}
