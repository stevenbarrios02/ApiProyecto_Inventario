using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiProyecto.Models;

[Table("roles")]
public partial class Role
{
	[Key]
	[Column("idRoles")]
	public int IdRoles { get; set; }

	[Required]
	[StringLength(45)]
	public string Nombre { get; set; } = null!;

	[InverseProperty("RolesIdRolesNavigation")]
	public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
