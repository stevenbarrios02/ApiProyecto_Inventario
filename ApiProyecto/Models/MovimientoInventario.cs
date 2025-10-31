using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ApiProyecto.Models
{
	[Table("MovimientoInventario")]
	public partial class MovimientoInventario
	{
		[Key]
		[Column("idMovimientoInventario")]
		public int IdMovimientoInventario { get; set; }

		public DateTime? FechaMovimiento { get; set; }

		[MaxLength(20)]
		public string TipoMovimiento { get; set; } // Ejemplo: "Salida", "Entrada", "Ajuste"

		public int? Cantidad { get; set; }

		[MaxLength(50)]
		public string Referencia { get; set; } // Ejemplo: "VTA-123", "CMP-456"

		// Clave Foránea a Producto
		public int? ProductosIdProductos { get; set; }
		[ForeignKey("ProductosIdProductos")]
		public virtual Producto ProductosIdProductosNavigation { get; set; }

		// Clave Foránea a Usuario (quién hizo el movimiento)
		public int? UsuariosIdUsuarios { get; set; }
		[ForeignKey("UsuariosIdUsuarios")]
		public virtual Usuario UsuariosIdUsuariosNavigation { get; set; }
	}
}
