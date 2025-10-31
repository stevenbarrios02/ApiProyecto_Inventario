namespace ApiProyecto.Dtos
{
	public class ApiResultDto<T> where T : class
	{
		public bool Succeeded { get; set; } = true; // Siempre true si la llamada fue exitosa (2xx)
		public string Message { get; set; } = "Operación completada con éxito.";

		// 🔑 Contiene la lista de resultados
		public IEnumerable<T> Data { get; set; }

		// 🔑 Propiedades para futura paginación (opcional por ahora, pero útil)
		public int TotalCount { get; set; } = 0;
		public int PageSize { get; set; } = 0;
		public int PageIndex { get; set; } = 0;

		// Constructor para resultados exitosos
		public ApiResultDto(IEnumerable<T> data, int totalCount = 0)
		{
			Data = data;
			TotalCount = totalCount == 0 ? data.Count() : totalCount; // Si no se pasa el total, usa el count
		}

		// Constructor para errores específicos (aunque el Filtro Global maneja la mayoría de 400/500)
		public ApiResultDto(bool succeeded, string message)
		{
			Succeeded = succeeded;
			Message = message;
		}
	}
}
