using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASP_32.Migrations
{
    /// <inheritdoc />
    public partial class Quiantity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Carts",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "CartItems",
                newName: "Id");

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "CartItems",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "CartItems");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Carts",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "CartItems",
                newName: "ID");
        }
    }
}
