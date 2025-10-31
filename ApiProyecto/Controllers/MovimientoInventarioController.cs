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
	[Authorize(Roles = "Admin")] // Solo Administradores pueden ajustar inventario
	public class MovimientoInventarioController : ControllerBase
	{
		private readonly EmpresaDbContext _context;
		private readonly IMapper _mapper;

		public MovimientoInventarioController(EmpresaDbContext context, IMapper mapper)
		{
			_context = context;
			_mapper = mapper;
		}

		// --- POST para registrar Entradas/Ajustes de Inventario ---
		[HttpPost]
		public async Task<ActionResult<MovimientoInventarioDto>> PostMovimientoInventario(
			CreateMovimientoInventarioDto dto)
		{
			// 1. Validar que el producto y el usuario existan
			var producto = await _context.Productos
				.FirstOrDefaultAsync(p => p.IdProductos == dto.ProductosIdProductos.Value);

			var usuario = await _context.Usuarios
				.FirstOrDefaultAsync(u => u.IdUsuarios == dto.UsuariosIdUsuarios.Value);

			if (producto == null || usuario == null)
			{
				return BadRequest("Producto o Usuario no válidos.");
			}

			// 2. Iniciar la Transacción (para garantizar que el stock y el movimiento se guarden)
			using var transaction = await _context.Database.BeginTransactionAsync();

			try
			{
				// 3. Registrar el Movimiento (sin cambios)
				var movimiento = _mapper.Map<MovimientoInventario>(dto);
				movimiento.FechaMovimiento = DateTime.Now;

				_context.MovimientosInventario.Add(movimiento);

				// 4. Actualizar el Stock del Producto (SUMA) (sin cambios)
				if (movimiento.Cantidad.HasValue)
				{
					if (movimiento.TipoMovimiento == "Entrada" || movimiento.TipoMovimiento == "Ajuste")
					{
						producto.Stock = (producto.Stock ?? 0) + movimiento.Cantidad.Value;
					}
					else
					{
						// Usamos 'continue' en el nombre de la variable para evitar conflicto
						throw new InvalidOperationException("Solo se permiten movimientos de tipo 'Entrada' o 'Ajuste' en este endpoint.");
					}
				}

				_context.Productos.Update(producto);

				// 5. Guardar todo
				await _context.SaveChangesAsync();

				// 🔑 6. COMMIT DE LA TRANSACCIÓN (Solo si el código llega hasta aquí)
				await transaction.CommitAsync();

				return CreatedAtAction(
					nameof(GetMovimientoInventario),
					new { id = movimiento.IdMovimientoInventario },
					_mapper.Map<MovimientoInventarioDto>(movimiento));
			}
			catch (InvalidOperationException ex)
			{
				// 🛑 ROLLBACK por lógica de negocio (ej. el TipoMovimiento no es válido)
				await transaction.RollbackAsync();
				return BadRequest(new { status = 400, message = ex.Message });
			}
			catch (Exception)
			{
				// 🛑 ROLLBACK por cualquier otro error de DB/Sistema
				// Esto asegura que la transacción se cierre antes de lanzar la excepción al filtro P1-06
				await transaction.RollbackAsync();
				throw; // P1-06 lo captura y devuelve 500
			}
		}

		// 🔑 TAREA PENDIENTE: Crea el método GET que se usa en CreatedAtAction.
		[HttpGet("{id}")]
		public async Task<ActionResult<MovimientoInventarioDto>> GetMovimientoInventario(int id)
		{
			var movimiento = await _context.MovimientosInventario.FindAsync(id);
			if (movimiento == null)
			{
				return NotFound();
			}
			return _mapper.Map<MovimientoInventarioDto>(movimiento);
		}
	}
}
