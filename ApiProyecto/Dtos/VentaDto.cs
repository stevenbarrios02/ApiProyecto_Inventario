namespace ApiProyecto.Dtos
{
	public class VentaDto
	{
		public int IdVentas { get; set; }
		public DateTime? Fecha { get; set; }
		public double? Total { get; set; }
		public int? UsuariosIdUsuarios { get; set; }
		public UsuarioDto? Usuario { get; set; }
	}

	public class CreateVentaDto
	{
		public DateTime? Fecha { get; set; }
		public double? Total { get; set; }
		public int? UsuariosIdUsuarios { get; set; }
		public List<CreateDetalleVentaDto> Detalles { get; set; }
	}
}

