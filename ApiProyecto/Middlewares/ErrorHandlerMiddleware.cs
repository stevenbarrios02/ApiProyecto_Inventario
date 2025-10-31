using System.Net;
using System.Text.Json;

namespace ApiProyecto.Middlewares
{
	public class ErrorHandlerMiddleware
	{
		private readonly RequestDelegate _next;

		public ErrorHandlerMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			try
			{
				await _next(context);
			}
			catch (Exception ex)
			{
				await HandleExceptionAsync(context, ex);
			}
		}

		private static Task HandleExceptionAsync(HttpContext context, Exception exception)
		{
			var statusCode = exception switch
			{
				ArgumentNullException => (int)HttpStatusCode.BadRequest,
				KeyNotFoundException => (int)HttpStatusCode.NotFound,
				_ => (int)HttpStatusCode.InternalServerError
			};

			var response = new
			{
				status = statusCode,
				message = exception.Message,
				detail = "Ocurrió un error mientras se procesaba tu solicitud."
			};

			context.Response.ContentType = "application/json";
			context.Response.StatusCode = statusCode;

			var json = JsonSerializer.Serialize(response);
			return context.Response.WriteAsync(json);
		}
	}
}
