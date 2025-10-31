using ApiProyecto.Dtos;
using ApiProyecto.Models;
using AutoMapper;

namespace ApiProyecto.Mappings
{
	public class ClasMappingProfiles
	{
		public class MappingProfiles : Profile
		{
			public MappingProfiles()
			{
				// === 1. Mapeo de ENTIDAD a DTO (Lectura: De la DB al Cliente) ===
				// Estos se usan en los métodos HTTP GET.

				CreateMap<Persona, PersonaDto>();
				CreateMap<Contrato, ContratoDto>();
				CreateMap<Usuario, UsuarioDto>();
				CreateMap<Producto, ProductoDto>();
				CreateMap<Inventario, InventarioDto>();
				CreateMap<Role, RolDto>();
				CreateMap<Venta, VentaDto>();
				// Asumo que tu entidad Detalleventum.cs se mapea a DetalleVentaDto
				CreateMap<Detalleventum, DetalleVentaDto>();


				// === 2. Mapeo de DTO a ENTIDAD (Escritura: Del Cliente a la DB) ===
				// Estos se usan en los métodos HTTP POST (Creación).

				// Importante: AutoMapper mapea el DTO a la entidad, listo para guardar.

				// DTOs de Creación (Asumiendo que existen)
				CreateMap<Usuario, UsuarioDto>()
				.ForMember(dest => dest.Persona, opt => opt.MapFrom(src => src.Persona))
				.ForMember(dest => dest.Rol, opt => opt.MapFrom(src => src.RolesIdRolesNavigation));

				CreateMap<CreateUsuarioDto, Usuario>()
					.ForMember(dest => dest.Contrasenia, opt => opt.Ignore()); ;

				CreateMap<Role, RolDto>();
				CreateMap<CreatePersonaDto, Persona>();
				CreateMap<CreateContratoDto, Contrato>();
				CreateMap<CreateProductoDto, Producto>();
				CreateMap<CreateInventarioDto, Inventario>();
				CreateMap<CreateMovimientoInventarioDto, MovimientoInventario>();

				// 🔑 Mapeo de RESPUESTA (necesario para el GET y CreatedAtAction)
				CreateMap<MovimientoInventario, MovimientoInventarioDto>();

				CreateMap<Detalleventum, DetalleVentaDto>()
				.ForMember(dest => dest.Producto,
				opt => opt.MapFrom(src => src.ProductosIdProductosNavigation))
				.ForMember(dest => dest.Venta,
				opt => opt.MapFrom(src => src.VentasIdVentasNavigation));

				CreateMap<CreateDetalleVentaDto, Detalleventum>()
	// Ignoramos PrecioTotal, ya que tu Controller lo calcula manualmente.
				.ForMember(dest => dest.PrecioTotal, opt => opt.Ignore());

				CreateMap<Venta, VentaDto>()
					.ForMember(dest => dest.Usuario,
						opt => opt.MapFrom(src => src.UsuariosIdUsuariosNavigation));


				// 2. 🔑 REGLA DE CREACIÓN (POST) - Para resolver el error 500 "Missing type map configuration".
				CreateMap<CreateVentaDto, Venta>()
					// Ignoramos la Fecha y Total para que tu Controller maneje la lógica de negocio.
					.ForMember(dest => dest.Fecha, opt => opt.Ignore())
					// Nota: Total y UsuariosIdUsuarios se mapean automáticamente si los nombres coinciden.
					.ForMember(dest => dest.Total, opt => opt.Ignore());


				// === 3. Mapeo para Actualización (PATCH/PUT) ===
				// Usaremos el mismo DTO de Creación para simplificar por ahora, o crear un UpdateDto
				// Ejemplo de mapeo para actualización (si usas un UpdatePersonaDto)
				// CreateMap<UpdatePersonaDto, Persona>();
			}
		}
	}
}
