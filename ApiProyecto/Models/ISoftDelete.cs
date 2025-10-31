namespace ApiProyecto.Models
{
	public interface ISoftDelete
	{
		bool EstaEliminado { get; set; }
		DateTime? FechaEliminacion { get; set; } 
	}
}
