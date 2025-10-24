SELECT
    h.name AS hotel_name,
    h.location,
    COUNT(b.id) AS total_bookings,
    COUNT(CASE WHEN b.status = 1 THEN 1 END) AS confirmed_bookings,
    COUNT(CASE WHEN b.status = 2 THEN 1 END) AS cancelled_bookings,
    SUM(b.total_price) AS total_revenue,
    AVG(b.total_price) AS average_booking_price,
    AVG(b.guest_count) AS average_guests,
    MIN(b.check_in_date) AS earliest_checkin,
    MAX(b.check_out_date) AS latest_checkout
FROM hotels h
         INNER JOIN bookings b ON h.id = b.hotel_id
WHERE h.is_deleted = FALSE AND b.is_deleted = FALSE
GROUP BY h.id, h.name, h.location
ORDER BY total_revenue DESC;


SELECT
    b.id AS BookingId,
    b.check_in_date AS CheckInDate,
    b.check_out_date AS CheckOutDate,
    b.status AS Status,
    b.total_price AS TotalPrice,
    h.name AS HotelName,
    h.location AS Location,
    rt.name AS RoomTypeName
FROM bookings b
         INNER JOIN hotels h ON b.hotel_id = h.id
         INNER JOIN room_types rt ON b.room_type_id = rt.id
WHERE b.user_id = @userId AND b.is_deleted = false
ORDER BY b.created_at DESC
LIMIT 10;

SELECT
    u.id AS UserId,
    u.username AS Username,
    u.email AS Email,
    u.first_name || ' ' || u.last_name AS FullName,
    COUNT(b.id) AS TotalBookings,
    COUNT(CASE WHEN b.status = 1 THEN 1 END) AS CompletedBookings,
    COUNT(CASE WHEN b.status = 2 THEN 1 END) AS CancelledBookings,
    COALESCE(SUM(b.total_price), 0) AS TotalSpent,
    COALESCE(AVG(b.total_price), 0) AS AverageBookingValue,
    MIN(b.created_at) AS FirstBookingDate,
    MAX(b.created_at) AS LastBookingDate,
    COUNT(DISTINCT b.hotel_id) AS UniqueHotelsVisited
FROM users u
         LEFT JOIN bookings b ON u.id = b.user_id AND b.is_deleted = false
WHERE u.id = @userId AND u.is_deleted = false
GROUP BY u.id, u.username, u.email, u.first_name, u.last_name;


CREATE TABLE user_action_audit
(
    id               serial primary key,
    user_id integer not null,
    user_action_type TEXT not null default 'Unknown',
    is_success       boolean not null,
    created_at       timestamp with time zone not null default current_timestamp,
    updated_at      timestamp with time zone,
    is_deleted       boolean not null default false,
    foreign key (user_id) references users(id)
);

-- Статистика по отелям
SELECT
    h.id AS HotelId,
    h.name AS HotelName,
    h.location AS Location,
    h.rating AS Rating,
    h.base_price AS BasePrice,
    COUNT(DISTINCT b.id) AS TotalBookings,
    COUNT(DISTINCT CASE WHEN b.status = 1 THEN b.id END) AS ConfirmedBookings,
    COUNT(DISTINCT CASE WHEN b.status = 2 THEN b.id END) AS CancelledBookings,
    COALESCE(SUM(b.total_price), 0) AS TotalRevenue,
    COALESCE(AVG(b.total_price), 0) AS AverageBookingPrice,
    COUNT(DISTINCT rt.id) AS TotalRoomTypes,
    COUNT(DISTINCT hp.id) AS TotalPhotos
FROM hotels h
         LEFT JOIN room_types rt ON h.id = rt.hotel_id AND rt.is_deleted = false
         LEFT JOIN bookings b ON h.id = b.hotel_id AND b.is_deleted = false
         LEFT JOIN hotel_photos hp ON h.id = hp.hotel_id AND hp.is_deleted = false
WHERE h.is_deleted = false
GROUP BY h.id, h.name, h.location, h.rating, h.base_price
ORDER BY TotalRevenue DESC;

-- Статистика бронирований по отелям
SELECT
    h.name AS hotel_name,
    h.location,
    COUNT(b.id) AS total_bookings,
    COUNT(CASE WHEN b.status = 1 THEN 1 END) AS confirmed_bookings,
    COUNT(CASE WHEN b.status = 2 THEN 1 END) AS cancelled_bookings,
    SUM(b.total_price) AS total_revenue,
    AVG(b.total_price) AS average_booking_price,
    AVG(b.guest_count) AS average_guests,
    MIN(b.check_in_date) AS earliest_checkin,
    MAX(b.check_out_date) AS latest_checkout
FROM hotels h
         INNER JOIN bookings b ON h.id = b.hotel_id
WHERE h.is_deleted = FALSE AND b.is_deleted = FALSE
GROUP BY h.id, h.name, h.location
ORDER BY total_revenue DESC;

