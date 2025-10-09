using System.ComponentModel.DataAnnotations.Schema;

namespace BookingSystem.Domain.Entities;

[Table("hotel_photos")]
public class HotelPhoto : BaseEntity
{
    [Column("hotel_id")]
    public int HotelId { get; set; }
    public Hotel Hotel { get; set; }
    [Column("url")]
    public string Url { get; set; }
    [Column("public_id")]
    public string PublicId { get; set; }
    [Column("description")]
    public string Description { get; set; }
    [Column("is_main")]
    public bool IsMain { get; set; }
}
