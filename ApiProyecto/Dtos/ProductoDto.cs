using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ApiProyecto.Dtos
{
	public class ProductoDto
	{
		public int IdProductos { get; set; }
		[Required(ErrorMessage = "El nombre es obligatorio")]
		public string Nombre { get; set; } = null!;
		[Required(ErrorMessage = "El precio es obligatorio")]
		public double Precio { get; set; }
		[Required(ErrorMessage = "La categoria es obligatoria")]
		public string Categoria { get; set; } = null!;
		public int? Stock { get; set; }
		[JsonIgnore]
		public int? InventarioIdInventario { get; set; }
	}

	public class CreateProductoDto
	{
		public string Nombre { get; set; }
		public double Precio { get; set; }
		public string Categoria { get; set; }
		public int? Stock { get; set; }
		public int? InventarioIdInventario { get; set; }
	}
}

