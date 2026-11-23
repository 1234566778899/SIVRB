using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Data;

public class AppDbContext : DbContext
{
	public DbSet<Persona> Personas { get; set; }

	public DbSet<Requisitoria> Requisitorias { get; set; }

	public DbSet<VerificacionLog> VerificacionLogs { get; set; }

	public DbSet<Usuario> Usuarios { get; set; }

	protected override void OnConfiguring(DbContextOptionsBuilder options)
	{
		string text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sivrb.db");
		options.UseSqlite("Data Source=" + text);
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Persona>().HasIndex((Persona p) => p.DNI).IsUnique();
		modelBuilder.Entity<Persona>().HasOne((Persona p) => p.Requisitoria).WithOne((Requisitoria r) => r.Persona)
			.HasForeignKey((Requisitoria r) => r.PersonaId);
		modelBuilder.Entity<Usuario>().HasIndex((Usuario u) => u.NombreUsuario).IsUnique();
	}

	public void SeedAdministrador()
	{
		Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS Usuarios (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    NombreUsuario TEXT NOT NULL,
                    PasswordHash TEXT NOT NULL,
                    Salt TEXT NOT NULL,
                    NombreCompleto TEXT NOT NULL,
                    Rol TEXT NOT NULL,
                    Activo INTEGER NOT NULL DEFAULT 1,
                    FechaCreacion TEXT NOT NULL
                )");
		Database.ExecuteSqlRaw(@"
                CREATE UNIQUE INDEX IF NOT EXISTS IX_Usuarios_NombreUsuario
                ON Usuarios (NombreUsuario)");
		Database.ExecuteSqlRaw("UPDATE Usuarios SET Rol = 'Administrador' WHERE Rol = 'SuperAdmin'");
		if (!Usuarios.Any((Usuario u) => u.NombreUsuario == "admin"))
		{
			string salt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
			string passwordHash = HashPassword("Dq23PbHt", salt);
			Usuarios.Add(new Usuario
			{
				NombreUsuario = "admin",
				PasswordHash = passwordHash,
				Salt = salt,
				NombreCompleto = "Administrador",
				Rol = "Administrador",
				Activo = true,
				FechaCreacion = DateTime.Now
			});
			SaveChanges();
		}
	}

	public static string HashPassword(string password, string salt)
	{
		return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(password + salt)));
	}
}
