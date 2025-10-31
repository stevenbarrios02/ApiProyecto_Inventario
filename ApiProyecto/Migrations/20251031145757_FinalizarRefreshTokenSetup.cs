using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiProyecto.Migrations
{
    /// <inheritdoc />
    public partial class FinalizarRefreshTokenSetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshToken_usuarios_UsuariosIdUsuarios",
                table: "RefreshToken");

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshToken_usuarios_UsuariosIdUsuarios",
                table: "RefreshToken",
                column: "UsuariosIdUsuarios",
                principalTable: "usuarios",
                principalColumn: "idUsuarios");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshToken_usuarios_UsuariosIdUsuarios",
                table: "RefreshToken");

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshToken_usuarios_UsuariosIdUsuarios",
                table: "RefreshToken",
                column: "UsuariosIdUsuarios",
                principalTable: "usuarios",
                principalColumn: "idUsuarios",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
