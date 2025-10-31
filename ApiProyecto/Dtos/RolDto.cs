namespace ApiProyecto.Dtos
{
	public class RolDto
	{
		public int IdRoles { get; set; }
		public string Nombre { get; set; } = null!;
	}

	public class CreateRolDto
	{
		public string Nombre { get; set; } = null!;
	}
}

