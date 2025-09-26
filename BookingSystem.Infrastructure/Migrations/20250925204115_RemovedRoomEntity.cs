using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BookingSystem.Migrations
{
    /// <inheritdoc />
    public partial class RemovedRoomEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropForeignKey(
            //     name: "f_k_bookings_rooms_room_id",
            //     table: "bookings");

            // migrationBuilder.DropForeignKey(
            //     name: "f_k_room_pricings_rooms_room_id",
            //     table: "room_pricings");

            // migrationBuilder.DropTable(
            //     name: "rooms");

            // migrationBuilder.DropIndex(
            //     name: "IX_room_pricings_room_id",
            //     table: "room_pricings");

            // migrationBuilder.DropIndex(
            //     name: "ix_bookings_room_id",
            //     table: "bookings");

            // migrationBuilder.DropColumn(
            //     name: "room_id",
            //     table: "room_pricings");

            // migrationBuilder.DropColumn(
            //     name: "room_id",
            //     table: "bookings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "room_id",
                table: "room_pricings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "room_id",
                table: "bookings",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "rooms",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    room_type_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_available = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    room_number = table.Column<string>(type: "text", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_rooms", x => x.id);
                    table.ForeignKey(
                        name: "f_k_rooms_room_types_room_type_id",
                        column: x => x.room_type_id,
                        principalTable: "room_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_room_pricings_room_id",
                table: "room_pricings",
                column: "room_id");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_room_id",
                table: "bookings",
                column: "room_id");

            migrationBuilder.CreateIndex(
                name: "IX_rooms_room_type_id",
                table: "rooms",
                column: "room_type_id");

            migrationBuilder.AddForeignKey(
                name: "f_k_bookings_rooms_room_id",
                table: "bookings",
                column: "room_id",
                principalTable: "rooms",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "f_k_room_pricings_rooms_room_id",
                table: "room_pricings",
                column: "room_id",
                principalTable: "rooms",
                principalColumn: "id");
        }
    }
}
