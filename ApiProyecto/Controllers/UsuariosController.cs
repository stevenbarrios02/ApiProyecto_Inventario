using ApiProyecto.Dtos;
using ApiProyecto.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiProyecto.Controllers
{

	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class UsuariosController : ControllerBase
	{
		private readonly EmpresaDbContext _context;
		private readonly IMapper _mapper; // 🔑 2. Declarar IMapper

		// 🔑 3. Inyectar IMapper en el constructor
		public UsuariosController(EmpresaDbContext context, IMapper mapper)
		{
			_context = context;
			_mapper = mapper;
		}

		private bool UsuarioExists(int id)
		{
			return _context.Usuarios.Any(e => e.IdUsuarios == id);
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<UsuarioDto>>> GetUsuarios()
		{
			try
			{
				// 🔑 4. Obtener la entidad completa con las inclusiones
				var usuarios = await _context.Usuarios
					.Include(u => u.Persona)
					.Include(u => u.RolesIdRolesNavigation)
					.ToListAsync(); // No necesitamos Select() aquí

				// 🔑 5. Usar AutoMapper para mapear List<Usuario> a List<UsuarioDto>
				return Ok(_mapper.Map<List<UsuarioDto>>(usuarios));
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { status = 500, message = "Error al obtener usuarios.", detail = ex.Message });
			}
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<UsuarioDto>> GetUsuario(int id)
		{
			if (id <= 0)
				return BadRequest(new { status = 400, message = "El ID debe ser mayor a cero." });

			try
			{
				// 🔑 6. Obtener la entidad completa con las inclusiones
				var usuario = await _context.Usuarios
					.Include(u => u.Persona)
					.Include(u => u.RolesIdRolesNavigation)
					.FirstOrDefaultAsync(u => u.IdUsuarios == id); // Usamos FirstOrDefault

				if (usuario == null)
					return NotFound(new { status = 404, message = $"No se encontró ningún usuario con el ID {id}." });

				// 🔑 7. Usar AutoMapper para mapear Usuario a UsuarioDto
				return Ok(_mapper.Map<UsuarioDto>(usuario));
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { status = 500, message = "Error al buscar el usuario.", detail = ex.Message });
			}
		}


		[HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<UsuarioDto>> PostUsuario(CreateUsuarioDto dto)
		{
			// 1. Validaciones básicas
			if (string.IsNullOrWhiteSpace(dto.Correo) ||
				string.IsNullOrWhiteSpace(dto.Contrasenia) ||
				dto.PersonasIdPersonas <= 0)
			{
				return BadRequest(new { status = 400, message = "Correo, Contraseña y Persona ID son campos obligatorios y válidos." });
			}

			try
			{
				// 2. Verificar FKs (Persona y Rol) - Sin cambios, es lógica de negocio importante
				var existePersona = await _context.Personas
					.AnyAsync(p => p.IdPersonas == dto.PersonasIdPersonas && p.EstaEliminado == false);

				if (!existePersona)
					return BadRequest(new { status = 400, message = $"La Persona con ID {dto.PersonasIdPersonas} no existe o ha sido eliminada." });

				if (dto.RolesIdRoles.HasValue && dto.RolesIdRoles.Value > 0)
				{
					var existeRol = await _context.Roles.AnyAsync(r => r.IdRoles == dto.RolesIdRoles.Value);
					if (!existeRol)
						return BadRequest(new { status = 400, message = $"El Rol con ID {dto.RolesIdRoles} no existe." });
				}

				// 3. Generar Hash (Clave del P1-01)
				string contraseniaHash = BCrypt.Net.BCrypt.HashPassword(dto.Contrasenia);

				// 🔑 8. Usar AutoMapper para el mapeo inicial (ignora Contrasenia por el .Ignore() en MappingProfiles)
				var usuario = _mapper.Map<Usuario>(dto);

				// 🔑 9. Asignar el Hash manualmente (ya que AutoMapper lo ignoró)
				usuario.Contrasenia = contraseniaHash;

				_context.Usuarios.Add(usuario);
				await _context.SaveChangesAsync();

				// 10. Para devolver el DTO completo, debemos cargar Persona y Rol (si es necesario)
				// La opción más limpia es recargar la entidad, o simplemente mapear la que ya tenemos
				// Si quieres evitar la carga extra, mapeamos la entidad 'usuario' y el DTO

				// 🔑 11. Mapear el resultado final a DTO
				var usuarioDtoResultado = _mapper.Map<UsuarioDto>(usuario);

				// Opcional: Cargar Persona para el DTO de retorno (solo si es necesario para el frontend)
				if (usuarioDtoResultado.PersonasIdPersonas > 0)
				{
					var persona = await _context.Personas.FindAsync(usuario.PersonasIdPersonas);
					if (persona != null)
					{
						usuarioDtoResultado.Persona = _mapper.Map<PersonaDto>(persona);
					}
				}

				return CreatedAtAction(nameof(GetUsuario), new { id = usuario.IdUsuarios }, usuarioDtoResultado);
			}
			catch (DbUpdateException ex)
			{
				return BadRequest(new { status = 400, message = ex.InnerException?.Message ?? ex.Message, detail = "Error al guardar en la base de datos (Verifique las FKs o correo duplicado)." });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { status = 500, message = ex.Message, detail = "Error inesperado al crear el usuario." });
			}
		}

		[HttpPut("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> PutUsuario(int id, CreateUsuarioDto dto)
		{
			// ... (Validaciones de ID y FKs - Sin cambios)

			try
			{
				var usuario = await _context.Usuarios.FindAsync(id);
				if (usuario == null)
					return NotFound(new { status = 404, message = $"No se encontró ningún usuario con el ID {id}." });

				// ... (Validación de FKs - Sin cambios)

				// 🔑 12. Usar AutoMapper para aplicar los cambios del DTO a la entidad EXISTENTE.
				// Esto actualiza Correo, PersonasIdPersonas, RolesIdRoles.
				// Contrasenia es ignorada por el perfil de mapeo.
				_mapper.Map(dto, usuario);

				// 🔑 13. Lógica de Hashing para actualizar la contraseña (si se proporciona)
				if (!string.IsNullOrWhiteSpace(dto.Contrasenia) && dto.Contrasenia != "PASSWORD_HASH_AQUI") // Asumimos que el DTO viene con la nueva clave
				{
					usuario.Contrasenia = BCrypt.Net.BCrypt.HashPassword(dto.Contrasenia);
				}
				// Si la Contrasenia es nula o vacía, no hacemos nada, conservamos el hash existente.

				_context.Entry(usuario).State = EntityState.Modified;
				await _context.SaveChangesAsync();

				return NoContent();
			}
			catch (DbUpdateConcurrencyException) when (!UsuarioExists(id))
			{
				return NotFound(new { status = 404, message = $"El usuario con ID {id} fue eliminado por otro usuario." });
			}
			catch (DbUpdateException ex)
			{
				return BadRequest(new { status = 400, message = ex.InnerException?.Message ?? ex.Message, detail = "Error al actualizar la base de datos." });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { status = 500, message = ex.Message, detail = "Error inesperado al actualizar el usuario." });
			}
		}

		[HttpDelete("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> DeleteUsuario(int id)
		{
			// ... (El método DeleteUsuario no necesita cambios, ya que solo toca la FK de Persona)
			if (id <= 0)
				return BadRequest(new { status = 400, message = "El ID debe ser mayor a cero." });

			try
			{
				var usuario = await _context.Usuarios
					.Include(u => u.Persona)
					.FirstOrDefaultAsync(u => u.IdUsuarios == id);

				if (usuario == null || usuario.Persona == null)
					return NotFound(new { status = 404, message = $"Usuario con ID {id} no encontrado." });

				if (usuario.Persona.EstaEliminado)
					return NoContent();

				usuario.Persona.EstaEliminado = true;

				_context.Entry(usuario.Persona).State = EntityState.Modified;

				await _context.SaveChangesAsync();

				return NoContent();
			}
			catch (DbUpdateException ex)
			{
				return BadRequest(new { status = 400, message = "Error al eliminar el registro (puede tener dependencias en Venta).", detail = ex.InnerException?.Message ?? ex.Message });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { status = 500, message = "Error inesperado al eliminar el usuario.", detail = ex.Message });
			}
		}
	}
}

