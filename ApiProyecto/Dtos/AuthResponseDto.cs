namespace ApiProyecto.Dtos
{
	public class AuthResponseDto
	{
		public string Token { get; set; } // El JWT que el frontend guardará
		public string NombreUsuario { get; set; }
	}
}
