using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Data;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Services;

public class DatabaseService
{
	public static void Initialize()
	{
		using AppDbContext appDbContext = new AppDbContext();
		appDbContext.Database.EnsureCreated();
		appDbContext.SeedAdministrador();
	}

	public async Task<List<Persona>> ObtenerTodasPersonas()
	{
		using AppDbContext db = new AppDbContext();
		return await (from p in db.Personas.Include((Persona p) => p.Requisitoria)
			orderby p.FechaRegistro descending
			select p).ToListAsync();
	}

	public async Task<Persona?> BuscarPorDNI(string dni)
	{
		using AppDbContext db = new AppDbContext();
		return await db.Personas.Include((Persona p) => p.Requisitoria).FirstOrDefaultAsync((Persona p) => p.DNI == dni);
	}

	public async Task<Persona?> ObtenerPersonaPorId(int id)
	{
		using AppDbContext db = new AppDbContext();
		return await db.Personas.Include((Persona p) => p.Requisitoria).FirstOrDefaultAsync((Persona p) => p.Id == id);
	}

	public async Task<List<Persona>> ObtenerPersonasConHuella()
	{
		using AppDbContext db = new AppDbContext();
		return await (from p in db.Personas.Include((Persona p) => p.Requisitoria)
			where p.HuellaTemplate != null
			select p).ToListAsync();
	}

	public async Task<List<Persona>> ObtenerPersonasConRequisitoria()
	{
		using AppDbContext db = new AppDbContext();
		return await (from p in db.Personas.Include((Persona p) => p.Requisitoria)
			where p.Requisitoria != null && p.Requisitoria.Activa
			select p).ToListAsync();
	}

	public async Task<Persona> RegistrarPersona(Persona persona)
	{
		using AppDbContext db = new AppDbContext();
		db.Personas.Add(persona);
		await db.SaveChangesAsync();
		return persona;
	}

	public async Task ActualizarPersona(Persona persona)
	{
		using AppDbContext db = new AppDbContext();
		Persona persona2 = await db.Personas.FindAsync(persona.Id);
		if (persona2 != null)
		{
			persona2.DNI = persona.DNI;
			persona2.NombreCompleto = persona.NombreCompleto;
			persona2.Direccion = persona.Direccion;
			persona2.Telefono = persona.Telefono;
			persona2.MotivoRegistro = persona.MotivoRegistro;
			persona2.Observaciones = persona.Observaciones;
			await db.SaveChangesAsync();
		}
	}

	public async Task ActualizarHuellaPersona(int personaId, byte[] huellaTemplate, int huellaTemplateSize)
	{
		using AppDbContext db = new AppDbContext();
		Persona persona = await db.Personas.FindAsync(personaId);
		if (persona != null)
		{
			persona.HuellaTemplate = huellaTemplate;
			persona.HuellaTemplateSize = huellaTemplateSize;
			await db.SaveChangesAsync();
		}
	}

	public async Task EliminarPersona(int personaId)
	{
		using AppDbContext db = new AppDbContext();
		List<VerificacionLog> entities = await db.VerificacionLogs.Where((VerificacionLog v) => v.PersonaId == (int?)personaId).ToListAsync();
		db.VerificacionLogs.RemoveRange(entities);
		Requisitoria requisitoria = await db.Requisitorias.FirstOrDefaultAsync((Requisitoria r) => r.PersonaId == personaId);
		if (requisitoria != null)
		{
			db.Requisitorias.Remove(requisitoria);
		}
		Persona persona = await db.Personas.FindAsync(personaId);
		if (persona != null)
		{
			db.Personas.Remove(persona);
			await db.SaveChangesAsync();
		}
	}

	public async Task<List<Requisitoria>> ObtenerTodasRequisitorias()
	{
		using AppDbContext db = new AppDbContext();
		return await (from r in db.Requisitorias.Include((Requisitoria r) => r.Persona)
			orderby r.FechaEmision descending
			select r).ToListAsync();
	}

	public async Task<Requisitoria> RegistrarRequisitoria(Requisitoria requisitoria)
	{
		using AppDbContext db = new AppDbContext();
		db.Requisitorias.Add(requisitoria);
		await db.SaveChangesAsync();
		return requisitoria;
	}

