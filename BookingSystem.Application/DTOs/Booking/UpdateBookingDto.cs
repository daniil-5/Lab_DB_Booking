namespace BookingSystem.Application.Booking;

public class UpdateBookingDto : CreateBookingDto
{
    public int Id { get; set; }
    public int Status { get; set; }
}