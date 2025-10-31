using ApiProyecto.Dtos;
using ApiProyecto.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiProyecto.Controllers
{

	[Route("api/[controller]")]
	[ApiController]
	[Authorize(Roles = "Admin")]
	public class RolesController : ControllerBase
	{
		private readonly EmpresaDbContext _context;

		public RolesController(EmpresaDbContext context)
		{
			_context = context;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<RolDto>>> GetRoles()
		{
			try
			{
				var roles = await _context.Roles
					.Select(r => new RolDto
					{
						IdRoles = r.IdRoles,
						Nombre = r.Nombre
					})
					.ToListAsync();

				return Ok(roles);
			}
			catch (Exception ex)
			{

				return StatusCode(500, new { status = 500, message = "Error al obtener la lista de roles.", detail = ex.Message });
			}
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<RolDto>> GetRol(int id)
		{

			if (id <= 0)
				return BadRequest(new { status = 400, message = "El ID de Rol debe ser un valor positivo." });

			try
			{
				var rol = await _context.Roles
					.Where(r => r.IdRoles == id)
					.Select(r => new RolDto
					{
						IdRoles = r.IdRoles,
						Nombre = r.Nombre
					})
					.FirstOrDefaultAsync();

				if (rol == null)
					return NotFound(new { status = 404, message = $"Rol con ID {id} no encontrado." });

				return Ok(rol);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { status = 500, message = "Error al buscar el rol.", detail = ex.Message });
			}
		}

		[HttpPost]
		public async Task<ActionResult<RolDto>> PostRol(CreateRolDto dto)
		{
			if (string.IsNullOrWhiteSpace(dto.Nombre))
				return BadRequest(new { status = 400, message = "El nombre del rol es requerido." });

			try
			{
				if (await _context.Roles.AnyAsync(r => r.Nombre.ToLower() == dto.Nombre.ToLower()))
					return Conflict(new { status = 409, message = $"Ya existe un rol con el nombre '{dto.Nombre}'." });

				var rol = new Role { Nombre = dto.Nombre };

				_context.Roles.Add(rol);
				await _context.SaveChangesAsync();

				return CreatedAtAction(nameof(GetRol), new { id = rol.IdRoles }, new RolDto
				{
					IdRoles = rol.IdRoles,
					Nombre = rol.Nombre
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { status = 500, message = "Error al crear el rol.", detail = ex.Message });
			}
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> PutRol(int id, CreateRolDto dto)
		{
			if (id <= 0)
				return BadRequest(new { status = 400, message = "El ID de Rol debe ser un valor positivo." });
			if (string.IsNullOrWhiteSpace(dto.Nombre))
				return BadRequest(new { status = 400, message = "El nombre del rol es requerido." });

			try
			{
				var rol = await _context.Roles.FindAsync(id);

				if (rol == null)
					return NotFound(new { status = 404, message = $"Rol con ID {id} no encontrado." });

				if (await _context.Roles.AnyAsync(r => r.Nombre.ToLower() == dto.Nombre.ToLower() && r.IdRoles != id))
					return Conflict(new { status = 409, message = $"Ya existe otro rol con el nombre '{dto.Nombre}'." });


				rol.Nombre = dto.Nombre;
				_context.Entry(rol).State = EntityState.Modified;
				await _context.SaveChangesAsync();

				return NoContent();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!_context.Roles.Any(e => e.IdRoles == id))
					return NotFound(new { status = 404, message = $"Rol con ID {id} no encontrado." });
				throw;
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { status = 500, message = "Error al actualizar el rol.", detail = ex.Message });
			}
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteRol(int id)
		{
			if (id <= 0)
				return BadRequest(new { status = 400, message = "El ID de Rol debe ser un valor positivo." });

			try
			{
				var rol = await _context.Roles.FindAsync(id);

				if (rol == null)
					return NotFound(new { status = 404, message = $"Rol con ID {id} no encontrado." });

				_context.Roles.Remove(rol);
				await _context.SaveChangesAsync();

				return NoContent();
			}
			catch (DbUpdateException ex) when (ex.InnerException?.Message?.Contains("FOREIGN KEY constraint failed") == true || ex.InnerException?.Message?.Contains("Cannot delete or update a parent row") == true)
			{
				return Conflict(new
				{
					status = 409,
					message = "Error al eliminar el Rol. Existen usuarios u otras entidades que dependen de este Rol.",
					detail = ex.InnerException?.Message
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { status = 500, message = "Error al eliminar el rol.", detail = ex.Message });
			}
		}
	}
}

