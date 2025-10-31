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
					.Select(i => new InventarioDto
					{
						IdInventario = i.IdInventario,
						FechaIngreso = i.FechaIngreso,
						Productos = i.Productos.Select(p => new ProductoDto
						{
							IdProductos = p.IdProductos,
							Nombre = p.Nombre,
							Precio = p.Precio,
							Categoria = p.Categoria,
							Stock = p.Stock,
						}).ToList()
					})
					.ToListAsync();

				if (inventarios == null || !inventarios.Any())
					return Ok(new List<InventarioDto>());

				return Ok(inventarios);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { status = 500, message = ex.Message, detail = "Error al obtener los registros de inventario." });
			}
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<InventarioDto>> GetInventario(int id)
		{
			if (id <= 0)
				return BadRequest(new { status = 400, message = "El ID debe ser mayor a cero." });

			try
			{
				var inventario = await _context.Inventarios
					.Where(i => i.IdInventario == id)
					.Include(i => i.Productos)
					.Select(i => new InventarioDto
					{
						IdInventario = i.IdInventario,
						FechaIngreso = i.FechaIngreso,
						Productos = i.Productos.Select(p => new ProductoDto
						{
							IdProductos = p.IdProductos,
							Nombre = p.Nombre,
							Precio = p.Precio,
							Categoria = p.Categoria,
							Stock = p.Stock,
						}).ToList()
					})
					.FirstOrDefaultAsync();

				if (inventario == null)
					return NotFound(new { status = 404, message = $"No se encontró ningún registro de inventario con el ID {id}." });

				return Ok(inventario);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { status = 500, message = ex.Message, detail = "Error al buscar el registro de inventario." });
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

