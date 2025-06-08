// Models/Course.cs
using System;
using System.Collections.Generic; // Make sure this is present
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_learning.Models
{
    public class Course
    {
        [Key]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "El nombre del curso es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio.")]
        public string Status { get; set; } // "Activo", "Inactivo", "Finalizado"

        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; }

        // Foreign key for Teacher - CHANGED FROM STRING TO INT
        [Required(ErrorMessage = "Debe asignar un profesor a este curso.")]
        public int TeacherId { get; set; } // Now matches User.UserId (int)

        // Navigation property for Teacher
        [ForeignKey("TeacherId")]
        public virtual User Teacher { get; set; }

        // ADD THIS NAVIGATION PROPERTY
        public virtual ICollection<StudentCourse> StudentCourses { get; set; } = new HashSet<StudentCourse>(); // Initialize to avoid null reference

        // ADDED: Navigation property for Grades
        public virtual ICollection<Grade> Grades { get; set; } = new HashSet<Grade>(); // Initialize to avoid null reference

        public Course()
        {
            StudentCourses = new HashSet<StudentCourse>();
            Grades = new HashSet<Grade>(); // Initialize collection in constructor
        }
    }
}
