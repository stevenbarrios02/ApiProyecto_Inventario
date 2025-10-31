using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiProyecto.Migrations
{
    /// <inheritdoc />
    public partial class SincronizacionFinalCompleta : Migration
    {
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{

			

			migrationBuilder.AlterColumn<int>(
				name: "Personas_idPersonas",
				table: "usuarios",
				type: "int",
				nullable: false,
				defaultValue: 0,
				oldClrType: typeof(int),
				oldType: "int",
				oldNullable: true);

			migrationBuilder.AlterColumn<int>(
				name: "Stock",
				table: "inventario",
				type: "int",
				nullable: true,
				oldClrType: typeof(string),
				oldType: "varchar(45)",
				oldMaxLength: 45)
				.OldAnnotation("MySql:CharSet", "utf8mb4")
				.OldAnnotation("Relational:Collation", "utf8mb4_0900_ai_ci");

			migrationBuilder.AlterColumn<decimal>(
				name: "PrecioTotal",
				table: "inventario",
				type: "decimal(10,2)",
				nullable: true,
				oldClrType: typeof(string),
				oldType: "varchar(45)",
				oldMaxLength: 45)
				.OldAnnotation("MySql:CharSet", "utf8mb4")
				.OldAnnotation("Relational:Collation", "utf8mb4_0900_ai_ci");

			migrationBuilder.CreateIndex(
				name: "IX_usuarios_Personas_idPersonas",
				table: "usuarios",
				column: "Personas_idPersonas",
				unique: true);

			migrationBuilder.AddForeignKey(
				name: "fk_Usuarios_Personas",
				table: "usuarios",
				column: "Personas_idPersonas",
				principalTable: "personas",
				principalColumn: "idPersonas",
				onDelete: ReferentialAction.Cascade);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_Usuarios_Personas",
                table: "usuarios");

            migrationBuilder.DropIndex(
                name: "IX_usuarios_Personas_idPersonas",
                table: "usuarios");

            migrationBuilder.RenameIndex(
                name: "Usuarios_idUsuarios",
                table: "ventas",
                newName: "Usuarios_idUsuarios1");

            migrationBuilder.AlterColumn<int>(
                name: "Personas_idPersonas",
                table: "usuarios",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.UpdateData(
                table: "productos",
                keyColumn: "Categoria",
                keyValue: null,
                column: "Categoria",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Categoria",
                table: "productos",
                type: "varchar(45)",
                maxLength: 45,
                nullable: false,
                collation: "utf8mb4_0900_ai_ci",
                oldClrType: typeof(string),
                oldType: "varchar(45)",
                oldMaxLength: 45,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.AddColumn<int>(
                name: "Usuarios_Personas_idPersonas",
                table: "personas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Usuarios_idUsuarios",
                table: "personas",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "inventario",
                keyColumn: "Stock",
                keyValue: null,
                column: "Stock",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Stock",
                table: "inventario",
                type: "varchar(45)",
                maxLength: 45,
                nullable: false,
                collation: "utf8mb4_0900_ai_ci",
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "inventario",
                keyColumn: "PrecioTotal",
                keyValue: null,
                column: "PrecioTotal",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "PrecioTotal",
                table: "inventario",
                type: "varchar(45)",
                maxLength: 45,
                nullable: false,
                collation: "utf8mb4_0900_ai_ci",
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaIngreso",
                table: "inventario",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "Usuarios_idUsuarios",
                table: "personas",
                column: "Usuarios_idUsuarios");

            migrationBuilder.AddForeignKey(
                name: "personas_ibfk_1",
                table: "personas",
                column: "Usuarios_idUsuarios",
                principalTable: "usuarios",
                principalColumn: "idUsuarios");

            migrationBuilder.AddForeignKey(
                name: "fk_Usuarios_Personas",
                table: "usuarios",
                column: "Personas_idPersonas",
                principalTable: "personas",
                principalColumn: "idPersonas");

            migrationBuilder.AddForeignKey(
                name: "ventas_ibfk_2",
                table: "ventas",
                column: "Usuarios_Personas_idPersonas",
                principalTable: "personas",
                principalColumn: "idPersonas");
        }
    }
}
