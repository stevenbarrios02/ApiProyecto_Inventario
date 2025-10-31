using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace ApiProyecto.Models;

public partial class EmpresaDbContext : DbContext
{
    public EmpresaDbContext()
    {
    }

    public EmpresaDbContext(DbContextOptions<EmpresaDbContext> options)
        : base(options)
    {
    }

	public override int SaveChanges()
	{
		OnBeforeSaving();
		return base.SaveChanges();
	}

	public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		OnBeforeSaving();
		return base.SaveChangesAsync(cancellationToken);
	}

	private void OnBeforeSaving()
	{
		// Iterar sobre todas las entidades que han cambiado de estado
		foreach (var entry in ChangeTracker.Entries())
		{
			// Solo nos interesa la lógica de Soft Delete si el estado es 'Deleted'
			if (entry.State == EntityState.Deleted)
			{
				// Verificar si la entidad implementa nuestra interfaz ISoftDelete
				if (entry.Entity is ISoftDelete softDeleteEntity)
				{
					// 1. Cambiar el estado de la entidad de 'Deleted' a 'Modified' (Update)
					entry.State = EntityState.Modified;

					// 2. Establecer el flag de eliminación a true
					softDeleteEntity.EstaEliminado = true;

					// Opcional: Si tienes una propiedad de fecha, actualízala aquí
					// softDeleteEntity.FechaEliminacion = DateTime.Now; 
				}
			}
		}
	}

	public virtual DbSet<Contrato> Contratos { get; set; }

    public virtual DbSet<Detalleventum> Detalleventa { get; set; }

    public virtual DbSet<Inventario> Inventarios { get; set; }

    public virtual DbSet<Persona> Personas { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Venta> Ventas { get; set; }
	public virtual DbSet<MovimientoInventario> MovimientosInventario { get; set; }
	public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;port=3306;database=empresadb;user=root;password=1234", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.4.3-mysql"));

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		foreach (var entityType in modelBuilder.Model.GetEntityTypes())
		{
			if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
			{
				// 🔑 SOLUCIÓN ROBUSTA para CS8917: Construcción manual del árbol de expresión
				var parameter = Expression.Parameter(entityType.ClrType, "entity");

				// 1. Llama a EF.Property<bool>(entity, "EstaEliminado")
				var propertyMethodInfo = typeof(EF).GetMethod("Property")
					.MakeGenericMethod(typeof(bool));
				var isDeletedProperty = Expression.Call(propertyMethodInfo, parameter, Expression.Constant("EstaEliminado"));

				// 2. Aplica la negación: !isDeletedProperty (es decir, EstaEliminado == false)
				var filterExpression = Expression.Lambda(Expression.Not(isDeletedProperty), parameter);

				// 3. Aplica el filtro con la expresión construida
				modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filterExpression);

				// Asegurar valor por defecto
				modelBuilder.Entity(entityType.ClrType)
					.Property<bool>("EstaEliminado")
					.HasDefaultValue(false);
			}
		}

		modelBuilder
			.UseCollation("utf8mb4_0900_ai_ci")
			.HasCharSet("utf8mb4");

		modelBuilder.Entity<Contrato>(entity =>
		{
			entity.HasKey(e => e.IdContratos).HasName("PRIMARY");
			entity.HasOne(d => d.PersonasIdPersonasNavigation).WithMany(p => p.Contratos).HasConstraintName("contratos_ibfk_1");
		});

		modelBuilder.Entity<Detalleventum>(entity =>
		{
			entity.HasKey(e => e.IdDetalleVenta).HasName("PRIMARY");


			entity.HasOne(d => d.ProductosIdProductosNavigation).WithMany(p => p.Detalleventa).HasConstraintName("detalleventa_ibfk_2");

			entity.HasOne(d => d.VentasIdVentasNavigation).WithMany(p => p.Detalleventa).HasConstraintName("detalleventa_ibfk_1");
		});

		modelBuilder.Entity<Inventario>(entity =>
		{
			entity.HasKey(e => e.IdInventario).HasName("PRIMARY");
		});

	
		modelBuilder.Entity<Persona>(entity =>
		{
			entity.HasKey(e => e.IdPersonas).HasName("PRIMARY");

		});


		modelBuilder.Entity<Producto>(entity =>
		{
			entity.HasKey(e => e.IdProductos).HasName("PRIMARY");
	
			entity.HasOne(d => d.Inventario).WithMany(p => p.Productos).HasConstraintName("productos_ibfk_1");
		});


		modelBuilder.Entity<Role>(entity =>
		{
			entity.HasKey(e => e.IdRoles).HasName("PRIMARY");
		});

		modelBuilder.Entity<Usuario>(entity =>
		{
			entity.HasKey(e => e.IdUsuarios).HasName("PRIMARY");

			entity.HasOne(d => d.Persona)
				.WithOne(p => p.Usuario)
				.HasForeignKey<Usuario>(d => d.PersonasIdPersonas)
				.IsRequired()
				.HasConstraintName("fk_Usuarios_Personas");


			entity.HasOne(d => d.RolesIdRolesNavigation).WithMany(p => p.Usuarios).HasConstraintName("usuarios_ibfk_1");
		});

		modelBuilder.Entity<Venta>(entity =>
		{
			entity.HasKey(e => e.IdVentas).HasName("PRIMARY");
			entity.HasOne(d => d.UsuariosIdUsuariosNavigation).WithMany(p => p.Ventas).HasConstraintName("ventas_ibfk_1");


		});
		var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
		v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
		v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

		modelBuilder.Entity<RefreshToken>()
		.Property(rt => rt.Expires)
		.HasConversion(dateTimeConverter);

		modelBuilder.Entity<RefreshToken>()
			.Property(rt => rt.Created)
			.HasConversion(dateTimeConverter);

		modelBuilder.Entity<RefreshToken>()
		.HasOne(rt => rt.UsuariosIdUsuariosNavigation)
		.WithMany(u => u.RefreshTokens)
		.HasForeignKey(rt => rt.UsuariosIdUsuarios)
		.IsRequired(false);

		OnModelCreatingPartial(modelBuilder);
	}

	partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
