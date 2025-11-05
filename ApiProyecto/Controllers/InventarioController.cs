using ApiProyecto.Dtos;
using ApiProyecto.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiProyecto.Controllers
{


	[Route("api/[controller]")]
	[ApiController]
	public class InventarioController : ControllerBase
	{
		private readonly EmpresaDbContext _context;

		public InventarioController(EmpresaDbContext context)
		{
			_context = context;
		}
		private bool InventarioExists(int id)
		{
			return _context.Inventarios.Any(e => e.IdInventario == id);
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<InventarioDto>>> GetInventario()
		{
			try
			{
				var inventarios = await _context.Inventarios
					.Include(i => i.Productos)
					.ToListAsync();

				if (!inventarios.Any())
					return NotFound(new { message = "No hay inventarios registrados." });

				var inventarioDtos = inventarios.Select(i => new InventarioDto
				{
					IdInventario = i.IdInventario,
					FechaIngreso = i.FechaIngreso,
					Productos = i.Productos.Select(p => new ProductoDto
					{
						IdProductos = p.IdProductos,
						Nombre = p.Nombre,
						Precio = p.Precio,
						Categoria = p.Categoria,
						Stock = p.Stock
					}).ToList()
				}).ToList();

				return Ok(inventarioDtos);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "Error al obtener los inventarios.", detail = ex.Message });
			}
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<InventarioDto>> GetInventario(int id)
		{
			// 🛑 NOTA: Ignoramos el 'id' porque la tabla 'Inventario' no se usa para el stock.
			// Siempre devolveremos el inventario completo, a menos que quieras filtrar por ese ID.

			// Si realmente quieres validar que el ID existe (para una futura tabla de Sucursales):
			var inventarioFisico = await _context.Inventarios.FindAsync(id);

			if (inventarioFisico == null)
			{
				// Devolvemos 404 si el registro de la cabecera de Inventario no existe
				return NotFound(new { status = 404, message = $"No se encontró la cabecera de inventario con el ID {id}." });
			}

			try
			{
				// 1. Obtener los IDs de todos los productos que han tenido movimientos asociados a este 'id' (si fuera un filtro)
				// O, más simple, obtener TODOS los productos con algún movimiento, ya que el 'id' es genérico (1).

				var productosConMovimiento = await _context.MovimientosInventario
					.Where(m => m.TipoMovimiento == "Entrada"
						&& m.ProductosIdProductos.HasValue
						&& m.ProductosIdProductosNavigation.InventarioIdInventario == id) // ✅ filtro agregado
					.Select(m => m.ProductosIdProductosNavigation)
					.Distinct()
					.ToListAsync();

				if (productosConMovimiento == null || !productosConMovimiento.Any())
				{
					return Ok(new InventarioDto
					{
						IdInventario = id,
						FechaIngreso = inventarioFisico.FechaIngreso,
						Productos = new List<ProductoDto>()
					});
				}

				// 2. Crear el DTO de Inventario usando el ID y la fecha del registro físico
				var inventarioDto = new InventarioDto
				{
					IdInventario = inventarioFisico.IdInventario,
					FechaIngreso = inventarioFisico.FechaIngreso,

					// Mapeamos los Productos
					Productos = productosConMovimiento.Select(p => new ProductoDto
					{
						IdProductos = p.IdProductos,
						Nombre = p.Nombre,
						Precio = p.Precio,
						Categoria = p.Categoria,
						Stock = p.Stock, // El stock actual está en la tabla Producto
						InventarioIdInventario = p.InventarioIdInventario
					}).ToList()
				};

				return Ok(inventarioDto);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { status = 500, message = "Error al obtener el inventario.", detail = ex.Message });
			}
		}
		[HttpPost]
		public async Task<ActionResult<InventarioDto>> PostInventario(CreateInventarioDto dto)
		{
			// 1. Validación de datos básicos

			try
			{
				var inventario = new Inventario
				{
					FechaIngreso = dto.FechaIngreso ?? DateTime.Now,

				};

				_context.Inventarios.Add(inventario);
				await _context.SaveChangesAsync();

				return CreatedAtAction(nameof(GetInventario), new { id = inventario.IdInventario }, new InventarioDto
				{
					IdInventario = inventario.IdInventario,
					FechaIngreso = inventario.FechaIngreso,
					Productos = new List<ProductoDto>()
				});
			}
			catch (DbUpdateException ex)
			{
				return BadRequest(new { status = 400, message = ex.InnerException?.Message ?? ex.Message, detail = "Error al guardar en la base de datos." });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { status = 500, message = ex.Message, detail = "Error inesperado al crear el registro de inventario." });
			}
		}

		[HttpPost("AgregarProducto")]
		public async Task<IActionResult> AgregarProductoAInventario([FromBody] CreateProductoInventarioDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			using var transaction = await _context.Database.BeginTransactionAsync();

			try
			{
				// 1️⃣ Obtener ID del usuario desde el token JWT (si aplica)
				int? usuarioId = null;
				var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
				if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedId))
					usuarioId = parsedId;

				// 2️⃣ Crear el nuevo producto
				var nuevoProducto = new Producto
				{
					Nombre = dto.Nombre,
					Precio = (double)dto.Precio,
					Categoria = dto.Categoria,
					Stock = dto.StockInicial
				};

				// Si el DTO tiene un InventarioId, lo asignamos
				nuevoProducto.InventarioIdInventario = dto.InventariosIdInventario;


				_context.Productos.Add(nuevoProducto);
				await _context.SaveChangesAsync();

				// 3️⃣ Registrar el movimiento de entrada
				var nuevoMovimiento = new MovimientoInventario
				{
					FechaMovimiento = DateTime.Now,
					TipoMovimiento = "Entrada",
					Cantidad = dto.StockInicial,
					Referencia = "Ingreso inicial",
					ProductosIdProductos = nuevoProducto.IdProductos,
					UsuariosIdUsuarios = usuarioId ?? 1 // Si no hay usuario, por defecto 1
				};

				_context.MovimientosInventario.Add(nuevoMovimiento);
				await _context.SaveChangesAsync();

				await transaction.CommitAsync();

				// 4️⃣ Respuesta
				return CreatedAtAction(nameof(AgregarProductoAInventario), new { id = nuevoProducto.IdProductos }, new
				{
					message = "✅ Producto agregado correctamente al inventario.",
					producto = new
					{
						nuevoProducto.IdProductos,
						nuevoProducto.Nombre,
						nuevoProducto.Precio,
						nuevoProducto.Categoria,
						nuevoProducto.Stock
					}
				});
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				return StatusCode(500, new
				{
					message = "❌ Error al crear el producto y registrar el movimiento de entrada.",
					detail = ex.Message
				});
			}
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> PutInventario(int id, CreateInventarioDto dto)
		{
			if (id <= 0)
				return BadRequest(new { status = 400, message = "El ID debe ser mayor a cero." });

			
			
			try
			{
				var inventario = await _context.Inventarios.FindAsync(id);
				if (inventario == null)
					return NotFound(new { status = 404, message = $"No se encontró ningún registro de inventario con el ID {id}." });

				inventario.FechaIngreso = dto.FechaIngreso ?? inventario.FechaIngreso;


				_context.Entry(inventario).State = EntityState.Modified;
				await _context.SaveChangesAsync();

				return NoContent();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!InventarioExists(id))
				{
					return NotFound(new { status = 404, message = $"El registro fue eliminado por otro usuario." });
				}
				else
				{
					return BadRequest(new { status = 409, message = "Error de concurrencia: El registro fue modificado por otro usuario. Recargue los datos.", detail = "DbUpdateConcurrencyException" });
				}
			}
			catch (DbUpdateException ex)
			{
				return BadRequest(new { status = 400, message = ex.InnerException?.Message ?? ex.Message, detail = "Error al actualizar la base de datos." });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { status = 500, message = ex.Message, detail = "Error inesperado al actualizar el registro de inventario." });
			}
		}


			[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteInventario(int id)
		{
			if (id <= 0)
				return BadRequest(new { status = 400, message = "El ID debe ser mayor a cero." });

			try
			{
				var inventario = await _context.Inventarios.FindAsync(id);
				if (inventario == null)
					return NotFound(new { status = 404, message = $"No se encontró ningún registro de inventario con el ID {id}." });

				_context.Inventarios.Remove(inventario);
				await _context.SaveChangesAsync();

				return NoContent();
			}
			catch (DbUpdateException ex) when (ex.InnerException?.Message?.Contains("FOREIGN KEY") == true)
			{
				return BadRequest(new { status = 400, message = "No se puede eliminar el registro de inventario porque está asociado a otra tabla (ej. DetalleVenta).", detail = ex.InnerException.Message });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { status = 500, message = ex.Message, detail = "Error inesperado al eliminar el registro de inventario." });
			}
		}
	}
}

