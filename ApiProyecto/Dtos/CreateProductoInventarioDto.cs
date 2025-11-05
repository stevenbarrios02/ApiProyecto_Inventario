namespace ApiProyecto.Dtos
{
	public class CreateProductoInventarioDto
	{
		// Datos del Producto
		public string Nombre { get; set; }
		public decimal Precio { get; set; }
		public string Categoria { get; set; }
		public int StockInicial { get; set; } // Cantidad inicial del producto

		// Para ligar el producto a la cabecera del inventario (si tienes una)
		public int InventariosIdInventario { get; set; }
	}
}
