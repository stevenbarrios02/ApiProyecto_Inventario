using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiProyecto.Models;

[Table("personas")]
[Index("Cedula", Name = "Cedula", IsUnique = true)]
public partial class Persona : ISoftDelete
{
	[Key]
	[Column("idPersonas")]
	public int IdPersonas { get; set; }

	[Required]
	[StringLength(45)]
	public string Nombre { get; set; } = null!;

	[Required]
	[StringLength(45)]
	public string Apellido { get; set; } = null!;

	[StringLength(45)]
	public string? Cedula { get; set; }

	[StringLength(45)]
	public string? Telefono { get; set; }

	[StringLength(45)]
	public string? Direccion { get; set; }

	[StringLength(45)]
	public string? Cargo { get; set; }

	[Required]
	[Column("EstaEliminado")]
	public bool EstaEliminado { get; set; } = false;

	public DateTime? FechaEliminacion { get; set; }

	[InverseProperty("Persona")] 
	public virtual Usuario? Usuario { get; set; }

	[InverseProperty("PersonasIdPersonasNavigation")]
	public virtual ICollection<Contrato> Contratos { get; set; } = new List<Contrato>();
}
