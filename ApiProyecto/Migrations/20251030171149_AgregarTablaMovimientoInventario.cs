using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiProyecto.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTablaMovimientoInventario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MovimientoInventario",
                columns: table => new
                {
                    idMovimientoInventario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FechaMovimiento = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    TipoMovimiento = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Cantidad = table.Column<int>(type: "int", nullable: true),
                    Referencia = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProductosIdProductos = table.Column<int>(type: "int", nullable: true),
                    UsuariosIdUsuarios = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientoInventario", x => x.idMovimientoInventario);
                    table.ForeignKey(
                        name: "FK_MovimientoInventario_productos_ProductosIdProductos",
                        column: x => x.ProductosIdProductos,
                        principalTable: "productos",
                        principalColumn: "idProductos");
                    table.ForeignKey(
                        name: "FK_MovimientoInventario_usuarios_UsuariosIdUsuarios",
                        column: x => x.UsuariosIdUsuarios,
                        principalTable: "usuarios",
                        principalColumn: "idUsuarios");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientoInventario_ProductosIdProductos",
                table: "MovimientoInventario",
                column: "ProductosIdProductos");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientoInventario_UsuariosIdUsuarios",
                table: "MovimientoInventario",
                column: "UsuariosIdUsuarios");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovimientoInventario");
        }
    }
}
