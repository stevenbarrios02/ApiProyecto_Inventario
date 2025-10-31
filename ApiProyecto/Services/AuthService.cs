using ApiProyecto.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ApiProyecto.Services
{
	public class AuthService : IAuthServices
	{
		private readonly EmpresaDbContext _context;
		private readonly IConfiguration _configuration;

		public AuthService(EmpresaDbContext context, IConfiguration configuration)
		{
			_context = context;
			_configuration = configuration;
		}

		// 🔑 Lógica de generación de JWT (MUÉVELA AQUÍ)
		public string GenerateJwtToken(Usuario usuario)
		{
			var jwtSettings = _configuration.GetSection("Jwt");
			var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
			var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

			// Claims: Información pública del usuario que viaja en el token
			var claims = new[]
			{
			new Claim(JwtRegisteredClaimNames.Sub, usuario.Correo),
			new Claim("IdUsuario", usuario.IdUsuarios.ToString()),
			new Claim(ClaimTypes.Role, usuario.RolesIdRolesNavigation?.Nombre ?? "Invitado")
		};

			var token = new JwtSecurityToken(
				issuer: jwtSettings["Issuer"],
				audience: jwtSettings["Audience"],
				claims: claims,
				expires: DateTime.Now.AddDays(7), // El token expira en 7 días
				signingCredentials: credentials);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}

		// 🔑 Lógica de Login modificada para incluir Refresh Token
		public async Task<(string? Jwt, string? RefreshToken)> LoginAsync(string email, string password)
		{
			using var transaction = await _context.Database.BeginTransactionAsync();
			// 1. **PEGA AQUÍ TU LÓGICA EXISTENTE DE VALIDACIÓN DE LOGIN**
			var usuario = await _context.Usuarios
				.Include(u => u.RolesIdRolesNavigation)
				.Include(u => u.Persona)
				.AsNoTracking()// Para el Soft Delete
				.FirstOrDefaultAsync(u => u.Correo == email);
			try
			{
				if (usuario == null || usuario.Persona.EstaEliminado == true)
					return (null, null);

				

				int userId = usuario.IdUsuarios;

				// 3. Generar Tokens
				var jwt = GenerateJwtToken(usuario);
				var refreshToken = GenerateRefreshToken();

				// 4. Guardar Refresh Token
				await SaveRefreshTokenWithoutSave(userId, refreshToken);

				await _context.SaveChangesAsync();
				await transaction.CommitAsync();

				return (jwt, refreshToken.Token);
			}
			catch (Exception)
			{
				await transaction.RollbackAsync();
				throw;
			}

		}

		// 🔑 Lógica de Refresh Tokens (Implementación P4-04)
		public RefreshToken GenerateRefreshToken()
		{
			var nowUtc = DateTime.UtcNow;
			// *** PEGAR LÓGICA DE GENERACIÓN DE TOKEN (Igual que en la respuesta anterior) ***
			// Implementación para ahorrar espacio:
			var refreshToken = new RefreshToken
			{
				Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
				Expires = nowUtc.AddDays(7), // 🔑 Fecha de expiración en UTC
				Created = nowUtc,
			};
			return refreshToken;
		}

		public async Task SaveRefreshToken(int userId, RefreshToken newRefreshToken)
		{
			// Implementación (Igual que en la respuesta anterior)
			var tokensViejos = _context.RefreshTokens
				.Where(t => t.UsuariosIdUsuarios == userId && t.Expires < DateTime.Now);
			_context.RefreshTokens.RemoveRange(tokensViejos);

			newRefreshToken.UsuariosIdUsuarios = userId;
			_context.RefreshTokens.Add(newRefreshToken);

			await _context.SaveChangesAsync();
		}
		private async Task SaveRefreshTokenWithoutSave(int userId, RefreshToken newRefreshToken)
		{
			// Eliminar tokens viejos sin guardar
			var tokensViejos = _context.RefreshTokens
				.Where(t => t.UsuariosIdUsuarios == userId && t.Expires < DateTime.Now);
			_context.RefreshTokens.RemoveRange(tokensViejos);

			newRefreshToken.UsuariosIdUsuarios = userId;
			_context.RefreshTokens.Add(newRefreshToken);
			// 🛑 NO LLAMAR A SaveChangesAsync() AQUÍ
		}
		public async Task<(string? Jwt, RefreshToken? RefreshToken)> RefreshAccessTokenAsync(string oldRefreshToken)
		{
			// 1. Buscamos y validamos el token
			var tokenGuardado = await _context.RefreshTokens
				.FirstOrDefaultAsync(t => t.Token == oldRefreshToken && t.IsRevoked == false);

			if (tokenGuardado == null || tokenGuardado.Expires < DateTime.Now)
			{
				return (null, null); // Inválido, expirado o revocado
			}

			// 2. Iniciar la Transacción (Aseguramos atomicidad)
			using var transaction = await _context.Database.BeginTransactionAsync();

			try
			{
				// 3. Revocar el token viejo (actualización)
				tokenGuardado.IsRevoked = true;
				_context.RefreshTokens.Update(tokenGuardado); // Marcamos el cambio

				// 4. Buscar el usuario
				var usuario = await _context.Usuarios
					.Include(u => u.RolesIdRolesNavigation)
					.FirstOrDefaultAsync(u => u.IdUsuarios == tokenGuardado.UsuariosIdUsuarios);

				if (usuario == null) throw new Exception("Usuario no encontrado para el token.");

				// 5. Generar nuevos tokens
				var newJwt = GenerateJwtToken(usuario);
				var newRefreshToken = GenerateRefreshToken();

				// 6. Guardar el nuevo Refresh Token
				// 🔑 IMPORTANTE: MODIFICAMOS SaveRefreshToken para que NO llame a SaveChangesAsync
				await SaveRefreshTokenWithoutSave(usuario.IdUsuarios, newRefreshToken);

				// 7. Guardar todos los cambios (el token revocado + el nuevo token)
				await _context.SaveChangesAsync();

				// 8. Commit de la transacción
				await transaction.CommitAsync();

				return (newJwt, newRefreshToken);
			}
			catch (Exception)
			{
				await transaction.RollbackAsync();
				throw; // Deja que el filtro P1-06 capture el 500
			}
		}

	}
}
