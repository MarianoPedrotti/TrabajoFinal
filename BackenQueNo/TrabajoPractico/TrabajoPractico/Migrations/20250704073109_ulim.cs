using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrabajoPractico.Migrations
{
    /// <inheritdoc />
    public partial class ulim : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Transacciones",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Transacciones");
        }
    }
}
