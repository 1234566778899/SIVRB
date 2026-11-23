using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Data;
using Models;

namespace Services
{
    public class DatabaseService
    {
        public static void Initialize()
        {
            using var db = new AppDbContext();
            db.Database.EnsureCreated();
        }

        // ===== PERSONAS =====

        public async Task<List<Persona>> ObtenerTodasPersonas()
        {
            using var db = new AppDbContext();
            return await db.Personas
                .Include(p => p.Requisitoria)
                .OrderByDescending(p => p.FechaRegistro)
                .ToListAsync();
        }

        public async Task<Persona?> BuscarPorDNI(string dni)
        {
            using var db = new AppDbContext();
            return await db.Personas
                .Include(p => p.Requisitoria)
                .FirstOrDefaultAsync(p => p.DNI == dni);
        }

        public async Task<Persona?> ObtenerPersonaPorId(int id)
        {
            using var db = new AppDbContext();
            return await db.Personas
                .Include(p => p.Requisitoria)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Persona>> ObtenerPersonasConHuella()
        {
            using var db = new AppDbContext();
            return await db.Personas
                .Include(p => p.Requisitoria)
                .Where(p => p.HuellaTemplate != null)
                .ToListAsync();
        }

        public async Task<List<Persona>> ObtenerPersonasConRequisitoria()
        {
            using var db = new AppDbContext();
            return await db.Personas
                .Include(p => p.Requisitoria)
                .Where(p => p.Requisitoria != null && p.Requisitoria.Activa)
                .ToListAsync();
        }

        public async Task<Persona> RegistrarPersona(Persona persona)
        {
            using var db = new AppDbContext();
            db.Personas.Add(persona);
            await db.SaveChangesAsync();
            return persona;
        }

        public async Task ActualizarPersona(Persona persona)
        {
            using var db = new AppDbContext();
            var existente = await db.Personas.FindAsync(persona.Id);
            if (existente != null)
            {
                existente.DNI = persona.DNI;
                existente.NombreCompleto = persona.NombreCompleto;
                existente.Direccion = persona.Direccion;
                existente.Telefono = persona.Telefono;
                existente.MotivoRegistro = persona.MotivoRegistro;
                existente.Observaciones = persona.Observaciones;
                await db.SaveChangesAsync();
            }
        }

        public async Task ActualizarHuellaPersona(int personaId, byte[] huellaTemplate, int huellaTemplateSize)
        {
            using var db = new AppDbContext();
            var existente = await db.Personas.FindAsync(personaId);
            if (existente != null)
            {
                existente.HuellaTemplate = huellaTemplate;
                existente.HuellaTemplateSize = huellaTemplateSize;
                await db.SaveChangesAsync();
            }
        }

        public async Task EliminarPersona(int personaId)
        {
            using var db = new AppDbContext();

            var verificaciones = await db.VerificacionLogs
                .Where(v => v.PersonaId == personaId)
                .ToListAsync();
            db.VerificacionLogs.RemoveRange(verificaciones);

            var requisitoria = await db.Requisitorias
                .FirstOrDefaultAsync(r => r.PersonaId == personaId);
            if (requisitoria != null)
                db.Requisitorias.Remove(requisitoria);

            var persona = await db.Personas.FindAsync(personaId);
            if (persona != null)
            {
                db.Personas.Remove(persona);
                await db.SaveChangesAsync();
            }
        }

        // ===== REQUISITORIAS =====

        public async Task<List<Requisitoria>> ObtenerTodasRequisitorias()
        {
            using var db = new AppDbContext();
            return await db.Requisitorias
                .Include(r => r.Persona)
                .OrderByDescending(r => r.FechaEmision)
                .ToListAsync();
        }

        public async Task<Requisitoria> RegistrarRequisitoria(Requisitoria requisitoria)
        {
            using var db = new AppDbContext();
            db.Requisitorias.Add(requisitoria);
            await db.SaveChangesAsync();
            return requisitoria;
        }

        public async Task ActualizarRequisitoria(Requisitoria requisitoria)
        {
            using var db = new AppDbContext();
            var existente = await db.Requisitorias.FindAsync(requisitoria.Id);
            if (existente != null)
            {
                existente.Delito = requisitoria.Delito;
                existente.Juzgado = requisitoria.Juzgado;
                existente.NumeroExpediente = requisitoria.NumeroExpediente;
                existente.FechaEmision = requisitoria.FechaEmision;
                existente.Activa = requisitoria.Activa;
                existente.Descripcion = requisitoria.Descripcion;
                await db.SaveChangesAsync();
            }
        }

        public async Task DesactivarRequisitoria(int requisitoriaId)
        {
            using var db = new AppDbContext();
            var req = await db.Requisitorias.FindAsync(requisitoriaId);
            if (req != null)
            {
                req.Activa = false;
                await db.SaveChangesAsync();
            }
        }

        public async Task EliminarRequisitoria(int requisitoriaId)
        {
            using var db = new AppDbContext();
            var req = await db.Requisitorias.FindAsync(requisitoriaId);
            if (req != null)
            {
                db.Requisitorias.Remove(req);
                await db.SaveChangesAsync();
            }
        }

        // ===== VERIFICACIONES / HISTORIAL =====

        public async Task RegistrarVerificacion(VerificacionLog log)
        {
            using var db = new AppDbContext();
            db.VerificacionLogs.Add(log);
            await db.SaveChangesAsync();
        }

        public async Task<List<VerificacionLog>> ObtenerHistorialVerificaciones()
        {
            using var db = new AppDbContext();
            return await db.VerificacionLogs
                .Include(v => v.Persona)
                .OrderByDescending(v => v.FechaVerificacion)
                .Take(500)
                .ToListAsync();
        }

        public async Task<List<VerificacionLog>> ObtenerHistorialPorFecha(DateTime desde, DateTime hasta)
        {
            using var db = new AppDbContext();
            return await db.VerificacionLogs
                .Include(v => v.Persona)
                .Where(v => v.FechaVerificacion.Date >= desde.Date
                         && v.FechaVerificacion.Date <= hasta.Date)
                .OrderByDescending(v => v.FechaVerificacion)
                .ToListAsync();
        }

        public async Task<int> ContarVerificacionesHoy()
        {
            using var db = new AppDbContext();
            var hoy = DateTime.Today;
            return await db.VerificacionLogs
                .CountAsync(v => v.FechaVerificacion.Date == hoy);
        }

        public async Task<int> ContarCapturas()
        {
            using var db = new AppDbContext();
            var hoy = DateTime.Today;
            return await db.VerificacionLogs
                .CountAsync(v => v.FechaVerificacion.Date == hoy
                    && v.Resultado == "Coincidencia"
                    && v.Persona != null
                    && v.Persona.Requisitoria != null);
        }

        // ===== ESTADISTICAS =====

        public async Task<int> ContarRegistrados()
        {
            using var db = new AppDbContext();
            return await db.Personas.CountAsync();
        }

        public async Task<int> ContarConRequisitoria()
        {
            using var db = new AppDbContext();
            return await db.Requisitorias.CountAsync(r => r.Activa);
        }
    }
}