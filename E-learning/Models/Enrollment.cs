using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_learning.Models
{
    public class Enrollment
    {
        [Key]
        public int EnrollmentId { get; set; }

        [Required(ErrorMessage = "El ID de usuario es obligatorio.")]
        public int UserId { get; set; } // Cambiado de StudentId a UserId

        [Required(ErrorMessage = "El curso es obligatorio.")]
        public int CourseId { get; set; }

        public DateTime EnrollmentDate { get; set; } = DateTime.Now;

        // Propiedades de navegación
        [ForeignKey("UserId")] // La clave foránea apunta a UserId en el modelo User
        public User User { get; set; } // Propiedad de navegación al modelo User (para el estudiante)

        [ForeignKey("CourseId")]
        public Course Course { get; set; }
    }
}
