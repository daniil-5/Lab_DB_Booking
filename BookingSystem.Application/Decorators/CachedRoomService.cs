using BookingSystem.Application.DTOs.Room;
using BookingSystem.Application.Interfaces;
using BookingSystem.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace BookingSystem.Application.Decorators
{
    public class CachedRoomService : IRoomService
    {
        private readonly IRoomService _roomService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<CachedRoomService> _logger;

        // Cache key constants
        private const string ROOM_BY_ID_KEY = "room:id:{0}";
        private const string ALL_ROOMS_KEY = "rooms:all";
        private const string ROOMS_BY_ROOMTYPE_KEY = "rooms:roomtype:{0}";
        private const string ROOM_PREFIX = "room";
        private const string ROOMS_PREFIX = "rooms";

        // Cache expiration times
        private static readonly TimeSpan RoomCacheExpiration = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan RoomListExpiration = TimeSpan.FromMinutes(20);

        public CachedRoomService(
            IRoomService roomService,
            ICacheService cacheService,
            ILogger<CachedRoomService> logger)
        {
            _roomService = roomService;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<RoomDto> CreateRoomAsync(CreateRoomDto roomDto)
        {
            _logger.LogInformation("Creating new room: {RoomNumber} for RoomType: {RoomTypeId}",
                roomDto.RoomNumber, roomDto.RoomTypeId);

            var newRoom = await _roomService.CreateRoomAsync(roomDto);

            await CacheRoom(newRoom);
            await InvalidateListCaches();

            _logger.LogInformation("Room created and cached with ID: {RoomId}", newRoom.Id);
            return newRoom;
        }

        public async Task<RoomDto> UpdateRoomAsync(UpdateRoomDto roomDto)
        {
            _logger.LogInformation("Updating room with ID: {RoomId}", roomDto.Id);

            var currentRoom = await GetRoomByIdAsync(roomDto.Id);

            var updatedRoom = await _roomService.UpdateRoomAsync(roomDto);

            await CacheRoom(updatedRoom);
            await InvalidateListCaches();

            if (currentRoom != null && currentRoom.RoomTypeId != updatedRoom.RoomTypeId)
            {
                await _cacheService.RemoveAsync(string.Format(ROOMS_BY_ROOMTYPE_KEY, currentRoom.RoomTypeId));
                await _cacheService.RemoveAsync(string.Format(ROOMS_BY_ROOMTYPE_KEY, updatedRoom.RoomTypeId));
            }
            else if (updatedRoom != null)
            {
                await _cacheService.RemoveAsync(string.Format(ROOMS_BY_ROOMTYPE_KEY, updatedRoom.RoomTypeId));
            }

            _logger.LogInformation("Room updated and cache refreshed for ID: {RoomId}", updatedRoom.Id);
            return updatedRoom;
        }

        public async Task DeleteRoomAsync(int id)
        {
            _logger.LogInformation("Deleting room with ID: {RoomId}", id);

            var roomToDelete = await GetRoomByIdAsync(id);

            await _roomService.DeleteRoomAsync(id);

            await _cacheService.RemoveAsync(string.Format(ROOM_BY_ID_KEY, id));
            await InvalidateListCaches();

            if (roomToDelete != null)
            {
                await _cacheService.RemoveAsync(string.Format(ROOMS_BY_ROOMTYPE_KEY, roomToDelete.RoomTypeId));
            }

            _logger.LogInformation("Room deleted and cache invalidated for ID: {RoomId}", id);
        }

        public async Task<RoomDto> GetRoomByIdAsync(int id)
        {
            var cacheKey = string.Format(ROOM_BY_ID_KEY, id);

            var cachedRoom = await _cacheService.GetAsync<RoomDto>(cacheKey);
            if (cachedRoom != null)
            {
                _logger.LogDebug("Room found in cache for ID: {RoomId}", id);
                return cachedRoom;
            }

            _logger.LogDebug("Room not found in cache, fetching from database for ID: {RoomId}", id);

            var room = await _roomService.GetRoomByIdAsync(id);

            if (room != null)
            {
                await CacheRoom(room);
            }

            return room;
        }

        public async Task<IEnumerable<RoomDto>> GetAllRoomsAsync()
        {
            var cacheKey = ALL_ROOMS_KEY;

            var cachedRooms = await _cacheService.GetAsync<IEnumerable<RoomDto>>(cacheKey);
            if (cachedRooms != null)
            {
                _logger.LogDebug("All rooms found in cache");
                return cachedRooms;
            }

            _logger.LogDebug("All rooms not found in cache, fetching from database");

            var rooms = await _roomService.GetAllRoomsAsync();

            await _cacheService.SetAsync(cacheKey, rooms, RoomListExpiration);

            foreach (var room in rooms)
            {
                await CacheRoom(room);
            }

            _logger.LogDebug("All rooms cached, count: {RoomCount}", rooms.Count());
            return rooms;
        }

        // Extension: Get rooms by room type
        public async Task<IEnumerable<RoomDto>> GetRoomsByRoomTypeIdAsync(int roomTypeId)
        {
            var cacheKey = string.Format(ROOMS_BY_ROOMTYPE_KEY, roomTypeId);

            var cachedRooms = await _cacheService.GetAsync<IEnumerable<RoomDto>>(cacheKey);
            if (cachedRooms != null)
            {
                _logger.LogDebug("Rooms found in cache for RoomType: {RoomTypeId}", roomTypeId);
                return cachedRooms;
            }

            _logger.LogDebug("Rooms not found in cache, fetching from database for RoomType: {RoomTypeId}", roomTypeId);

            // You may need to implement this in IRoomService/RoomService if not present
            var rooms = (await _roomService.GetAllRoomsAsync())
                .Where(r => r.RoomTypeId == roomTypeId)
                .ToList();

            await _cacheService.SetAsync(cacheKey, rooms, RoomListExpiration);

            foreach (var room in rooms)
            {
                await CacheRoom(room);
            }

            _logger.LogDebug("Rooms cached for RoomType: {RoomTypeId}, count: {RoomCount}", roomTypeId, rooms.Count());
            return rooms;
        }

        #region Private helper methods

        private async Task CacheRoom(RoomDto room)
        {
            if (room == null) return;
            var cacheKey = string.Format(ROOM_BY_ID_KEY, room.Id);
            await _cacheService.SetAsync(cacheKey, room, RoomCacheExpiration);
        }

        private async Task InvalidateListCaches()
        {
            var tasks = new List<Task>
            {
                _cacheService.RemoveAsync(ALL_ROOMS_KEY),
                _cacheService.RemoveByPrefixAsync(ROOMS_PREFIX)
            };
            await Task.WhenAll(tasks);
            _logger.LogDebug("Room list caches invalidated");
        }

        #endregion
    }
}