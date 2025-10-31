using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiProyecto.Models;

[Table("contratos")]
[Index("PersonasIdPersonas", Name = "Personas_idPersonas")]
public partial class Contrato : ISoftDelete
{
    [Key]
    [Column("idContratos")]
    public int IdContratos { get; set; }

	[Required]
	[StringLength(45)]
    public decimal? Sueldo { get; set; }

    [Required]
	public DateTime? FechaInicio { get; set; }

	[Required]
	public DateTime? FechaFinal { get; set; }

	[Required]
    [StringLength(45)]
    public string? Cargo { get; set; }

    public bool EstaEliminado { get; set; } = false;

	public DateTime? FechaEliminacion { get; set; }

	[Column("Personas_idPersonas")]
    public int? PersonasIdPersonas { get; set; }

    [ForeignKey("PersonasIdPersonas")]
    [InverseProperty("Contratos")]
    public virtual Persona? PersonasIdPersonasNavigation { get; set; }
}
