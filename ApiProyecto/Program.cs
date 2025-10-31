
using ApiProyecto.Filters;
using ApiProyecto.Middlewares;
using ApiProyecto.Models;
using ApiProyecto.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Text;

namespace ApiProyecto
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			var jwtSettings = builder.Configuration.GetSection("Jwt");
			var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

			builder.Services.AddAutoMapper(typeof(Program));
			// 5. Añadir servicios de autenticación JWT
			builder.Services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			})
			.AddJwtBearer(options =>
			{
				options.RequireHttpsMetadata = false; // En desarrollo podemos usar HTTP
				options.SaveToken = true;
				var jwtSettings = builder.Configuration.GetSection("Jwt");
				var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);

				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuerSigningKey = true,

					// 🔑 CORRECCIÓN: Usamos 'key' (que ya está como array de bytes)
					IssuerSigningKey = new SymmetricSecurityKey(key),

					// ValidateIssuerSigningKey = true es suficiente, no necesitamos el 'IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8...)' duplicado.

					ValidateIssuer = true,
					// 🔑 USAMOS LA CONFIGURACIÓN CARGADA
					ValidIssuer = jwtSettings["Issuer"],

					ValidateAudience = true,
					// 🔑 USAMOS LA CONFIGURACIÓN CARGADA
					ValidAudience = jwtSettings["Audience"],

					ValidateLifetime = true,
					ClockSkew = TimeSpan.Zero
				};
			});
			builder.Services.AddScoped<IAuthServices, AuthService>();
			// Registrar el DbContext
			builder.Services.AddDbContext<EmpresaDbContext>(options =>
			options.UseMySql(builder.Configuration.GetConnectionString("MySqlConnection"),
				ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySqlConnection"))));

			// Add services to the container.
			builder.Services.AddCors(options =>
			{
				options.AddPolicy("AllowAll",
					builder => builder
						.AllowAnyOrigin()    // Permite todos los orígenes (para desarrollo)
						.AllowAnyMethod()    // Permite GET, POST, PUT, DELETE, etc.
						.AllowAnyHeader());  // Permite todos los encabezados
			});
			builder.Services.AddControllers();
			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddControllers(options =>
			{
				// 🔑 Aplicar el filtro a TODOS los controllers
				options.Filters.Add<CustomExceptionFilter>();
			});
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen(options =>
			{
				// 1. Definición de Seguridad: Creamos el esquema JWT
				options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
				{
					Name = "Authorization",
					Type = SecuritySchemeType.Http,
					Scheme = "bearer", // 🔑 ESTO ES CRUCIAL: Define el prefijo como 'Bearer '
					BearerFormat = "JWT",
					In = ParameterLocation.Header,
					Description = "Autorización JWT (Pegar solo el token)"
				});

				// 2. Requisito de Seguridad: Aplicar el esquema a todas las operaciones
				options.AddSecurityRequirement(new OpenApiSecurityRequirement
			{
				{
				new OpenApiSecurityScheme
				{
					Reference = new OpenApiReference
					{
					Type = ReferenceType.SecurityScheme,
					Id = "Bearer" // Debe coincidir con el nombre de la definición anterior
					}
				},
						new string[] {}
				}
			});

				// Si usaste SecurityRequirementsOperationFilter, coméntalo o elimínalo.
				// options.OperationFilter<SecurityRequirementsOperationFilter>(); 
			});
			builder.Services.AddSwaggerGen();

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}
			app.UseMiddleware<ErrorHandlerMiddleware>();

			app.UseHttpsRedirection();
			app.UseCors("AllowAll");
			app.UseAuthentication();
			app.UseAuthorization();


			app.MapControllers();

			app.Run();
		}
	}
}
