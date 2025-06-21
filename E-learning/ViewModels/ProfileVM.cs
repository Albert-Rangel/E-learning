// ViewModels/ProfileVM.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http; // Necesario para IFormFile
using System; // Asegúrate de tener este using para DateTime

namespace E_learning.ViewModels
{
    public class ProfileVM
    {
        // Propiedades existentes del usuario que quizás quieras mostrar/editar
        public int UserId { get; set; }

        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre completo no puede exceder los 50 caracteres.")]
        [Display(Name = "Nombre Completo")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de email inválido.")]
        // Ajustado a 100 para coincidir con el modelo User si es 100 allá
        [StringLength(100, ErrorMessage = "El email no puede exceder los 100 caracteres.")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; } = string.Empty;

        // Nuevas propiedades del perfil
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Nacimiento")]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(10)]
        [Display(Name = "Género")]
        public string? Gender { get; set; }

        [StringLength(100)]
        [Display(Name = "País de Residencia")]
        public string? Country { get; set; }

        [StringLength(50)]
        [Display(Name = "Número de Identidad")]
        public string? NationalIdNumber { get; set; }

        [Display(Name = "Foto de Perfil Actual")]
        public string? CurrentProfilePicturePath { get; set; } // Para mostrar la foto actual

        [Display(Name = "Nueva Foto de Perfil")]
        public IFormFile? NewProfilePicture { get; set; } // Para subir una nueva foto

        // Propiedad opcional para manejar el rol si se permite la visualización
        [Display(Name = "Rol")]
        public string? Role { get; set; }
    }
}