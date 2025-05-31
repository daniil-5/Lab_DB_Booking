using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using BookingSystem.Domain.Entities;

namespace BookingSystem.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        protected AppDbContext() {}
        
        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<RoomType> RoomTypes { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<HotelPhoto> HotelPhotos { get; set; }
        public DbSet<RoomPricing> RoomPricings { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure table names and schema
            modelBuilder.Entity<Hotel>().ToTable("hotels");
            modelBuilder.Entity<Room>().ToTable("rooms");
            modelBuilder.Entity<RoomType>().ToTable("room_types");
            modelBuilder.Entity<Booking>()
                .HasIndex("RoomId")
                .HasName("IX_bookings_room_id")
                .IsUnique(false)
                .HasFilter(null)
                .HasAnnotation("Relational:Name", "ix_bookings_room_id");
            modelBuilder.Entity<Booking>().ToTable("bookings");
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<HotelPhoto>().ToTable("hotel_photos");
            modelBuilder.Entity<RoomPricing>().ToTable("room_pricings");

            // Configure relationships with proper delete behavior
            
            // Hotel - RoomType relationship
            modelBuilder.Entity<Hotel>()
                .HasMany(h => h.RoomTypes)
                .WithOne(rt => rt.Hotel)
                .HasForeignKey(rt => rt.HotelId)
                .OnDelete(DeleteBehavior.Cascade);

            // // RoomType - Room relationship
            modelBuilder.Entity<RoomType>()
                .HasMany(rt => rt.Rooms)
                .WithOne(r => r.RoomType)
                .HasForeignKey(r => r.RoomTypeId)
                .OnDelete(DeleteBehavior.Cascade);

            // RoomType - RoomPricing relationship (updated)
            modelBuilder.Entity<RoomType>()
                .HasMany(rt => rt.Pricing)
                .WithOne(rp => rp.RoomType)
                .HasForeignKey(rp => rp.RoomTypeId)
                .OnDelete(DeleteBehavior.Cascade);

            // User - Booking relationship
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Hotel - Booking relationship
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Hotel)
                .WithMany(h => h.Bookings)
                .HasForeignKey(b => b.HotelId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // RoomType - Booking relationship (new)
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.RoomType)
                .WithMany(rt => rt.Bookings)
                .HasForeignKey(b => b.RoomTypeId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Enum value conversion for Booking.Status
            modelBuilder.Entity<Booking>()
                .Property(b => b.Status)
                .HasConversion<int>();
                
            // Configure additional properties
            modelBuilder.Entity<Booking>()
                .Property(b => b.TotalPrice)
                .HasColumnType("decimal(18,2)");
                
            modelBuilder.Entity<RoomPricing>()
                .Property(rp => rp.Price)
                .HasColumnType("decimal(18,2)");
                
            modelBuilder.Entity<RoomType>()
                .Property(rt => rt.BasePrice)
                .HasColumnType("decimal(18,2)");
            
            modelBuilder.Entity<Hotel>()
                .Property(h => h.BasePrice)
                .HasColumnType("decimal(18,2)");
            
             modelBuilder.Entity<Hotel>(entity =>
             {
                 entity.ToTable("hotels");
        
                 // Configure Amenities as a JSON column for PostgreSQL
                 entity.Property(rt => rt.Amenities)
                     .HasColumnType("jsonb")
                     .HasConversion(
                     v => JsonSerializer.Serialize(v, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
                     v => JsonSerializer.Deserialize<List<string>>(v, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }) ?? new List<string>()
                 );
             });
             
            // Configure snake_case naming convention for all entities
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.SetTableName(entity.GetTableName().ToSnakeCase());
                
                foreach (var property in entity.GetProperties())
                {
                    property.SetColumnName(property.Name.ToSnakeCase());
                }

                foreach (var key in entity.GetKeys())
                {
                    key.SetName(key.GetName().ToSnakeCase());
                }

                foreach (var foreignKey in entity.GetForeignKeys())
                {
                    foreignKey.SetConstraintName(foreignKey.GetConstraintName().ToSnakeCase());
                }
            }
        }
    }

    // Extension method for snake_case conversion
    public static class StringExtensions
    {
        public static string ToSnakeCase(this string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            return string.Concat(input.Select((c, i) => 
                i > 0 && char.IsUpper(c) 
                    ? "_" + c.ToString().ToLower() 
                    : c.ToString().ToLower()));
        }
    }
}