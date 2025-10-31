using ApiProyecto.Dtos;
using ApiProyecto.Models;
using ApiProyecto.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ApiProyecto.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly IAuthServices _authService; // 🔑 Inyectar el servicio

		public AuthController(IAuthServices authService) // 🔑 Adaptar el constructor
		{
			_authService = authService;
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login(LoginDto request)
		{
			var (jwt, refreshToken) = await _authService.LoginAsync(request.NombreUsuario, request.Contrasena);

			if (jwt == null)
			{
				return BadRequest(new { message = "Credenciales inválidas." });
			}

			// 🔑 Devolver ambos tokens
			return Ok(new
			{
				AccessToken = jwt,
				RefreshToken = refreshToken
			});
		}

		// 🔑 Endpoint de Refresh (P4-04) - ¡Ya lo tienes listo de la respuesta anterior!
		[HttpPost("refresh")]
		public async Task<IActionResult> Refresh(RefreshTokenRequestDto request)
		{
			var (newJwt, newRefreshToken) = await _authService.RefreshAccessTokenAsync(request.RefreshToken);

			if (newJwt == null || newRefreshToken == null)
			{
				return BadRequest(new { message = "Token de refresco inválido o expirado." });
			}

			return Ok(new
			{
				AccessToken = newJwt,
				RefreshToken = newRefreshToken.Token
			});
		}

		// El método de registro debe seguir la misma inyección de servicios si es necesario.
	}
}