using ApiProyecto.Dtos;
using ApiProyecto.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiProyecto.Controllers
{

	[Route("api/[controller]")]
	[ApiController]
	public class DetalleVentaController : ControllerBase
	{
		private readonly EmpresaDbContext _context;
		private readonly IMapper _mapper; // 🔑 2. Declarar IMapper

		// 🔑 3. Inyectar IMapper en el constructor
		public DetalleVentaController(EmpresaDbContext context, IMapper mapper)
		{
			_context = context;
			_mapper = mapper;
		}

		// --- Métodos Auxiliares de Validación --- (Sin cambios)
		private async Task<bool> VentaExistsAsync(int idVenta)
		{
			return await _context.Ventas.AnyAsync(v => v.IdVentas == idVenta);
		}
		private async Task<Producto?> GetProductoAsync(int idProducto)
		{
			return await _context.Productos.FindAsync(idProducto);
		}


		//-------------------------------------------------------------
		// GET ALL (Utiliza AutoMapper para todo el anidamiento)
		//-------------------------------------------------------------
		[HttpGet]
		public async Task<ActionResult<IEnumerable<DetalleVentaDto>>> GetDetalles()
		{
			try
			{
				// 🔑 4. Obtener la Entidad con todas las inclusiones necesarias
				var detalles = await _context.Detalleventa
	.Include(d => d.ProductosIdProductosNavigation) // Incluye Producto (Rama 1)

	// Inclusión de la rama Venta -> Usuario -> Rol
	.Include(d => d.VentasIdVentasNavigation) // Incluye Venta (Rama 2, Nivel 1)
		.ThenInclude(v => v.UsuariosIdUsuariosNavigation) // Incluye Usuario (Nivel 2)
			.ThenInclude(u => u.RolesIdRolesNavigation) // 🔑 Incluye Rol (Nivel 3)

	// Inclusión de la rama Venta -> Usuario -> Persona
	.Include(d => d.VentasIdVentasNavigation) // Repite para la rama Persona
		.ThenInclude(v => v.UsuariosIdUsuariosNavigation)
			.ThenInclude(u => u.Persona) // 🔑 Incluye Persona (Nivel 3)

	.ToListAsync();

				// 🔑 5. Usar AutoMapper para el mapeo anidado
				// AutoMapper resolverá DetalleVenta -> Venta -> Usuario -> Persona
				// Siempre y cuando las reglas en MappingProfiles estén definidas.
				return Ok(_mapper.Map<List<DetalleVentaDto>>(detalles));
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { status = 500, message = "Error al obtener la lista de detalles de venta.", detail = ex.Message });
			}
		}

		//-------------------------------------------------------------
		// GET BY ID (Utiliza AutoMapper)
		//-------------------------------------------------------------
		[HttpGet("{id}")]
		public async Task<ActionResult<DetalleVentaDto>> GetDetalle(int id)
		{
			if (id <= 0)
				return BadRequest(new { status = 400, message = "El ID debe ser un valor positivo." });

			try
			{
				// 🔑 6. Obtener la Entidad con inclusiones
				var detalle = await _context.Detalleventa
					.Include(d => d.ProductosIdProductosNavigation)
					.Include(d => d.VentasIdVentasNavigation)
					.FirstOrDefaultAsync(d => d.IdDetalleVenta == id);

				if (detalle == null)
					return NotFound(new { status = 404, message = $"Detalle de venta con ID {id} no encontrado." });

				// 🔑 7. Usar AutoMapper para mapear Detalleventum a DetalleVentaDto
				return Ok(_mapper.Map<DetalleVentaDto>(detalle));
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { status = 500, message = "Error al buscar el detalle de venta.", detail = ex.Message });
			}
		}

		//-------------------------------------------------------------
		// POST (Utiliza AutoMapper y mantiene la lógica de negocio)
		//-------------------------------------------------------------
		[HttpPost]
		public async Task<ActionResult<DetalleVentaDto>> PostDetalle(CreateDetalleVentaDto dto)
		{
			// 1. Validaciones de datos (sin cambios)
			if (!dto.Cantidad.HasValue || dto.Cantidad.Value <= 0)
				return BadRequest(new { status = 400, message = "La cantidad debe ser un valor positivo." });

			// ... (Otras validaciones)

			try
			{
				// 2. Validación de FK (sin cambios)
				if (!await VentaExistsAsync(dto.VentasIdVentas.Value))
					return BadRequest(new { status = 400, message = $"La Venta con ID {dto.VentasIdVentas.Value} no existe." });

				var producto = await GetProductoAsync(dto.ProductosIdProductos.Value);
				if (producto == null)
					return BadRequest(new { status = 400, message = $"El Producto con ID {dto.ProductosIdProductos.Value} no existe." });

				// 🔑 8. Usar AutoMapper para mapear el DTO a la Entidad
				var detalle = _mapper.Map<Detalleventum>(dto);

				// 9. CÁLCULO EN EL SERVIDOR (Lógica de negocio: debe permanecer en el Controller)
				detalle.PrecioTotal = (decimal?)(detalle.Cantidad.Value * detalle.PrecioUnitario.Value);

				_context.Detalleventa.Add(detalle);
				await _context.SaveChangesAsync();

				// 🔑 10. Mapear la Entidad guardada para la respuesta CreatedAtAction
				return CreatedAtAction(nameof(GetDetalle), new { id = detalle.IdDetalleVenta }, _mapper.Map<DetalleVentaDto>(detalle));
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { status = 500, message = "Error al crear el detalle de venta.", detail = ex.Message });
			}
		}

		//-------------------------------------------------------------
		// PUT (Utiliza AutoMapper y mantiene la lógica de negocio)
		//-------------------------------------------------------------
		[HttpPut("{id}")]
		public async Task<IActionResult> PutDetalle(int id, CreateDetalleVentaDto dto)
		{
			// 1. Validaciones de entrada (sin cambios)
			// ...

			try
			{
				var detalle = await _context.Detalleventa.FindAsync(id);
				if (detalle == null) return NotFound(new { status = 404, message = $"Detalle con ID {id} no encontrado." });

				// 2. Validar que las FKs si se modifican, sean válidas (sin cambios)
				// ...

				// 🔑 11. Usar AutoMapper para actualizar las propiedades del DTO a la entidad EXISTENTE
				// Las propiedades nulas en el DTO se ignorarán si tienes el mapeo configurado correctamente.
				_mapper.Map(dto, detalle);

				// 3. Recalcular PrecioTotal (Lógica de negocio: debe permanecer aquí)
				if (detalle.Cantidad.HasValue && detalle.PrecioUnitario.HasValue)
				{
					detalle.PrecioTotal = (decimal?)(detalle.Cantidad.Value * detalle.PrecioUnitario.Value);
				}

				_context.Entry(detalle).State = EntityState.Modified;
				await _context.SaveChangesAsync();

				return NoContent();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!_context.Detalleventa.Any(e => e.IdDetalleVenta == id))
					return NotFound(new { status = 404, message = $"Detalle con ID {id} no encontrado." });
				throw;
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { status = 500, message = "Error al actualizar el detalle de venta.", detail = ex.Message });
			}
		}

		//-------------------------------------------------------------
		// DELETE (Sin cambios)
		//-------------------------------------------------------------
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteDetalle(int id)
		{
			// ... (Tu código de eliminación existente, sin cambios)
			if (id <= 0)
				return BadRequest(new { status = 400, message = "El ID debe ser un valor positivo." });

			try
			{
				var detalle = await _context.Detalleventa.FindAsync(id);
				if (detalle == null) return NotFound(new { status = 404, message = $"Detalle con ID {id} no encontrado." });

				_context.Detalleventa.Remove(detalle);
				await _context.SaveChangesAsync();

				return NoContent();
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { status = 500, message = "Error al eliminar el detalle de venta.", detail = ex.Message });
			}
		}

		// --- FUNCIÓN AUXILIAR PARA EL POST (ya no se necesita si usamos AutoMapper) ---
		// [ELIMINAR ESTA FUNCIÓN]
		/*
		private async Task<VentaDto?> GetVentaReferenciaDtoAsync(int idVenta)
		{
			// ...
		}
		*/
	}
}

