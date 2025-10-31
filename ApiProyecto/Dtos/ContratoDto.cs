namespace ApiProyecto.Dtos
{
	public class ContratoDto
	{
		public int IdContratos { get; set; }
		public decimal? Sueldo { get; set; }
		public DateTime? FechaInicio { get; set; }
		public DateTime? FechaFinal { get; set; }
		public string? Cargo { get; set; }
		public int? PersonasIdPersonas { get; set; }
		public PersonaDto? Persona { get; set; }
	}

	public class CreateContratoDto
	{
		public decimal? Sueldo { get; set; }
		public DateTime? FechaInicio { get; set; }
		public DateTime? FechaFinal { get; set; }
		public string? Cargo { get; set; }
		public int? PersonasIdPersonas { get; set; }
	}
}

