using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class VerificacionLog
    {
        [Key]
        public int Id { get; set; }

        public int? PersonaId { get; set; }

        [ForeignKey("PersonaId")]
        public Persona? Persona { get; set; }

        public DateTime FechaVerificacion { get; set; } = DateTime.Now;

        [MaxLength(50)]
        public string Resultado { get; set; } = string.Empty;

        public double PorcentajeCoincidencia { get; set; }

        [MaxLength(100)]
        public string OperadorPNP { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Observaciones { get; set; } = string.Empty;
    }
}