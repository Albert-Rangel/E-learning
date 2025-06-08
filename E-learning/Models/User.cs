// Models/User.cs (Confirm or Update this)
using System.ComponentModel.DataAnnotations;

namespace E_learning.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre completo no puede exceder los 50 caracteres.")]
        [Display(Name = "Nombre Completo")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de email inválido.")]
        [StringLength(50, ErrorMessage = "El email no puede exceder los 50 caracteres.")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; } = string.Empty;

        [StringLength(128, ErrorMessage = "La contraseña hash no puede exceder los 100 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string? Password { get; set; } // It should be nullable (string?) as per previous discussion.


        [Required(ErrorMessage = "El rol es obligatorio.")]
        [StringLength(20)]
        [Display(Name = "Rol")]
        public string Role { get; set; } = "Estudiante"; // Default role

        public virtual ICollection<Course> TaughtCourses { get; set; } // For teachers
        public virtual ICollection<StudentCourse> StudentCourses { get; set; } // For students
        public virtual ICollection<Grade> Grades { get; set; } // ADDED: For student grades
        public User()
        {
            TaughtCourses = new HashSet<Course>();
            StudentCourses = new HashSet<StudentCourse>();
            Grades = new HashSet<Grade>(); // Initialize collection
        }

    }
}