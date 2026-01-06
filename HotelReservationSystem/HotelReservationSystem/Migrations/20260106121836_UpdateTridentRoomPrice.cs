using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelReservationSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTridentRoomPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Rooms SET BasePrice = 500 WHERE RoomType = 'Deluxe' AND Capacity = 4 AND HotelId IN (SELECT Id FROM Hotels WHERE Name LIKE '%Trident%')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Rooms SET BasePrice = 100 WHERE RoomType = 'Deluxe' AND Capacity = 4 AND HotelId IN (SELECT Id FROM Hotels WHERE Name LIKE '%Trident%')");
        }
    }
}
