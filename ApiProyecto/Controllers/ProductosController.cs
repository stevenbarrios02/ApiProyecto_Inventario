using ApiProyecto.Dtos;
using ApiProyecto.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiProyecto.Controllers
{

	[Route("api/[controller]")]
	[ApiController]
	public class ProductosController : ControllerBase
	{
		private readonly EmpresaDbContext _context;

		public ProductosController(EmpresaDbContext context)
		{
			_context = context;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<ProductoDto>>> GetProductos()
		{
			if(_context.Productos == null)
			{
				throw new Exception("No se puede acceder a la base de datos.");
			}
			var productos = await _context.Productos
				.Select(p => new ProductoDto
				{
					IdProductos = p.IdProductos,
					Nombre = p.Nombre ?? "",
					Precio = (double)p.Precio,
					Categoria = p.Categoria ?? "",
					Stock = p.Stock
				})
				.ToListAsync();

			if(productos == null || !productos.Any())
			{
				throw new KeyNotFoundException("No se encontraron personas registradas.");
			}

			return Ok(productos);
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<ProductoDto>> GetProducto(int id)
		{
			if (id <= 0)
			{
				throw new ArgumentException("El ID proporcionado no es válido.");
			}

			if (_context.Productos == null)
			{
				throw new Exception("No se puede acceder a la base de datos.");
			}

			var producto = await _context.Productos
				.Where(p => p.IdProductos == id)
				.Select(p => new ProductoDto
				{
					IdProductos = p.IdProductos,
					Nombre = p.Nombre ?? "",
					Precio = (double)p.Precio,
					Categoria = p.Categoria ?? "",
					Stock= p.Stock
				})
				.FirstOrDefaultAsync();

			if (producto == null)
			{
				throw new KeyNotFoundException($"No se encontró ninguna persona con el ID {id}.");
			}
			return Ok(producto);
		}

		[HttpPost]
		public async Task<ActionResult<ProductoDto>> PostProducto(CreateProductoDto dto)
		{
			if (string.IsNullOrWhiteSpace(dto.Nombre) ||
		string.IsNullOrWhiteSpace(dto.Categoria) ||
		dto.Precio <= 0)
			{
				return BadRequest(new
				{
					status = 400,
					message = "Nombre, Categoría y Precio son obligatorios y el precio debe ser mayor a cero.",
					detail = "Revisa los datos enviados."
				});
			}

			if (dto.InventarioIdInventario != null)
			{
				var inventarioExiste = await _context.Inventarios
					.AnyAsync(i => i.IdInventario == dto.InventarioIdInventario);
				if (!inventarioExiste)
					return BadRequest(new { status = 400, message = "El inventario indicado no existe." });
			}

			var producto = new Producto
			{
				Nombre = dto.Nombre,
				Precio = dto.Precio,
				Categoria = dto.Categoria,
				Stock = dto.Stock,
				InventarioIdInventario = dto.InventarioIdInventario
			};

			_context.Productos.Add(producto);

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateException ex)
			{
				return BadRequest(new
				{
					status = 400,
					message = ex.InnerException?.Message ?? ex.Message,
					detail = "Error al guardar en la base de datos."
				});
			}

			return CreatedAtAction(nameof(GetProducto), new { id = producto.IdProductos }, new ProductoDto
			{
				IdProductos = producto.IdProductos,
				Nombre = producto.Nombre,
				Precio = producto.Precio,
				Stock= producto.Stock,
				Categoria = producto.Categoria,
			});
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> PutProducto(int id, CreateProductoDto dto)
		{
			if (string.IsNullOrWhiteSpace(dto.Nombre) ||
		string.IsNullOrWhiteSpace(dto.Categoria) ||
		dto.Precio <= 0)
			{
				return BadRequest(new { status = 400, message = "Nombre, Categoría y Precio son obligatorios y el precio debe ser mayor a cero." });
			}

			var producto = await _context.Productos.FindAsync(id);
			if (producto == null)
				return NotFound($"No se encontró ningún producto con el ID {id}.");

			if (dto.InventarioIdInventario != null)
			{
				var inventarioExiste = await _context.Inventarios
					.AnyAsync(i => i.IdInventario == dto.InventarioIdInventario);
				if (!inventarioExiste)
					return BadRequest(new { status = 400, message = "El inventario indicado no existe." });
			}

			producto.Nombre = dto.Nombre;
			producto.Precio = dto.Precio;
			producto.Categoria = dto.Categoria;
			producto.Stock = dto.Stock;
			producto.InventarioIdInventario = dto.InventarioIdInventario;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateException ex)
			{
				return BadRequest(new { status = 400, message = ex.InnerException?.Message ?? ex.Message });
			}

			return NoContent();
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteProducto(int id)
		{
			if (id <= 0)
				throw new ArgumentException("El ID proporcionado no es válido. Debe ser mayor a cero.");

			var producto = await _context.Productos.FindAsync(id);
			if (producto == null) 
				throw new KeyNotFoundException($"No se encontró ninguna persona con el ID {id}.");

			_context.Productos.Remove(producto);
			await _context.SaveChangesAsync();

			return NoContent();
		}
	}
}

