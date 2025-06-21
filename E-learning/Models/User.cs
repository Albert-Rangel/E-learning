// Models/User.cs
using System.ComponentModel.DataAnnotations;
using System; // Asegúrate de tener este using para DateTime
using System.Collections.Generic; // Make sure this is present if you have collections

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
        public string? Password { get; set; } // Debería ser nullable (string?)

        [Required(ErrorMessage = "El rol es obligatorio.")]
        [StringLength(20)]
        [Display(Name = "Rol")]
        public string Role { get; set; } = "Estudiante"; // Rol por defecto

        // --- NUEVAS PROPIEDADES PARA EL PERFIL ---
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Nacimiento")]
        public DateTime? DateOfBirth { get; set; } // Puede ser nulo si no es obligatorio

        [StringLength(10)]
        [Display(Name = "Género")]
        public string? Gender { get; set; } // Ejemplo: "Masculino", "Femenino", "Otro"

        [StringLength(100)]
        [Display(Name = "País de Residencia")]
        public string? Country { get; set; }

        [StringLength(50)]
        [Display(Name = "Número de Identidad")]
        public string? NationalIdNumber { get; set; } // DNI, Cédula, Pasaporte, etc.

        [StringLength(255)] // Ruta relativa a la imagen (ej. /images/profiles/user_123.jpg)
        [Display(Name = "Foto de Perfil")]
        public string? ProfilePicturePath { get; set; } = "/images/default-avatar.png"; // Valor predeterminado
        // --- FIN DE NUEVAS PROPIEDADES ---

        public virtual ICollection<Course> TaughtCourses { get; set; } // Para profesores
        public virtual ICollection<StudentCourse> StudentCourses { get; set; } // Para estudiantes
        public virtual ICollection<Grade> Grades { get; set; } // Para notas de estudiantes

        public User()
        {
            TaughtCourses = new HashSet<Course>();
            StudentCourses = new HashSet<StudentCourse>();
            Grades = new HashSet<Grade>(); // Inicializar colección
        }
    }
}