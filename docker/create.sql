-- Создание таблицы ролей
CREATE TABLE role (
                      id SERIAL PRIMARY KEY,
                      role_name TEXT NOT NULL
);

-- Создание таблицы пользователей
CREATE TABLE users (
                       id SERIAL PRIMARY KEY,
                       username TEXT NOT NULL UNIQUE,
                       email TEXT NOT NULL UNIQUE,
                       password_hash TEXT NOT NULL,
                       first_name TEXT,
                       last_name TEXT,
                       phone_number TEXT,
                       role_id INTEGER,
                       created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                       updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                       is_deleted BOOLEAN DEFAULT FALSE,
                       CONSTRAINT fk_user_role FOREIGN KEY (role_id) REFERENCES role(id) ON DELETE SET NULL
);

-- Создание таблицы отелей
CREATE TABLE hotels (
                        id SERIAL PRIMARY KEY,
                        name TEXT NOT NULL,
                        description TEXT,
                        location TEXT,
                        rating NUMERIC(3, 2) CHECK (rating >= 0 AND rating <= 5),
                        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                        updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                        is_deleted BOOLEAN DEFAULT FALSE,
                        base_price NUMERIC(10, 2),
);

-- Создание таблицы типов комнат
CREATE TABLE room_types (
                            id SERIAL PRIMARY KEY,
                            name TEXT NOT NULL,
                            description TEXT,
                            capacity INTEGER CHECK (capacity > 0),
                            area NUMERIC(6, 2),
                            floor INTEGER,
                            hotel_id INTEGER NOT NULL,
                            created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                            updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                            is_deleted BOOLEAN DEFAULT FALSE,
                            base_price NUMERIC(10, 2),
                            CONSTRAINT fk_room_type_hotel FOREIGN KEY (hotel_id) REFERENCES hotels(id) ON DELETE CASCADE
);

-- Создание таблицы цен на комнаты
CREATE TABLE room_pricings (
                               id SERIAL PRIMARY KEY,
                               date TIMESTAMP NOT NULL,
                               price NUMERIC(10, 2) NOT NULL CHECK (price >= 0),
                               created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                               updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                               is_deleted BOOLEAN DEFAULT FALSE,
                               room_type_id INTEGER NOT NULL,
                               CONSTRAINT fk_room_pricing_room_type FOREIGN KEY (room_type_id) REFERENCES room_types(id) ON DELETE CASCADE,
                               CONSTRAINT unique_room_pricing_date UNIQUE (room_type_id, date)
);

-- Создание таблицы фотографий отелей
CREATE TABLE hotel_photos (
                              id SERIAL PRIMARY KEY,
                              hotel_id INTEGER NOT NULL,
                              url TEXT NOT NULL,
                              description TEXT,
                              is_main BOOLEAN DEFAULT FALSE,
                              created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                              updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                              is_deleted BOOLEAN DEFAULT FALSE,
                              public_id TEXT,
                              CONSTRAINT fk_hotel_photo_hotel FOREIGN KEY (hotel_id) REFERENCES hotels(id) ON DELETE CASCADE
);

-- Создание таблицы бронирований
CREATE TABLE bookings (
                          id SERIAL PRIMARY KEY,
                          user_id INTEGER NOT NULL,
                          check_in_date TIMESTAMP NOT NULL,
                          check_out_date TIMESTAMP NOT NULL,
                          guest_count INTEGER CHECK (guest_count > 0),
                          total_price NUMERIC(10, 2) NOT NULL CHECK (total_price >= 0),
                          status INTEGER NOT NULL,
                          created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                          updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                          is_deleted BOOLEAN DEFAULT FALSE,
                          hotel_id INTEGER NOT NULL,
                          room_type_id INTEGER NOT NULL,
                          CONSTRAINT fk_booking_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
                          CONSTRAINT fk_booking_hotel FOREIGN KEY (hotel_id) REFERENCES hotels(id) ON DELETE CASCADE,
                          CONSTRAINT fk_booking_room_type FOREIGN KEY (room_type_id) REFERENCES room_types(id) ON DELETE CASCADE,
                          CONSTRAINT check_dates CHECK (check_out_date > check_in_date)
);

-- Создание таблицы аудита действий пользователей
CREATE TABLE user_action_audit (
                                   id SERIAL PRIMARY KEY,
                                   user_id INTEGER NOT NULL,
                                   user_action_type TEXT NOT NULL,
                                   is_success BOOLEAN DEFAULT FALSE,
                                   created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                                   updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                                   is_deleted BOOLEAN DEFAULT FALSE,
                                   CONSTRAINT fk_user_action_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

-- Создание индексов для улучшения производительности
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_username ON users(username);
CREATE INDEX idx_users_role_id ON users(role_id);
CREATE INDEX idx_room_types_hotel_id ON room_types(hotel_id);
CREATE INDEX idx_room_pricings_room_type_id ON room_pricings(room_type_id);
CREATE INDEX idx_room_pricings_date ON room_pricings(date);
CREATE INDEX idx_hotel_photos_hotel_id ON hotel_photos(hotel_id);
CREATE INDEX idx_bookings_user_id ON bookings(user_id);
CREATE INDEX idx_bookings_hotel_id ON bookings(hotel_id);
CREATE INDEX idx_bookings_room_type_id ON bookings(room_type_id);
CREATE INDEX idx_bookings_dates ON bookings(check_in_date, check_out_date);
CREATE INDEX idx_user_action_audit_user_id ON user_action_audit(user_id);

