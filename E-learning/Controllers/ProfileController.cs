// Controllers/ProfileController.cs
using Microsoft.AspNetCore.Mvc;
using E_learning.Data;
using E_learning.Models;
using E_learning.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using System.IO;

// --- ASEGÚRATE DE QUE ESTOS USING ESTÁN PRESENTES ---
using Microsoft.AspNetCore.Authentication; // Necesario para HttpContext.SignInAsync
using Microsoft.AspNetCore.Authentication.Cookies; // Necesario para CookieAuthenticationDefaults
// --- FIN DE LOS USING NECESARIOS ---

namespace E_learning.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly AppDBContext _appDbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProfileController(AppDBContext appDbContext, IWebHostEnvironment webHostEnvironment)
        {
            _appDbContext = appDbContext;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                // Manejar el caso donde el userId no puede ser encontrado o parseado
                // Es mejor redirigir a Login o una página de error clara.
                return RedirectToAction("Login", "Access"); // Asegúrate que "Access" es tu controlador de login
            }

            // Buscar el usuario en la base de datos
            var user = await _appDbContext.Users.FindAsync(userId);
            if (user == null)
            {
                // Esto no debería pasar si el usuario está autenticado, pero es un buen fallback.
                return NotFound("Usuario no encontrado.");
            }

            // Mapear los datos del usuario al ProfileVM
            var profileVM = new ProfileVM
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                Country = user.Country,
                NationalIdNumber = user.NationalIdNumber,
                CurrentProfilePicturePath = user.ProfilePicturePath, // Importante para mostrar la actual
                Role = user.Role // Muestra el rol si lo deseas
            };

            return View(profileVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ProfileVM model)
        {
            // Re-obtener el userId del usuario logueado (por seguridad)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return RedirectToAction("Login", "Access"); // Ajusta a tu controlador de login
            }

            // Validar que el UserId del modelo coincida con el usuario logueado
            if (model.UserId != userId)
            {
                TempData["ErrorMessage"] = "Intento de edición no autorizado.";
                return RedirectToAction(nameof(Index));
            }

            // Re-obtener el usuario de la DB para actualizar solo los campos permitidos y mantener la contraseña/rol
            var userToUpdate = await _appDbContext.Users.FindAsync(userId);
            if (userToUpdate == null)
            {
                TempData["ErrorMessage"] = "Usuario no encontrado para actualizar.";
                return RedirectToAction(nameof(Index));
            }

            // Validar el ModelState ANTES de procesar el archivo.
            // Esto asegura que los campos de texto cumplen con las validaciones.
            if (!ModelState.IsValid)
            {
                // Si hay errores de validación, reestablece la ruta de la imagen actual
                // para que se muestre correctamente en la vista al regresar.
                model.CurrentProfilePicturePath = userToUpdate.ProfilePicturePath;
                ViewData["Mensaje"] = "Por favor, corrige los errores del formulario.";
                return View(model);
            }

            // Actualizar propiedades del usuario desde el ViewModel
            userToUpdate.FullName = model.FullName;
            // userToUpdate.Email = model.Email; // El email es readonly en la vista, no lo actualizamos aquí.

            userToUpdate.DateOfBirth = model.DateOfBirth;
            userToUpdate.Gender = model.Gender;
            userToUpdate.Country = model.Country;
            userToUpdate.NationalIdNumber = model.NationalIdNumber;

            // Manejo de la subida de la nueva foto de perfil
            if (model.NewProfilePicture != null && model.NewProfilePicture.Length > 0)
            {
                // Validaciones adicionales para la imagen (opcional, pero recomendado)
                const long maxFileSize = 5 * 1024 * 1024; // 5 MB
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(model.NewProfilePicture.FileName).ToLowerInvariant();

                if (model.NewProfilePicture.Length > maxFileSize)
                {
                    ModelState.AddModelError("NewProfilePicture", "El tamaño de la imagen no puede exceder 5MB.");
                    model.CurrentProfilePicturePath = userToUpdate.ProfilePicturePath; // Reestablecer para mostrar en la vista
                    return View(model);
                }

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("NewProfilePicture", "Formato de archivo no permitido. Solo JPG, PNG, GIF.");
                    model.CurrentProfilePicturePath = userToUpdate.ProfilePicturePath; // Reestablecer para mostrar en la vista
                    return View(model);
                }

                // 1. Definir la carpeta donde se guardarán las imágenes
                // Asegúrate de que esta ruta coincida con lo que esperas en wwwroot
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "profile_pictures");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // 2. Eliminar la foto anterior si no es la predeterminada
                // y si la ruta existe en el disco.
                if (!string.IsNullOrEmpty(userToUpdate.ProfilePicturePath) &&
                    userToUpdate.ProfilePicturePath != "/images/default-avatar.png")
                {
                    var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, userToUpdate.ProfilePicturePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        try
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                        catch (IOException ex)
                        {
                            // Manejar el caso donde el archivo no puede ser borrado (ej. en uso)
                            // Esto podría no ser un error crítico para el usuario, pero es bueno loggearlo.
                            Console.WriteLine($"Error al eliminar archivo antiguo: {ex.Message}");
                        }
                    }
                }

                // 3. Generar un nombre de archivo único
                var uniqueFileName = Guid.NewGuid().ToString() + fileExtension; // Usar la extensión del archivo subido
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // 4. Guardar la nueva foto
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.NewProfilePicture.CopyToAsync(fileStream);
                }
                userToUpdate.ProfilePicturePath = "/images/profile_pictures/" + uniqueFileName; // Guardar la ruta relativa en la DB
            }
            // Si el usuario envía el formulario y no sube una nueva imagen,
            // pero su CurrentProfilePicturePath en el ViewModel está vacío (ej. la eliminó en el form),
            // se puede resetear a la imagen por defecto.
            // Cuidado: si solo editan el nombre y no suben imagen, no queremos cambiar la foto existente.
            // Esta lógica solo se activa si no hay nueva imagen Y la ruta actual en el MODELO está vacía.
            else if (string.IsNullOrEmpty(model.CurrentProfilePicturePath) && !string.IsNullOrEmpty(userToUpdate.ProfilePicturePath))
            {
                // Si CurrentProfilePicturePath del ViewModel es null/vacío (significa que se quitó la img)
                // y el usuario YA TENÍA una imagen, la reseteamos a la predeterminada.
                // Primero, eliminamos la imagen antigua si no es la predeterminada
                if (userToUpdate.ProfilePicturePath != "/images/default-avatar.png")
                {
                    var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, userToUpdate.ProfilePicturePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        try { System.IO.File.Delete(oldFilePath); } catch (IOException ex) { Console.WriteLine($"Error al eliminar archivo antiguo: {ex.Message}"); }
                    }
                }
                userToUpdate.ProfilePicturePath = "/images/default-avatar.png";
            }


            try
            {
                _appDbContext.Users.Update(userToUpdate);
                await _appDbContext.SaveChangesAsync();

                // ***** Lógica para RE-AUTENTICAR y ACTUALIZAR los CLAIMS del usuario *****
                // Esto es crucial para que los cambios (nombre, foto de perfil)
                // se reflejen inmediatamente en el _Layout sin necesidad de re-login.
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userToUpdate.UserId.ToString()),
                    new Claim(ClaimTypes.Name, userToUpdate.FullName), // Nombre actualizado
                    new Claim(ClaimTypes.Email, userToUpdate.Email),
                    new Claim(ClaimTypes.Role, userToUpdate.Role),
                    // Añadir o actualizar el claim de la foto de perfil
                    new Claim("ProfilePicturePath", userToUpdate.ProfilePicturePath ?? "/images/default-avatar.png")
                };

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true // Asegúrate de que esta propiedad coincida con tu lógica de login
                                        // Si tu cookie no es persistente, ponlo a 'false'
                };

                // Re-firmar el usuario para actualizar el cookie de autenticación
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);
                // ***** FIN DE LA LÓGICA DE ACTUALIZACIÓN DE CLAIMS *****


                TempData["SuccessMessage"] = "¡Perfil actualizado exitosamente!";
                return RedirectToAction("Index"); // Redirige de nuevo a la página de perfil
            }
            catch (DbUpdateConcurrencyException)
            {
                // Si hay un error de concurrencia, intenta encontrar el usuario.
                // Esto es útil en escenarios donde múltiples usuarios editan el mismo registro.
                if (!_appDbContext.Users.Any(e => e.UserId == userToUpdate.UserId))
                {
                    return NotFound("El usuario ya no existe.");
                }
                else
                {
                    throw; // Re-lanza la excepción si es un problema de concurrencia no esperado
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ocurrió un error al guardar el perfil: {ex.Message}";
                // En un entorno de producción, loggea `ex` para depuración.
                // Reestablece la ruta de la imagen actual en el modelo para que se muestre al regresar a la vista
                model.CurrentProfilePicturePath = userToUpdate.ProfilePicturePath;
                return View(model);
            }
        }
    }
}
