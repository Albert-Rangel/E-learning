// Models/Grade.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_learning.Models
{
    public class Grade
    {
        [Key]
        public int GradeId { get; set; }

        [Required]
        public int StudentId { get; set; } // Foreign key to User (Student)

        [Required]
        public int CourseId { get; set; } // Foreign key to Course

        [Range(0.00, 20.00, ErrorMessage = "La nota del Lapso 1 debe estar entre 0 y 20.")]
        public decimal? Lapso1 { get; set; } // Nullable, ya que las notas pueden no estar ingresadas al inicio

        [Range(0.00, 20.00, ErrorMessage = "La nota del Lapso 2 debe estar entre 0 y 20.")]
        public decimal? Lapso2 { get; set; }

        [Range(0.00, 20.00, ErrorMessage = "La nota del Lapso 3 debe estar entre 0 y 20.")]
        public decimal? Lapso3 { get; set; }

        // AÑADIDO: Propiedades FinalGrade y LapsoTotal con rango 0-20
        [Range(0.00, 20.00, ErrorMessage = "La nota final debe estar entre 0 y 20.")]
        public decimal? FinalGrade { get; set; }

        [Range(0.00, 20.00, ErrorMessage = "El total de lapsos debe estar entre 0 y 20.")]
        public decimal? LapsoTotal { get; set; }

        public DateTime? LastUpdated { get; set; } // Para saber cuándo se actualizaron por última vez

        // Propiedades de navegación
        [ForeignKey("StudentId")]
        public virtual User Student { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }
    }
}
