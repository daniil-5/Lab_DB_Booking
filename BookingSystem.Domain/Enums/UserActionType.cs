namespace BookingSystem.Domain.Enums;

public enum UserActionType
{
    UserCreated,
    UserUpdated,
    UserDeleted,
    UserLogout,
    ChangePassword,
    
    HotelCreated,
    HotelUpdated,
    HotelDeleted,
    
    BookingCreated,
    BookingUpdated,
    BookingDeleted,
    
    RoomTypesCreated,
    RoomTypesUpdated,
    RoomTypesDeleted,
    
    RoomPricingsCreated,
    RoomPricingsUpdated,
    RoomPricingsDeleted,
    
    Unknown
}