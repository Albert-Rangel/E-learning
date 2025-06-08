using System.ComponentModel.DataAnnotations;

namespace E_learning.ViewModels
{
    public class LoginVM
    {

        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de email inválido.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        // No min length here for login, as it's just for matching.
        // The password has already been hashed and stored.
        public string Password { get; set; }

    }
}
