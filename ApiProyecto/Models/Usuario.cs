using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiProyecto.Models;

[Table("usuarios")]
[Index("Correo", Name = "Correo", IsUnique = true)]
[Index("RolesIdRoles", Name = "Roles_idRoles")]
[Index("PersonasIdPersonas", Name = "fk_Usuarios_Personas")]
public partial class Usuario : ISoftDelete
{
	[Key]
	[Column("idUsuarios")]
	public int IdUsuarios { get; set; }

	[Required]
	[StringLength(45)]
	public string Correo { get; set; } = null!;

	[Required]
	[StringLength(45)]
	public string Contrasenia { get; set; } = null!;

	public bool EstaEliminado { get; set; } = false;

	public DateTime? FechaEliminacion { get; set; }
	[Required]
	[Column("Personas_idPersonas")]
	public int PersonasIdPersonas { get; set; }

	[Column("Roles_idRoles")]
	public int? RolesIdRoles { get; set; }

	[ForeignKey(nameof(PersonasIdPersonas))]
	[InverseProperty("Usuario")] 
	public virtual Persona Persona { get; set; } = null!;

	[ForeignKey(nameof(RolesIdRoles))]
	[InverseProperty("Usuarios")]
	public virtual Role? RolesIdRolesNavigation { get; set; }

	[InverseProperty("UsuariosIdUsuariosNavigation")]
	public virtual ICollection<Venta> Ventas { get; set; } = new List<Venta>();
	public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
