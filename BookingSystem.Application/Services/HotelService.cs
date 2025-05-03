using System.Linq.Expressions;
using BookingSystem.Application.DTOs.HotelPhoto;
using BookingSystem.Application.Hotel;
using BookingSystem.Application.RoomType;
using BookingSystem.Domain.Interfaces;

namespace BookingSystem.Application.Services;

 public class HotelService : IHotelService
    {
        private readonly IHotelRepository _hotelRepository;

        public HotelService(IHotelRepository hotelRepository)
        {
            _hotelRepository = hotelRepository;
        }

        public async Task<HotelDto> CreateHotelAsync(CreateHotelDto hotelDto)
        {
            var hotel = new Domain.Entities.Hotel
            {
                Name = hotelDto.Name,
                Description = hotelDto.Description,
                Location = hotelDto.Location,
                Rating = hotelDto.Rating
            };

            await _hotelRepository.AddAsync(hotel);
            return MapToDto(hotel);
        }

        public async Task<HotelDto> UpdateHotelAsync(UpdateHotelDto hotelDto)
        {
            var existingHotel = await _hotelRepository.GetByIdAsync(hotelDto.Id);
            
            existingHotel.Name = hotelDto.Name;
            existingHotel.Description = hotelDto.Description;
            existingHotel.Location = hotelDto.Location;
            existingHotel.Rating = hotelDto.Rating;
            
            await _hotelRepository.UpdateAsync(existingHotel);
            return MapToDto(existingHotel);
        }

        public async Task DeleteHotelAsync(int id)
        {
            await _hotelRepository.DeleteAsync(id);
        }

        public async Task<HotelDto> GetHotelByIdAsync(int id)
        {
            var hotel = await _hotelRepository.GetByIdAsync(id);
            return hotel != null ? MapToDto(hotel) : null;
        }

        public async Task<IEnumerable<HotelDto>> GetAllHotelsAsync()
        {
            var hotels = await _hotelRepository.GetAllAsync();
            return hotels.Select(MapToDto).ToList();
        }
        
        public async Task<HotelSearchResultDto> SearchHotelsAsync(HotelSearchDto searchDto)
        {
            // Create the filter expression
            Expression<Func<Domain.Entities.Hotel, bool>> filter = hotel => true; // Start with all hotels

            // Apply name filter
            if (!string.IsNullOrEmpty(searchDto.Name))
            {
                filter = filter.And(h => h.Name.ToLower().Contains(searchDto.Name.ToLower()));
            }

            // Apply location filter
            if (!string.IsNullOrEmpty(searchDto.Location))
            {
                filter = filter.And(h => h.Location.ToLower().Contains(searchDto.Location.ToLower()));
            }

            // Apply rating range filter
            if (searchDto.MinRating.HasValue)
            {
                filter = filter.And(h => h.Rating >= searchDto.MinRating.Value);
            }

            if (searchDto.MaxRating.HasValue)
            {
                filter = filter.And(h => h.Rating <= searchDto.MaxRating.Value);
            }

            // Apply room type filter
            if (searchDto.RoomTypeId.HasValue)
            {
                filter = filter.And(h => h.RoomTypes.Any(rt => rt.Id == searchDto.RoomTypeId.Value));
            }

            // Create the ordering expression
            Func<IQueryable<Domain.Entities.Hotel>, IOrderedQueryable<Domain.Entities.Hotel>> orderBy = null;
            switch (searchDto.SortBy.ToLower())
            {
                case "name":
                    orderBy = query => searchDto.SortDescending
                        ? query.OrderByDescending(h => h.Name)
                        : query.OrderBy(h => h.Name);
                    break;
                case "location":
                    orderBy = query => searchDto.SortDescending
                        ? query.OrderByDescending(h => h.Location)
                        : query.OrderBy(h => h.Location);
                    break;
                case "rating":
                default:
                    orderBy = query => searchDto.SortDescending
                        ? query.OrderByDescending(h => h.Rating)
                        : query.OrderBy(h => h.Rating);
                    break;
            }

            // Execute the search
            var (hotels, totalCount) = await _hotelRepository.SearchHotelsAsync(
                filter,
                orderBy,
                searchDto.PageNumber,
                searchDto.PageSize,
                includeRoomTypes: searchDto.RoomTypeId.HasValue,
                includePhotos: true
            );

            // Calculate pagination values
            var totalPages = (int)Math.Ceiling(totalCount / (double)searchDto.PageSize);
            var hasPrevious = searchDto.PageNumber > 1;
            var hasNext = searchDto.PageNumber < totalPages;

            // Map results to DTOs
            var hotelDtos = hotels.Select(MapToDto).ToList();

            // Create and return the search result DTO
            return new HotelSearchResultDto
            {
                Hotels = hotelDtos,
                TotalCount = totalCount,
                PageNumber = searchDto.PageNumber,
                PageSize = searchDto.PageSize,
                TotalPages = totalPages,
                HasPrevious = hasPrevious,
                HasNext = hasNext
            };
        }
        
        private static HotelDto MapToDto(Domain.Entities.Hotel hotel)
        {
            return new HotelDto
            {
                Id = hotel.Id,
                Name = hotel.Name,
                Description = hotel.Description,
                Location = hotel.Location,
                Rating = hotel.Rating,
                CreatedAt = hotel.CreatedAt,
                UpdatedAt = hotel.UpdatedAt,
                // // Map photos if needed
                // Photos = hotel.Photos?.Select(p => new HotelPhotoDto
                // {
                //     Id = p.Id,
                //     Url = p.Url,
                //     IsMain = p.IsMain,
                //     Description = p.Description
                // }).ToList(),
                // // Map room types if needed
                // RoomTypes = hotel.RoomTypes?.Select(rt => new RoomTypeDto
                // {
                //     Id = rt.Id,
                //     Name = rt.Name,
                //     Capacity = rt.Capacity
                // }).ToList()
            };
        }
    }
 public static class ExpressionExtensions
 {
     public static Expression<Func<T, bool>> And<T>(
         this Expression<Func<T, bool>> left,
         Expression<Func<T, bool>> right)
     {
         var parameter = Expression.Parameter(typeof(T));
         var leftVisitor = new ReplaceParameterVisitor(left.Parameters[0], parameter);
         var leftBody = leftVisitor.Visit(left.Body);
         var rightVisitor = new ReplaceParameterVisitor(right.Parameters[0], parameter);
         var rightBody = rightVisitor.Visit(right.Body);
         return Expression.Lambda<Func<T, bool>>(
             Expression.AndAlso(leftBody, rightBody), parameter);
     }

     private class ReplaceParameterVisitor : System.Linq.Expressions.ExpressionVisitor
     {
         private readonly ParameterExpression _old;
         private readonly ParameterExpression _new;

         public ReplaceParameterVisitor(ParameterExpression old, ParameterExpression @new)
         {
             _old = old;
             _new = @new;
         }

         protected override Expression VisitParameter(ParameterExpression node)
         {
             return node == _old ? _new : base.VisitParameter(node);
         }
     }
 }