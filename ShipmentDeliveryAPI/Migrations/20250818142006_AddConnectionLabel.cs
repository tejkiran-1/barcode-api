using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShipmentDeliveryAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddConnectionLabel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConnectionLabel",
                table: "ContainerItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ConnectionLabel",
                table: "BulkItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConnectionLabel",
                table: "ContainerItems");

            migrationBuilder.DropColumn(
                name: "ConnectionLabel",
                table: "BulkItems");
        }
    }
}
