using BookingSystem.Application.Hotel;
using BookingSystem.Application.Services;
using Microsoft.AspNetCore.Mvc;
using BookingSystem.Domain.Entities;
using BookingSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;


namespace BookingSystem.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class HotelsController : ControllerBase
    {
        private readonly IHotelService _hotelService;

        public HotelsController(IHotelService hotelService)
        {
            _hotelService = hotelService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<HotelDto>>> GetHotels()
        {
            var hotels = await _hotelService.GetAllHotelsAsync();
            return Ok(hotels);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<HotelDto>> GetHotel(int id)
        {
            var hotel = await _hotelService.GetHotelByIdAsync(id);
            return hotel != null ? Ok(hotel) : NotFound();
        }
        /// <summary>
        /// {
        ///     "Name": "Urban Boutique Hotel",
        ///     "Description": "A stylish hotel in the heart of the city, close to attractions.",
        ///     "Location": "654 City Blvd, Metropolis",
        ///     "Rating": 4.9
        /// }
        /// </summary>
        /// <param name="hotelDto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<HotelDto>> CreateHotel(CreateHotelDto hotelDto)
        {
            var createdHotel = await _hotelService.CreateHotelAsync(hotelDto);
            return CreatedAtAction(nameof(GetHotel), new { id = createdHotel.Id }, createdHotel);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHotel(int id, UpdateHotelDto hotelDto)
        {
            if (id != hotelDto.Id) return BadRequest("ID mismatch");
            
            await _hotelService.UpdateHotelAsync(hotelDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHotel(int id)
        {
            await _hotelService.DeleteHotelAsync(id);
            return NoContent();
        }
        
        [HttpGet("search")]
        public async Task<ActionResult<HotelSearchResultDto>> SearchHotels([FromQuery] HotelSearchDto searchDto)
        {
            var result = await _hotelService.SearchHotelsAsync(searchDto);
            return Ok(result);
        }
    }
}