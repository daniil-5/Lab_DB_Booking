using BookingSystem.Application.Dtos.Hotel;
using BookingSystem.Application.DTOs.Hotel;
using BookingSystem.Application.Hotel;
using BookingSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelsController : ControllerBase
    {
        private readonly IHotelService _hotelService;
        private readonly ILogger<HotelsController> _logger;

        public HotelsController(IHotelService hotelService, ILogger<HotelsController> logger)
        {
            _hotelService = hotelService;
            _logger = logger;
        }

        // GET: api/Hotels
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<HotelDto>>> GetHotels()
        {
            try
            {
                _logger.LogInformation("Getting all hotels");
                var hotels = await _hotelService.GetAllHotelsAsync();
                return Ok(hotels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all hotels");
                return StatusCode(500, "An error occurred while retrieving hotels");
            }
        }

        // GET: api/Hotels/5
        [HttpGet("{id}")]
        [AllowAnonymous] // Allow public access to hotel details
        public async Task<ActionResult<HotelDto>> GetHotel(int id)
        {
            try
            {
                _logger.LogInformation("Getting hotel with ID: {HotelId}", id);
                var hotel = await _hotelService.GetHotelByIdAsync(id);
                
                if (hotel == null)
                {
                    _logger.LogWarning("Hotel with ID: {HotelId} not found", id);
                    return NotFound($"Hotel with ID {id} not found");
                }
                
                return Ok(hotel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hotel with ID: {HotelId}", id);
                return StatusCode(500, "An error occurred while retrieving the hotel");
            }
        }

        // POST: api/Hotels
        /// <summary>
        /// Creates a new hotel
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        /// {
        ///     "name": "Urban Boutique Hotel",
        ///     "description": "A stylish hotel in the heart of the city, close to attractions.",
        ///     "location": "New York",
        ///     "address": "654 City Blvd, Metropolis",
        ///     "rating": 4.9,
        ///     "basePrice": 199,
        ///     "checkInTime": "15:00",
        ///     "checkOutTime": "11:00",
        ///     "amenities": ["WiFi", "Pool", "Gym"]
        /// }
        /// </remarks>
        [HttpPost]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<ActionResult<HotelDto>> CreateHotel(CreateHotelDto hotelDto)
        {
            try
            {
                _logger.LogInformation("Creating new hotel: {HotelName}", hotelDto.Name);
                var createdHotel = await _hotelService.CreateHotelAsync(hotelDto);
                return CreatedAtAction(nameof(GetHotel), new { id = createdHotel.Id }, createdHotel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating hotel");
                return StatusCode(500, "An error occurred while creating the hotel");
            }
        }

        // PUT: api/Hotels/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> UpdateHotel(int id, UpdateHotelDto hotelDto)
        {
            if (id != hotelDto.Id)
            {
                return BadRequest("ID mismatch between URL and body");
            }

            try
            {
                _logger.LogInformation("Updating hotel with ID: {HotelId}", id);
                await _hotelService.UpdateHotelAsync(hotelDto);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Hotel with ID: {HotelId} not found for update", id);
                return NotFound($"Hotel with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating hotel with ID: {HotelId}", id);
                return StatusCode(500, "An error occurred while updating the hotel");
            }
        }

        // DELETE: api/Hotels/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> DeleteHotel(int id)
        {
            try
            {
                _logger.LogInformation("Deleting hotel with ID: {HotelId}", id);
                await _hotelService.DeleteHotelAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Hotel with ID: {HotelId} not found for deletion", id);
                return NotFound($"Hotel with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting hotel with ID: {HotelId}", id);
                return StatusCode(500, "An error occurred while deleting the hotel");
            }
        }
        
        // GET: api/Hotels/search
        [HttpGet("search")]
        [AllowAnonymous] // Allow public access to search
        public async Task<ActionResult<HotelSearchResultDto>> SearchHotels([FromQuery] HotelSearchDto searchDto)
        {
            try
            {
                _logger.LogInformation("Searching hotels with criteria: {SearchCriteria}", searchDto);
                var result = await _hotelService.SearchHotelsAsync(searchDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching hotels");
                return StatusCode(500, "An error occurred while searching for hotels");
            }
        }
    }
}