using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiProyecto.Migrations
{
    /// <inheritdoc />
    public partial class EliminarStockDeInventario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrecioTotal",
                table: "inventario");

            migrationBuilder.DropColumn(
                name: "Stock",
                table: "inventario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PrecioTotal",
                table: "inventario",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Stock",
                table: "inventario",
                type: "int",
                nullable: true);
        }
    }
}