	public async Task ActualizarRequisitoria(Requisitoria requisitoria)
	{
		using AppDbContext db = new AppDbContext();
		Requisitoria requisitoria2 = await db.Requisitorias.FindAsync(requisitoria.Id);
		if (requisitoria2 != null)
		{
			requisitoria2.Delito = requisitoria.Delito;
			requisitoria2.Juzgado = requisitoria.Juzgado;
			requisitoria2.NumeroExpediente = requisitoria.NumeroExpediente;
			requisitoria2.FechaEmision = requisitoria.FechaEmision;
			requisitoria2.Activa = requisitoria.Activa;
			requisitoria2.Descripcion = requisitoria.Descripcion;
			await db.SaveChangesAsync();
		}
	}

	public async Task DesactivarRequisitoria(int requisitoriaId)
	{
		using AppDbContext db = new AppDbContext();
		Requisitoria requisitoria = await db.Requisitorias.FindAsync(requisitoriaId);
		if (requisitoria != null)
		{
			requisitoria.Activa = false;
			await db.SaveChangesAsync();
		}
	}

	public async Task EliminarRequisitoria(int requisitoriaId)
	{
		using AppDbContext db = new AppDbContext();
		Requisitoria requisitoria = await db.Requisitorias.FindAsync(requisitoriaId);
		if (requisitoria != null)
		{
			db.Requisitorias.Remove(requisitoria);
			await db.SaveChangesAsync();
		}
	}

	public async Task RegistrarVerificacion(VerificacionLog log)
	{
		using AppDbContext db = new AppDbContext();
		db.VerificacionLogs.Add(log);
		await db.SaveChangesAsync();
	}

	public async Task<List<VerificacionLog>> ObtenerHistorialVerificaciones()
	{
		using AppDbContext db = new AppDbContext();
		return await (from v in db.VerificacionLogs.Include((VerificacionLog v) => v.Persona)
			orderby v.FechaVerificacion descending
			select v).Take(500).ToListAsync();
	}

	public async Task<List<VerificacionLog>> ObtenerHistorialPorFecha(DateTime desde, DateTime hasta)
	{
		using AppDbContext db = new AppDbContext();
		return await (from v in db.VerificacionLogs.Include((VerificacionLog v) => v.Persona)
			where v.FechaVerificacion.Date >= ((DateTime)desde).Date && v.FechaVerificacion.Date <= ((DateTime)hasta).Date
			orderby v.FechaVerificacion descending
			select v).ToListAsync();
	}

	public async Task<int> ContarVerificacionesHoy()
	{
		using AppDbContext db = new AppDbContext();
		DateTime hoy = DateTime.Today;
		return await db.VerificacionLogs.CountAsync((VerificacionLog v) => v.FechaVerificacion.Date == hoy && v.PersonaId != (int?)null && v.Persona != null);
	}

	public async Task<int> ContarCapturas()
	{
		using AppDbContext db = new AppDbContext();
		DateTime hoy = DateTime.Today;
		return await db.VerificacionLogs.CountAsync((VerificacionLog v) => v.FechaVerificacion.Date == hoy && v.Resultado == "Coincidencia" && v.Persona != null && v.Persona.Requisitoria != null);
	}

	public async Task<Usuario?> AutenticarUsuario(string nombreUsuario, string password)
	{
		using AppDbContext db = new AppDbContext();
		Usuario usuario = await db.Usuarios.FirstOrDefaultAsync((Usuario u) => u.NombreUsuario == nombreUsuario && u.Activo);
		if (usuario == null)
		{
			return null;
		}
		return (AppDbContext.HashPassword(password, usuario.Salt) == usuario.PasswordHash) ? usuario : null;
	}

	public async Task<List<Usuario>> ObtenerTodosUsuarios()
	{
		using AppDbContext db = new AppDbContext();
		return await db.Usuarios.OrderBy((Usuario u) => u.NombreUsuario).ToListAsync();
	}

