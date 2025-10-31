namespace ApiProyecto.Dtos
{
	public class MovimientoInventarioDto
	{
		public int IdMovimientoInventario { get; set; }
		public DateTime? FechaMovimiento { get; set; }
		public string TipoMovimiento { get; set; }
		public int? Cantidad { get; set; }
		public string Referencia { get; set; }
		public int? ProductosIdProductos { get; set; }
		public int? UsuariosIdUsuarios { get; set; }

		// Propiedades de navegación simplificadas si las necesitas
		// public string NombreProducto { get; set; }
		// public string CorreoUsuario { get; set; }
	}

	// DTO para registrar un movimiento manual (ajuste/entrada)
	public class CreateMovimientoInventarioDto
	{
		public string TipoMovimiento { get; set; }
		public int? Cantidad { get; set; }
		public string Referencia { get; set; }
		public int? ProductosIdProductos { get; set; }
		public int? UsuariosIdUsuarios { get; set; }
	}
}
