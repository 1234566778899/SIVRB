using System;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class Persona
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(8)]
        public string DNI { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string NombreCompleto { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Direccion { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Telefono { get; set; } = string.Empty;

        [MaxLength(50)]
        public string MotivoRegistro { get; set; } = string.Empty;

        public byte[]? HuellaTemplate { get; set; }
        public int HuellaTemplateSize { get; set; }
        public int IndiceDedo { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [MaxLength(500)]
        public string Observaciones { get; set; } = string.Empty;

        public Requisitoria? Requisitoria { get; set; }
    }
}