using System;
using System.ComponentModel.DataAnnotations;

namespace Models;

public class Usuario
{
	[Key]
	public int Id { get; set; }

	[Required]
	[MaxLength(50)]
	public string NombreUsuario { get; set; } = string.Empty;

	[Required]
	public string PasswordHash { get; set; } = string.Empty;

	[Required]
	public string Salt { get; set; } = string.Empty;

	[Required]
	[MaxLength(200)]
	public string NombreCompleto { get; set; } = string.Empty;

	[Required]
	[MaxLength(20)]
	public string Rol { get; set; } = "Operador";

	public bool Activo { get; set; } = true;

	public DateTime FechaCreacion { get; set; } = DateTime.Now;
}
