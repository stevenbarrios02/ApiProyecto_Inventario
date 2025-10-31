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
	public class PersonasController : ControllerBase
	{
		private readonly EmpresaDbContext _context;
		private readonly IMapper _mapper; // 🔑 2. Declarar IMapper

		// 🔑 3. Inyectar IMapper en el constructor
		public PersonasController(EmpresaDbContext context, IMapper mapper)
		{
			_context = context;
			_mapper = mapper;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<PersonaDto>>> GetPersonas()
		{

			try
			{
				// 🔑 4. Simplificar: Obtener la entidad completa (Persona) de la DB
				var personas = await _context.Personas.ToListAsync();

				if (personas == null || !personas.Any())
					return Ok(new List<PersonaDto>());

				// 🔑 5. Usar AutoMapper para convertir List<Persona> a List<PersonaDto>
				var personasDtos = (_mapper.Map<List<PersonaDto>>(personas));
				var resultado = new ApiResultDto<PersonaDto>(personasDtos.ToList());

				return Ok(resultado);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { status = 500, message = ex.Message, detail = "Error al obtener las personas." });
			}
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<PersonaDto>> GetPersona(int id)
		{
			if (id <= 0)
				return BadRequest(new { status = 400, message = "El ID debe ser mayor a cero." });

			try
			{
				// 🔑 6. Simplificar: Obtener la entidad completa (Persona) de la DB
				var persona = await _context.Personas
					.FirstOrDefaultAsync(p => p.IdPersonas == id);

				if (persona == null)
					return NotFound(new { status = 404, message = $"No se encontró ninguna persona con el ID {id}." });

				// 🔑 7. Usar AutoMapper para convertir Persona a PersonaDto
				return Ok(_mapper.Map<PersonaDto>(persona));
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { status = 500, message = ex.Message, detail = "Error al buscar la persona." });
			}
		}

		[HttpPost]
		public async Task<ActionResult<PersonaDto>> PostPersona(CreatePersonaDto dto)
		{
			// ... (Validaciones de campos requeridos)
			if (string.IsNullOrWhiteSpace(dto.Nombre) ||
				string.IsNullOrWhiteSpace(dto.Apellido) ||
				string.IsNullOrWhiteSpace(dto.Cedula) ||
				string.IsNullOrWhiteSpace(dto.Telefono) ||
				string.IsNullOrWhiteSpace(dto.Cargo))
			{
				return BadRequest(new
				{
					status = 400,
					message = "Todos los campos requeridos deben estar completos (Nombre, Apellido, Cédula, Teléfono, Cargo)."
				});
			}

			try
			{
				var existeCedula = await _context.Personas.AnyAsync(p => p.Cedula == dto.Cedula);
				if (existeCedula)
					return BadRequest(new { status = 400, message = $"Ya existe una persona registrada con la cédula {dto.Cedula}." });

				// 🔑 8. Usar AutoMapper para mapear CreatePersonaDto a Persona.
				// Esto reemplaza todo el mapeo manual (var persona = new Persona { ... })
				var persona = _mapper.Map<Persona>(dto);

				_context.Personas.Add(persona);
				await _context.SaveChangesAsync();

				// 🔑 9. Usar AutoMapper para mapear la entidad guardada de vuelta a PersonaDto
				return CreatedAtAction(nameof(GetPersona), new { id = persona.IdPersonas }, _mapper.Map<PersonaDto>(persona));
			}
			catch (DbUpdateException ex)
			{
				return BadRequest(new { status = 400, message = ex.InnerException?.Message ?? ex.Message, detail = "Error al guardar en la base de datos." });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { status = 500, message = ex.Message, detail = "Error inesperado al crear la persona." });
			}
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> PutPersona(int id, CreatePersonaDto dto)
		{
			// ... (Validaciones y búsqueda de persona)
			if (id <= 0)
				return BadRequest(new { status = 400, message = "El ID debe ser mayor a cero." });

			// ... (Validaciones de campos requeridos)

			try
			{
				var persona = await _context.Personas.FindAsync(id);

				if (persona == null)
					return NotFound(new { status = 404, message = $"No se encontró ninguna persona con el ID {id}." });

				if (persona.EstaEliminado)
				{
					return NotFound(new { status = 404, message = $"La persona con ID {id} ha sido eliminada lógicamente y no puede ser actualizada." });
				}

				// ... (Validación de cédula duplicada)

				// 🔑 10. Usar AutoMapper para mapear las propiedades del DTO a la entidad EXISTENTE.
				// Esto reemplaza las asignaciones manuales (persona.Nombre = dto.Nombre; etc.)
				_mapper.Map(dto, persona);

				await _context.SaveChangesAsync();

				return NoContent();
			}
			catch (DbUpdateException ex)
			{
				return BadRequest(new { status = 400, message = ex.InnerException?.Message ?? ex.Message, detail = "Error al actualizar la base de datos." });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { status = 500, message = ex.Message, detail = "Error inesperado al actualizar la persona." });
			}
		}

		// ... (El método DeletePersona no necesita cambios)
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeletePersona(int id)
		{
			// 1. Iniciar la Transacción
			using var transaction = await _context.Database.BeginTransactionAsync();

			try
			{
				// 2. Buscar la Persona y el Usuario Asociado
				var persona = await _context.Personas
					.Include(p => p.Usuario) // 🔑 Asegurarse de incluir los Usuarios
					.FirstOrDefaultAsync(p => p.IdPersonas == id && p.EstaEliminado == false);

				if (persona == null)
				{
					return NotFound();
				}

				// 3. Eliminar Lógicamente la Persona
				persona.EstaEliminado = true;
				persona.FechaEliminacion = DateTime.Now;
				_context.Personas.Update(persona);

				// 4. Eliminar Lógicamente los Usuarios asociados
				if (persona.Usuario != null) // 🔑 Corregido: Usamos el objeto Usuario (singular)
				{
					var usuario = persona.Usuario;

					usuario.EstaEliminado = true;
					usuario.FechaEliminacion = DateTime.Now;
					_context.Usuarios.Update(usuario);
				}

				// 5. Guardar los cambios (Persona + Usuario)
				await _context.SaveChangesAsync();

				// 6. Commit de la Transacción
				await transaction.CommitAsync();

				return NoContent();
			}
			catch (Exception)
			{
				// 🛑 Rollback si algo falla
				await transaction.RollbackAsync();
				throw; // Capturado por P1-06 (Manejo de Errores)
			}
		}
	}
}
