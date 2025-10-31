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
	public class ContratosController : ControllerBase
	{
		private readonly EmpresaDbContext _context;

		public ContratosController(EmpresaDbContext context)
		{
			_context = context;
		}
		private bool ContratoExists(int id)
		{
			return _context.Contratos.Any(e => e.IdContratos == id);
		}

		[HttpGet]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<IEnumerable<ContratoDto>>> GetContratos()
		{
			try
			{
				var contratos = await _context.Contratos
					.Include(c => c.PersonasIdPersonasNavigation)
					.Select(c => new ContratoDto
					{
						IdContratos = c.IdContratos,
						Sueldo = c.Sueldo,
						FechaInicio = c.FechaInicio,
						FechaFinal = c.FechaFinal,
						Cargo = c.Cargo ?? "",
						PersonasIdPersonas = c.PersonasIdPersonas,

						Persona = c.PersonasIdPersonasNavigation == null ? null : new PersonaDto
						{
							IdPersonas = c.PersonasIdPersonasNavigation.IdPersonas,
							Nombre = c.PersonasIdPersonasNavigation.Nombre,
							Apellido = c.PersonasIdPersonasNavigation.Apellido,
							Cedula = c.PersonasIdPersonasNavigation.Cedula,
							Telefono = c.PersonasIdPersonasNavigation.Telefono,
							Direccion = c.PersonasIdPersonasNavigation.Direccion,
							Cargo = c.PersonasIdPersonasNavigation.Cargo,
							EstaEliminado = c.PersonasIdPersonasNavigation.EstaEliminado,
						}
					})
					.ToListAsync();

				return Ok(contratos);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { status = 500, message = "Error al obtener contratos.", detail = ex.Message });
			}
		}
		[HttpGet("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ContratoDto>> GetContrato(int id)
		{
			if (id <= 0)
				return BadRequest(new { status = 400, message = "El ID debe ser mayor a cero." });

			try
			{
				var contrato = await _context.Contratos
					.Where(c => c.IdContratos == id)
					.Include(c => c.PersonasIdPersonasNavigation)
					.Select(c => new ContratoDto
					{
						IdContratos = c.IdContratos,
						Sueldo = c.Sueldo,
						FechaInicio = c.FechaInicio,
						FechaFinal = c.FechaFinal,
						Cargo = c.Cargo ?? "",
						PersonasIdPersonas = c.PersonasIdPersonas,
						Persona = c.PersonasIdPersonasNavigation == null ? null : new PersonaDto
						{
							IdPersonas = c.PersonasIdPersonasNavigation.IdPersonas,
							Nombre = c.PersonasIdPersonasNavigation.Nombre,
							Apellido = c.PersonasIdPersonasNavigation.Apellido,
							Cedula = c.PersonasIdPersonasNavigation.Cedula,
							Telefono = c.PersonasIdPersonasNavigation.Telefono,
							Direccion = c.PersonasIdPersonasNavigation.Direccion,
							Cargo = c.PersonasIdPersonasNavigation.Cargo,
							EstaEliminado = c.PersonasIdPersonasNavigation.EstaEliminado,
						}
					})
					.FirstOrDefaultAsync();

				if (contrato == null)
					return NotFound(new { status = 404, message = $"No se encontró ningún contrato con el ID {id}." });

				return Ok(contrato);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { status = 500, message = "Error al buscar el contrato.", detail = ex.Message });
			}
		}

		[HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ContratoDto>> PostContrato(CreateContratoDto dto)
		{
			if (!dto.Sueldo.HasValue || dto.Sueldo.Value <= 0 ||
				!dto.PersonasIdPersonas.HasValue || dto.PersonasIdPersonas.Value <= 0 ||
				string.IsNullOrWhiteSpace(dto.Cargo))
			{
				return BadRequest(new { status = 400, message = "Sueldo (positivo), Persona ID y Cargo son campos obligatorios y válidos." });
			}

			try
			{
				var existePersona = await _context.Personas
					.AnyAsync(p => p.IdPersonas == dto.PersonasIdPersonas.Value && p.EstaEliminado == false);

				if (!existePersona)
					return BadRequest(new { status = 400, message = $"La Persona con ID {dto.PersonasIdPersonas} no existe o ha sido eliminada (Soft Delete)." });

				var contrato = new Contrato
				{
					Sueldo = dto.Sueldo.Value, 
					FechaInicio = dto.FechaInicio,
					FechaFinal = dto.FechaFinal,
					Cargo = dto.Cargo,
					PersonasIdPersonas = dto.PersonasIdPersonas.Value
				};

				_context.Contratos.Add(contrato);
				await _context.SaveChangesAsync();

				var persona = await _context.Personas.FindAsync(contrato.PersonasIdPersonas);

				return CreatedAtAction(nameof(GetContrato), new { id = contrato.IdContratos }, new ContratoDto
				{
					IdContratos = contrato.IdContratos,
					Sueldo = contrato.Sueldo,
					FechaInicio = contrato.FechaInicio,
					FechaFinal = contrato.FechaFinal,
					Cargo = contrato.Cargo,
					PersonasIdPersonas = contrato.PersonasIdPersonas,
				});
			}
			catch (DbUpdateException ex)
			{
				return BadRequest(new { status = 400, message = ex.InnerException?.Message ?? ex.Message, detail = "Error al guardar en la base de datos (Verifique la FK)." });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { status = 500, message = ex.Message, detail = "Error inesperado al crear el contrato." });
			}
		}

		[HttpPut("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> PutContrato(int id, CreateContratoDto dto)
		{
			if (id <= 0)
				return BadRequest(new { status = 400, message = "El ID de Contrato debe ser mayor a cero." });

			if (!dto.Sueldo.HasValue || dto.Sueldo.Value <= 0 ||
				!dto.PersonasIdPersonas.HasValue || dto.PersonasIdPersonas.Value <= 0 ||
				string.IsNullOrWhiteSpace(dto.Cargo))
			{
				return BadRequest(new { status = 400, message = "Sueldo, Cargo y Persona ID deben ser válidos." });
			}

			try
			{
				var contrato = await _context.Contratos.FindAsync(id);
				if (contrato == null)
					return NotFound(new { status = 404, message = $"No se encontró ningún contrato con el ID {id}." });

				if (contrato.PersonasIdPersonas != dto.PersonasIdPersonas.Value)
				{
					var existeNuevaPersona = await _context.Personas
						.AnyAsync(p => p.IdPersonas == dto.PersonasIdPersonas.Value && p.EstaEliminado == false);
					if (!existeNuevaPersona)
						return BadRequest(new { status = 400, message = $"La nueva Persona con ID {dto.PersonasIdPersonas} no existe o ha sido eliminada." });
				}

				contrato.Sueldo = (decimal)dto.Sueldo;
				contrato.FechaInicio = dto.FechaInicio;
				contrato.FechaFinal = dto.FechaFinal;
				contrato.Cargo = dto.Cargo;
				contrato.PersonasIdPersonas = dto.PersonasIdPersonas.Value;

				_context.Entry(contrato).State = EntityState.Modified;
				await _context.SaveChangesAsync();

				return NoContent();
			}
			catch (DbUpdateConcurrencyException) when (!ContratoExists(id))
			{
				return NotFound(new { status = 404, message = $"El contrato con ID {id} fue eliminado por otro usuario." });
			}
			catch (DbUpdateException ex)
			{
				return BadRequest(new { status = 400, message = ex.InnerException?.Message ?? ex.Message, detail = "Error al actualizar la base de datos." });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { status = 500, message = "Error inesperado al actualizar el contrato." });
			}
		}


		[HttpDelete("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> DeleteContrato(int id)
		{
			if (id <= 0)
				return BadRequest(new { status = 400, message = "El ID debe ser mayor a cero." });

			try
			{
				var contrato = await _context.Contratos.FindAsync(id);
				if (contrato == null)
					return NotFound(new { status = 404, message = $"No se encontró ningún contrato con el ID {id}." });

				_context.Contratos.Remove(contrato);
				await _context.SaveChangesAsync();

				return NoContent();
			}
			catch (DbUpdateException ex)
			{
				return BadRequest(new { status = 400, message = "Error al eliminar el registro (puede tener dependencias).", detail = ex.InnerException?.Message ?? ex.Message });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { status = 500, message = "Error inesperado al eliminar el contrato.", detail = ex.Message });
			}
		}
	}
	}

