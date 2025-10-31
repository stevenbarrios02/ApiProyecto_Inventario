using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiProyecto.Models;

[Table("inventario")]
public partial class Inventario
{
	[Key]
	[Column("idInventario")]
	public int IdInventario { get; set; }

	public DateTime? FechaIngreso { get; set; }

	

	[InverseProperty("Inventario")]
	public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
