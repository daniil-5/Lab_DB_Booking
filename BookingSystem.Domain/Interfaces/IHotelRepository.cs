using System.Linq.Expressions;
using BookingSystem.Domain.DTOs.Hotel;
using BookingSystem.Domain.Entities;

namespace BookingSystem.Domain.Interfaces
{
    public interface IHotelRepository : IRepository<Hotel>
    {
        Task<(IEnumerable<Hotel> hotels, int totalCount)> SearchHotelsAsync(
            Expression<Func<Hotel, bool>> filter = null,
            Func<IQueryable<Hotel>, IOrderedQueryable<Hotel>> orderBy = null,
            int pageNumber = 1,
            int pageSize = 10,
            bool includeRoomTypes = false,
            bool includePhotos = false);
            
        Task<IEnumerable<Hotel>> GetHotelsWithDetailsAsync(
            Expression<Func<Hotel, bool>> filter = null, 
            int pageNumber = 1, 
            int pageSize = 10);
            
        Task<Hotel> GetHotelWithDetailsAsync(int id);
        
        Task<IEnumerable<Hotel>> SearchHotelsByAvailabilityAsync(
            string location, 
            DateTime checkIn, 
            DateTime checkOut, 
            int guests, 
            int pageNumber = 1, 
            int pageSize = 10);

        Task<IEnumerable<HotelStatistics>> GetHotelsWithStatisticsAsync();
        Task<IEnumerable<HotelAvailability>> SearchAvailableHotelsAsync(string location, DateTime checkIn, DateTime checkOut, int guestCount);
        Task<IEnumerable<HotelRanking>> GetHotelsRankedByLocationAsync();
        Task<HotelPerformanceReport> GetHotelPerformanceReportAsync(int hotelId);
        Task<IEnumerable<MonthlyBookingTrend>> GetMonthlyBookingTrendsAsync(int? hotelId = null, int months = 12);
        Task<IEnumerable<Hotel>> GetHotelsOrderedByRatingAndNameAsync();
        Task<IEnumerable<Hotel>> GetPremiumHotelsAsync();
    }
}