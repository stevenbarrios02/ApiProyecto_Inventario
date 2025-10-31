using System.ComponentModel.DataAnnotations;

namespace ApiProyecto.Dtos
{
	public class PersonaDto
	{
		public int IdPersonas { get; set; }
		[Required(ErrorMessage = "El nombre es obligatorio")]
		public string Nombre { get; set; }
		[Required(ErrorMessage = "El apellido es obligatorio.")]
		public string Apellido { get; set; }
		[Required(ErrorMessage = "La cedula es obligatorio.")]
		public string? Cedula { get; set; }
		public string ?Telefono { get; set; }
		public string? Direccion { get; set; }
		public string? Cargo { get; set; }
		public bool EstaEliminado { get; set; }

	}

	public class CreatePersonaDto
	{
		public string Nombre { get; set; }
		public string Apellido { get; set; }
		public string? Cedula { get; set; }
		public string? Telefono { get; set; }
		public string? Direccion { get; set; }
		public string? Cargo { get; set; }
	}
}

