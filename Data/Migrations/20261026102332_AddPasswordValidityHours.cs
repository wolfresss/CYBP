using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CYBP.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordValidityHours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PasswordValidityHours",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordValidityHours",
                table: "AspNetUsers");
        }
    }
}
