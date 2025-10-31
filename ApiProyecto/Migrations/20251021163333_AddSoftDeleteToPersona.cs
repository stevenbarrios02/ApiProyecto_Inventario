using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiProyecto.Migrations
{
	/// <inheritdoc />
	public partial class AddSoftDeleteToPersona : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			
			migrationBuilder.AddColumn<bool>(
				name: "EstaEliminado",
				table: "personas",
				type: "tinyint(1)",
				nullable: false,
				defaultValue: false); 
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			
			migrationBuilder.DropColumn(
				name: "EstaEliminado",
				table: "personas");

		}
	}
}

