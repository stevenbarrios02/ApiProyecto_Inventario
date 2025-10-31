namespace ApiProyecto.Dtos
{
	public class InventarioDto
	{
		public int IdInventario { get; set; }
		public DateTime? FechaIngreso { get; set; }
		

		public List<ProductoDto>? Productos { get; set; }
	}

	public class CreateInventarioDto
	{
		public DateTime? FechaIngreso { get; set; }
		
		public List<ProductoDto>? Productos { get; set; }
		
	}
}

