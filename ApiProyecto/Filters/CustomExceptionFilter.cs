using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ApiProyecto.Filters
{
	public class CustomExceptionFilter : IExceptionFilter
	{
		private readonly ILogger<CustomExceptionFilter> _logger;

		public CustomExceptionFilter(ILogger<CustomExceptionFilter> logger)
		{
			_logger = logger;
		}

		public void OnException(ExceptionContext context)
		{
			// 1. Registrar la excepción completa para el desarrollador
			_logger.LogError(
				new EventId(context.Exception.HResult),
				context.Exception,
				context.Exception.Message);

			// 2. Definir la respuesta estándar
			var statusCode = (int)HttpStatusCode.InternalServerError;
			var message = "Error inesperado del servidor. Por favor, contacte al soporte.";

			// 3. Crear el objeto de respuesta limpio
			var result = new ObjectResult(new
			{
				status = statusCode,
				message = message,
				// Dejamos el 'detail' para el desarrollador, pero en producción se oculta
				detail = context.Exception.Message
			})
			{
				StatusCode = statusCode
			};

			// 4. Establecer el resultado
			context.Result = result;
			context.ExceptionHandled = true;
		}
	}
}