	public async Task<bool> CrearUsuario(string nombreUsuario, string password, string nombreCompleto, string rol)
	{
		using AppDbContext db = new AppDbContext();
		if (await db.Usuarios.AnyAsync((Usuario u) => u.NombreUsuario == nombreUsuario))
		{
			return false;
		}
		string salt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
		string passwordHash = AppDbContext.HashPassword(password, salt);
		db.Usuarios.Add(new Usuario
		{
			NombreUsuario = nombreUsuario,
			PasswordHash = passwordHash,
			Salt = salt,
			NombreCompleto = nombreCompleto,
			Rol = rol,
			Activo = true,
			FechaCreacion = DateTime.Now
		});
		await db.SaveChangesAsync();
		return true;
	}

	public async Task<bool> ActualizarUsuario(int id, string nombreCompleto, string rol, bool activo)
	{
		using AppDbContext db = new AppDbContext();
		Usuario usuario = await db.Usuarios.FindAsync(id);
		if (usuario == null)
		{
			return false;
		}
		usuario.NombreCompleto = nombreCompleto;
		usuario.Rol = rol;
		usuario.Activo = activo;
		await db.SaveChangesAsync();
		return true;
	}

	public async Task<bool> CambiarPassword(int id, string nuevaPassword)
	{
		using AppDbContext db = new AppDbContext();
		Usuario usuario = await db.Usuarios.FindAsync(id);
		if (usuario == null)
		{
			return false;
		}
		string salt = (usuario.Salt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)));
		usuario.PasswordHash = AppDbContext.HashPassword(nuevaPassword, salt);
		await db.SaveChangesAsync();
		return true;
	}

	public async Task<bool> EliminarUsuario(int id)
	{
		using AppDbContext db = new AppDbContext();
		Usuario usuario = await db.Usuarios.FindAsync(id);
		if (usuario == null || usuario.NombreUsuario == "admin")
		{
			return false;
		}
		db.Usuarios.Remove(usuario);
		await db.SaveChangesAsync();
		return true;
	}

	public async Task<List<(string Mes, int Cantidad)>> ObtenerRegistrosPorMes()
	{
		using AppDbContext db = new AppDbContext();
		DateTime dateTime = DateTime.Today.AddMonths(-5);
		DateTime inicio = new DateTime(dateTime.Year, dateTime.Month, 1);
		List<Persona> source = await db.Personas.Where((Persona p) => p.FechaRegistro >= inicio).ToListAsync();
		List<(string, int)> list = new List<(string, int)>();
		for (int num = 0; num < 6; num++)
		{
			DateTime mes = inicio.AddMonths(num);
			int item = Enumerable.Count(source, (Persona p) => p.FechaRegistro.Year == mes.Year && p.FechaRegistro.Month == mes.Month);
			list.Add((mes.ToString("MMM"), item));
		}
		return list;
	}

	public async Task<List<(string Dia, int Cantidad)>> ObtenerVerificacionesPorDia()
	{
		using AppDbContext db = new AppDbContext();
		DateTime hace7Dias = DateTime.Today.AddDays(-6.0);
		List<VerificacionLog> source = await db.VerificacionLogs.Where((VerificacionLog v) => v.FechaVerificacion.Date >= hace7Dias).ToListAsync();
		List<(string, int)> list = new List<(string, int)>();
		for (int num = 0; num < 7; num++)
		{
			DateTime dia = hace7Dias.AddDays(num);
			int item = Enumerable.Count(source, (VerificacionLog v) => v.FechaVerificacion.Date == dia.Date);
			list.Add((dia.ToString("ddd"), item));
		}
		return list;
	}

	public async Task<List<(string Motivo, int Cantidad)>> ObtenerRegistrosPorMotivo()
	{
		using AppDbContext db = new AppDbContext();
		return (from p in await db.Personas.ToListAsync()
			group p by (!string.IsNullOrEmpty(p.MotivoRegistro)) ? p.MotivoRegistro : "Sin motivo" into g
			select (Key: g.Key, g.Count()) into x
			orderby x.Item2 descending
			select x).Take(6).ToList();
	}

	public async Task<int> ContarRegistrados()
	{
		using AppDbContext db = new AppDbContext();
		return await db.Personas.CountAsync();
	}

	public async Task<int> ContarConRequisitoria()
	{
		using AppDbContext db = new AppDbContext();
		return await db.Requisitorias.CountAsync((Requisitoria r) => r.Activa);
	}
}
