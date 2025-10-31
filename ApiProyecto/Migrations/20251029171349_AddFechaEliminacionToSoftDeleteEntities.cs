using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiProyecto.Migrations
{
    /// <inheritdoc />
    public partial class AddFechaEliminacionToSoftDeleteEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            

            migrationBuilder.AddColumn<bool>(
                name: "EstaEliminado",
                table: "usuarios",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEliminacion",
                table: "usuarios",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EstaEliminado",
                table: "productos",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEliminacion",
                table: "productos",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "EstaEliminado",
                table: "personas",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEliminacion",
                table: "personas",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "PrecioUnitario",
                table: "detalleventa",
                type: "double",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AlterColumn<decimal>(
                name: "PrecioTotal",
                table: "detalleventa",
                type: "decimal(10,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<int>(
                name: "Cantidad",
                table: "detalleventa",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "Sueldo",
                table: "contratos",
                type: "decimal(65,30)",
                maxLength: 45,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(45)",
                oldMaxLength: 45)
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.AddColumn<bool>(
                name: "EstaEliminado",
                table: "contratos",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEliminacion",
                table: "contratos",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstaEliminado",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "FechaEliminacion",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "EstaEliminado",
                table: "productos");

            migrationBuilder.DropColumn(
                name: "FechaEliminacion",
                table: "productos");

            migrationBuilder.DropColumn(
                name: "FechaEliminacion",
                table: "personas");

            migrationBuilder.DropColumn(
                name: "EstaEliminado",
                table: "contratos");

            migrationBuilder.DropColumn(
                name: "FechaEliminacion",
                table: "contratos");

            migrationBuilder.AddColumn<int>(
                name: "Usuarios_Personas_idPersonas",
                table: "ventas",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "EstaEliminado",
                table: "personas",
                type: "tinyint(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<double>(
                name: "PrecioUnitario",
                table: "detalleventa",
                type: "double",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "PrecioTotal",
                table: "detalleventa",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Cantidad",
                table: "detalleventa",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Sueldo",
                table: "contratos",
                type: "varchar(45)",
                maxLength: 45,
                nullable: false,
                collation: "utf8mb4_0900_ai_ci",
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)",
                oldMaxLength: 45)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "Usuarios_Personas_idPersonas",
                table: "ventas",
                column: "Usuarios_Personas_idPersonas");
        }
    }
}
