using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiProyecto.Models;

[Table("productos")]
[Index("InventarioIdInventario", Name = "Inventario_idInventario")]
public partial class Producto : ISoftDelete
{
	[Key]
	[Column("idProductos")]
	public int IdProductos { get; set; }

	[Required]
	[StringLength(45)]
	public string Nombre { get; set; } = null!;

	[Required] 
	public double Precio { get; set; }

	[StringLength(45)]
	public string? Categoria { get; set; }
	public int? Stock { get; set; }

	public bool EstaEliminado { get; set; } = false;

	public DateTime? FechaEliminacion { get; set; }

	[Column("Inventario_idInventario")]
	public int? InventarioIdInventario { get; set; }


	[InverseProperty("ProductosIdProductosNavigation")]
	public virtual ICollection<Detalleventum> Detalleventa { get; set; } = new List<Detalleventum>();

	[ForeignKey(nameof(InventarioIdInventario))]
	[InverseProperty("Productos")]
	public virtual Inventario? Inventario { get; set; }
}
