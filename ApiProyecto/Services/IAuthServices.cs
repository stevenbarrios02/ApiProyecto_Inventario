using ApiProyecto.Models;

namespace ApiProyecto.Services
{
	public interface IAuthServices
	{
		// Método que genera el JWT token
		string GenerateJwtToken(Usuario usuario);

		// Método de Login que ahora devuelve el par JWT + Refresh Token
		Task<(string? Jwt, string? RefreshToken)> LoginAsync(string email, string password);

		// 🔑 Métodos para Tokens de Refresco (P4-04)
		RefreshToken GenerateRefreshToken();
		Task SaveRefreshToken(int userId, RefreshToken refreshToken);
		Task<(string? Jwt, RefreshToken? RefreshToken)> RefreshAccessTokenAsync(string oldRefreshToken);


	}

}
