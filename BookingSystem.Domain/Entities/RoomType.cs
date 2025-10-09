using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;

namespace BookingSystem.Domain.Entities;

[Table("room_types")]
public class RoomType : BaseEntity
{
    [Column("name")]
    public string Name { get; set; }
    [Column("description")]
    public string Description { get; set; }
    [Column("base_price")]
    public decimal BasePrice { get; set; }
    [Column("capacity")]
    public int Capacity { get; set; } = 2; 
    [Column("area")]
    public decimal Area { get; set; }
    [Column("floor")]
    public int? Floor { get; set; }
    [Column("hotel_id")]
    public int HotelId { get; set; }
    public Hotel Hotel { get; set; }

    public ICollection<RoomPricing> Pricing { get; set; } = new List<RoomPricing>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
