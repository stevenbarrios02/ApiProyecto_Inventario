using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiProyecto.Models;

[Table("detalleventa")]
[Index("ProductosIdProductos", Name = "Productos_idProductos")]
[Index("VentasIdVentas", Name = "Ventas_idVentas")]
public partial class Detalleventum
{
	[Key]
	[Column("idDetalleVenta")]
	public int IdDetalleVenta { get; set; }

	public int? Cantidad { get; set; }

	public double? PrecioUnitario { get; set; }

	[Column(TypeName = "decimal(10, 2)")]
	public decimal? PrecioTotal { get; set; }

	[Column("Ventas_idVentas")]
	public int? VentasIdVentas { get; set; }

	[Column("Productos_idProductos")]
	public int? ProductosIdProductos { get; set; }

	[ForeignKey(nameof(ProductosIdProductos))]
	[InverseProperty("Detalleventa")]
	public virtual Producto? ProductosIdProductosNavigation { get; set; }

	[ForeignKey(nameof(VentasIdVentas))]
	[InverseProperty("Detalleventa")]
	public virtual Venta? VentasIdVentasNavigation { get; set; }
}
