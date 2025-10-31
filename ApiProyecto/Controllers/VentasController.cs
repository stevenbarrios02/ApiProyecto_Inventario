using ApiProyecto.Dtos;
using ApiProyecto.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace ApiProyecto.Controllers
{

	[Route("api/[controller]")]
	[ApiController]
	public class VentasController : ControllerBase
	{
		private readonly EmpresaDbContext _context;
		private readonly IMapper _mapper; // 🔑 2. Declarar IMapper

		// 🔑 3. Inyectar IMapper en el constructor
		public VentasController(EmpresaDbContext context, IMapper mapper)
		{
			_context = context;
			_mapper = mapper;
		}

		// Método auxiliar para validar el Usuario (Sin cambios, es lógica de negocio)
		private async Task<bool> UsuarioExistsAsync(int idUsuario)
		{
			// Verificamos que el Usuario exista y que la Persona asociada NO esté eliminada (soft delete).
			return await _context.Usuarios
				.Include(u => u.Persona)
				.AnyAsync(u =>
					u.IdUsuarios == idUsuario &&
					u.Persona != null &&
					u.Persona.EstaEliminado == false);
		}

		//-------------------------------------------------------------
		// GET ALL (Utiliza AutoMapper)
		//-------------------------------------------------------------
		[HttpGet]
		[Authorize(Roles = "Admin,Vendedor")]
		public async Task<ActionResult<IEnumerable<VentaDto>>> GetVentas()
		{
			try
			{
				// 🔑 4. Obtener la Entidad con todas las inclusiones necesarias
				var ventas = await _context.Ventas
					.Include(v => v.UsuariosIdUsuariosNavigation)
					.ThenInclude(u => u.Persona) // 1. Carga Persona (rama 1)
					.Include(v => v.UsuariosIdUsuariosNavigation) // 2. Vuelve a empezar la inclusión desde Venta
						.ThenInclude(u => u.RolesIdRolesNavigation) // 3. Carga Rol (rama 2)
					.ToListAsync(); // No necesitamos Select() aquí

				// 🔑 5. Usar AutoMapper para el mapeo anidado
				return Ok(_mapper.Map<List<VentaDto>>(ventas));
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { status = 500, message = "Error al obtener ventas.", detail = ex.Message });
			}
		}

		//-------------------------------------------------------------
		// GET BY ID (Utiliza AutoMapper)
		//-------------------------------------------------------------
		[HttpGet("{id}")]
		[Authorize(Roles = "Admin,Vendedor")]
		public async Task<ActionResult<VentaDto>> GetVenta(int id)
		{
			if (id <= 0)
				return BadRequest(new { status = 400, message = "El ID debe ser mayor a cero." });

			try
			{
				// 🔑 6. Obtener la Entidad con inclusiones
				var venta = await _context.Ventas
					.Where(v => v.IdVentas == id)
					.Include(v => v.UsuariosIdUsuariosNavigation)
					.ThenInclude(u => u.Persona) // 1. Carga Persona (rama 1)
					.Include(v => v.UsuariosIdUsuariosNavigation) // 2. Vuelve a empezar la inclusión desde Venta
						.ThenInclude(u => u.RolesIdRolesNavigation)
					.FirstOrDefaultAsync();

				if (venta == null)
					return NotFound(new { status = 404, message = $"No se encontró ninguna venta con el ID {id}." });

				// 🔑 7. Usar AutoMapper para mapear Venta a VentaDto
				return Ok(_mapper.Map<VentaDto>(venta));
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { status = 500, message = "Error al buscar la venta.", detail = ex.Message });
			}
		}

		//-------------------------------------------------------------
		// POST (Utiliza AutoMapper)
		//-------------------------------------------------------------

		[HttpPost]
		[Authorize(Roles = "Admin,Vendedor")]
		public async Task<ActionResult<VentaDto>> PostVenta(CreateVentaDto dto)
		{
			// 1. Validaciones básicas
			if (!dto.UsuariosIdUsuarios.HasValue || !await UsuarioExistsAsync(dto.UsuariosIdUsuarios.Value))
			{
				return BadRequest(new { status = 400, message = "El Usuario no existe o ha sido eliminado." });
			}

			// 🔑 INICIAR LA TRANSACCIÓN ATÓMICA
			using var transaction = await _context.Database.BeginTransactionAsync();

			try
			{
				// 2. Mapear y guardar la Venta (para obtener el IdVentas)
				var venta = _mapper.Map<Venta>(dto);
				venta.Fecha = dto.Fecha ?? DateTime.Now;
				venta.Total = 0;

				_context.Ventas.Add(venta);
				await _context.SaveChangesAsync();

				double totalVentaCalculado = 0;

				// 3. Procesar, Descontar Stock y Guardar Detalles
				foreach (var detalleDto in dto.Detalles)
				{
					// Validar que el producto y la cantidad existan
					if (!detalleDto.ProductosIdProductos.HasValue || !detalleDto.Cantidad.HasValue)
					{
						throw new InvalidOperationException("Faltan datos de Producto o Cantidad en el detalle.");
					}

					// 🔑 3a. BUSCAR EL PRODUCTO (CON SU STOCK INDIVIDUAL)
					var producto = await _context.Productos
						.FirstOrDefaultAsync(p => p.IdProductos == detalleDto.ProductosIdProductos.Value);

					// 🔑 3b. VALIDACIÓN CRÍTICA DEL STOCK
					if (producto == null || !producto.Stock.HasValue || producto.Stock.Value < detalleDto.Cantidad)
					{
						throw new InvalidOperationException($"Stock insuficiente para Producto ID: {detalleDto.ProductosIdProductos.Value}. Stock actual: {producto?.Stock ?? 0}.");
					}

					// 🔑 3c. Descontar Stock y Actualizar Producto
					producto.Stock -= detalleDto.Cantidad.Value;
					_context.Productos.Update(producto); // Marcar como modificado

					var movimiento = new MovimientoInventario
					{
						FechaMovimiento = DateTime.Now,
						TipoMovimiento = "Salida", // Por ser una venta
						Cantidad = detalleDto.Cantidad,
						ProductosIdProductos = detalleDto.ProductosIdProductos.Value,
						// Usamos el ID de la Venta para referenciar el movimiento
						Referencia = $"VTA-{venta.IdVentas}",
						// ID del usuario que generó la venta.
						UsuariosIdUsuarios = venta.UsuariosIdUsuarios
					};

					_context.MovimientosInventario.Add(movimiento);

					// 3d. Mapear y configurar el Detalle de Venta
					var detalle = _mapper.Map<Detalleventum>(detalleDto);
					detalle.VentasIdVentas = venta.IdVentas;
					detalle.PrecioTotal = (decimal?)(detalle.PrecioUnitario * detalle.Cantidad);

					_context.Detalleventa.Add(detalle);
					totalVentaCalculado += detalle.PrecioTotal.HasValue ? (double)detalle.PrecioTotal.Value : 0;
				}

				// 4. Actualizar el Total de la Venta
				venta.Total = totalVentaCalculado;
				_context.Ventas.Update(venta);

				// 5. Guardar todos los cambios (Productos, Detalles, Total de Venta)
				await _context.SaveChangesAsync();

				// 6. COMMIT DE LA TRANSACCIÓN
				await transaction.CommitAsync();

				// 7. Recuperar y devolver la Venta (Tu lógica de inclusiones)
				var ventaConInclusiones = await _context.Ventas
					.Where(v => v.IdVentas == venta.IdVentas)
					.Include(v => v.Detalleventa)
					.Include(v => v.UsuariosIdUsuariosNavigation)
						.ThenInclude(u => u.Persona)
					.Include(v => v.UsuariosIdUsuariosNavigation)
						.ThenInclude(u => u.RolesIdRolesNavigation)
					.FirstOrDefaultAsync();

				return CreatedAtAction(
					nameof(GetVenta),
					new { id = venta.IdVentas },
					_mapper.Map<VentaDto>(ventaConInclusiones)
				);
			}
			catch (InvalidOperationException ex)
			{
				// 🛑 ROLLBACK por lógica de negocio (Stock insuficiente)
				await transaction.RollbackAsync();
				return BadRequest(new { status = 400, message = ex.Message });
			}
			catch (Exception ex)
			{
				// 🛑 ROLLBACK por cualquier otro error
				await transaction.RollbackAsync();
				throw; // El filtro P1-06 lo captura y devuelve un 500 limpio
			}
		}


		//-------------------------------------------------------------
		// PUT: api/Ventas/5 (Utiliza AutoMapper)
		//-------------------------------------------------------------
		[HttpPut("{id}")]
		public async Task<IActionResult> PutVenta(int id, CreateVentaDto dto)
		{
			// ... (Validaciones - Sin cambios)

			try
			{
				var venta = await _context.Ventas.FindAsync(id);
				if (venta == null)
					return NotFound(new { status = 404, message = $"No se encontró ninguna venta con el ID {id}." });

				// 1. Verificar la FK principal si se está cambiando (Sin cambios)
				if (venta.UsuariosIdUsuarios != dto.UsuariosIdUsuarios.Value)
				{
					if (!await UsuarioExistsAsync(dto.UsuariosIdUsuarios.Value))
					{
						return BadRequest(new { status = 400, message = "El nuevo Usuario o la Persona asociada no existen o han sido eliminados." });
					}
				}

				// 🔑 11. Usar AutoMapper para actualizar las propiedades del DTO a la entidad EXISTENTE
				_mapper.Map(dto, venta);

				// Asegurarse de que Total se actualice (aunque AutoMapper debería hacerlo si no es null)
				if (dto.Total.HasValue)
				{
					venta.Total = dto.Total.Value;
				}

				_context.Entry(venta).State = EntityState.Modified;
				await _context.SaveChangesAsync();

				return NoContent();
			}
			catch (DbUpdateConcurrencyException)
			{
				return NotFound(new { status = 404, message = $"La venta con ID {id} fue eliminada por otro usuario o no existe." });
			}
			catch (DbUpdateException ex)
			{
				return BadRequest(new { status = 400, message = ex.InnerException?.Message ?? ex.Message, detail = "Error al actualizar la base de datos." });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { status = 500, message = ex.Message, detail = "Error inesperado al actualizar la venta." });
			}
		}

		[HttpDelete("{id}")]
		// ... (El resto del Delete no tiene cambios)
		public async Task<IActionResult> DeleteVenta(int id)
		{
			if (id <= 0)
				return BadRequest(new { status = 400, message = "El ID debe ser mayor a cero." });

			try
			{
				var venta = await _context.Ventas.FindAsync(id);
				if (venta == null)
					return NotFound(new { status = 404, message = $"No se encontró ninguna venta con el ID {id}." });

				_context.Ventas.Remove(venta);
				await _context.SaveChangesAsync();

				return NoContent();
			}
			catch (DbUpdateException ex)
			{
				return BadRequest(new { status = 400, message = "Error al eliminar el registro (puede tener dependencias).", detail = ex.InnerException?.Message ?? ex.Message });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { status = 500, message = ex.Message, detail = "Error inesperado al eliminar la venta." });
			}
		}
	}
}

