// Models/ViewModels/ChangePasswordViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace E_learning.Models.ViewModels
{
    public class ChangePasswordVM
    {
        [Required(ErrorMessage = "El ID del profesor es obligatorio.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva Contraseña")]
        public string NewPassword { get; set; } = string.Empty;

        // You could add a confirmation password field here for client-side validation
        // [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden.")]
        // [DataType(DataType.Password)]
        // [Display(Name = "Confirmar Contraseña")]
        // public string ConfirmPassword { get; set; }
    }
}