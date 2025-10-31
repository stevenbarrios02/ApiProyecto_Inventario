using System.ComponentModel.DataAnnotations;

namespace ApiProyecto.Dtos
{
	public class UsuarioDto
	{
		public int IdUsuarios { get; set; }
		public string Correo { get; set; } = null!;
		public int PersonasIdPersonas { get; set; }
		public int? RolesIdRoles { get; set; }
		public PersonaDto? Persona { get; set; }
		public RolDto? Rol { get; set; }
	}

	public class CreateUsuarioDto
	{
		public string Correo { get; set; } = null!;

		public string Contrasenia { get; set; } = null!;
		[Required]
		public int PersonasIdPersonas { get; set; }

		public int? RolesIdRoles { get; set; }
	}
}

