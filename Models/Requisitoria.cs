using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class Requisitoria
    {
        [Key]
        public int Id { get; set; }

        public int PersonaId { get; set; }

        [ForeignKey("PersonaId")]
        public Persona? Persona { get; set; }

        [Required]
        [MaxLength(200)]
        public string Delito { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Juzgado { get; set; } = string.Empty;

        [MaxLength(50)]
        public string NumeroExpediente { get; set; } = string.Empty;

        public DateTime FechaEmision { get; set; }

        public bool Activa { get; set; } = true;

        [MaxLength(500)]
        public string Descripcion { get; set; } = string.Empty;
    }
}