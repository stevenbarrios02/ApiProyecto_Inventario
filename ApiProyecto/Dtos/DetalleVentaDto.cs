namespace ApiProyecto.Dtos
{
	public class DetalleVentaDto
	{
		public int IdDetalleVenta { get; set; }
		public int? Cantidad { get; set; }
		public double? PrecioUnitario { get; set; }
		public decimal? PrecioTotal { get; set; }
		public int? VentasIdVentas { get; set; }
		public int? ProductosIdProductos { get; set; }

		public VentaDto Venta { get; set; }
		public ProductoDto Producto { get; set; }
	}

	public class CreateDetalleVentaDto
	{
		public int? Cantidad { get; set; }
		public double? PrecioUnitario { get; set; }
		public int? VentasIdVentas { get; set; }
		public int? ProductosIdProductos { get; set; }
	}
}

