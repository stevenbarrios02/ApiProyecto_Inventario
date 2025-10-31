using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ApiProyecto.Models
{
	[Table("RefreshToken")]
	public class RefreshToken
	{
		[Key]
		public int Id { get; set; }

		[Required]
		[MaxLength(100)]
		public string Token { get; set; } // El token de refresco en sí

		public DateTime Expires { get; set; } // Fecha de caducidad del token de refresco

		public DateTime Created { get; set; } = DateTime.Now;

		public bool IsRevoked { get; set; } = false;

		// Clave Foránea al Usuario
		public int UsuariosIdUsuarios { get; set; }

		[ForeignKey("UsuariosIdUsuarios")]
		public virtual Usuario UsuariosIdUsuariosNavigation { get; set; }
	}
}
