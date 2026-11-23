using Models;

namespace Helpers;

public static class SesionActual
{
	public static Usuario? Usuario { get; set; }

	public static bool EsAdministrador => Usuario?.Rol == "Administrador";

	public static bool EsOperador => Usuario?.Rol == "Operador";

	public static string NombreUsuario => Usuario?.NombreUsuario ?? "---";
}
