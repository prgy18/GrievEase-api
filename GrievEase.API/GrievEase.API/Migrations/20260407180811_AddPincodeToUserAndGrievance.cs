using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrievEase.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPincodeToUserAndGrievance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Pincode",
                table: "Users",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Pincode",
                table: "Grievances",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Pincode",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Pincode",
                table: "Grievances");
        }
    }
}
