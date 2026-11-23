using Microsoft.EntityFrameworkCore;
using Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Persona> Personas { get; set; }
        public DbSet<Requisitoria> Requisitorias { get; set; }
        public DbSet<VerificacionLog> VerificacionLogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // La base de datos se crea en la carpeta del ejecutable
            var dbPath = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "sivrb.db");
            options.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Indice unico para DNI (no puede haber dos personas con el mismo DNI)
            modelBuilder.Entity<Persona>()
                .HasIndex(p => p.DNI)
                .IsUnique();

            // Relacion Persona -> Requisitoria (1 a 1 o 1 a 0)
            modelBuilder.Entity<Persona>()
                .HasOne(p => p.Requisitoria)
                .WithOne(r => r.Persona)
                .HasForeignKey<Requisitoria>(r => r.PersonaId);
        }
    }
}