using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiProyecto.Models;

[Table("ventas")]
[Index("UsuariosIdUsuarios", Name = "Usuarios_idUsuarios")]
public partial class Venta
{
    [Key]
    [Column("idVentas")]
    public int IdVentas { get; set; }

	[Required]
	public DateTime? Fecha { get; set; }

    [Required]
    public double? Total { get; set; }

    [Column("Usuarios_idUsuarios")]
    public int? UsuariosIdUsuarios { get; set; }

    [InverseProperty("VentasIdVentasNavigation")]
    public virtual ICollection<Detalleventum> Detalleventa { get; set; } = new List<Detalleventum>();

    [ForeignKey("UsuariosIdUsuarios")]
    [InverseProperty("Ventas")]
    public virtual Usuario? UsuariosIdUsuariosNavigation { get; set; }

}
