using Microsoft.AspNetCore.Mvc;
using E_learning.Data;
using E_learning.Models;
using E_learning.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using BCrypt.Net; // <--- ADD THIS USING STATEMENT

namespace E_learning.Controllers
{
    public class AccessController : Controller
    {
        private readonly AppDBContext _appDdContext;
        public AccessController(AppDBContext appDBContext)
        {
            _appDdContext = appDBContext;
        }

        [HttpGet]
        public IActionResult SignIn()
        {
            return View();
        }

        // Acción POST para procesar el envío del formulario (Registro de Usuario)
        [HttpPost]
        public async Task<IActionResult> SignIn(UserVM model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Mensaje"] = "Por favor, corrige los errores del formulario.";
                return View(model);
            }

            // Check if email already exists
            User? user_Found = await _appDdContext.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user_Found != null)
            {
                ViewData["Mensaje"] = "El correo electrónico ya está registrado.";
                return View(model);
            }

            User new_User = new User()
            {
                FullName = model.FullName,
                Email = model.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = model.Role // This should typically be "Estudiante" if registered via SignIn
            };

            await _appDdContext.Users.AddAsync(new_User);
            await _appDdContext.SaveChangesAsync();

            // Store the success message in TempData to be displayed on the login page
            TempData["SuccessMessage"] = $"¡Usuario {new_User.FullName} registrado exitosamente! Ahora puedes iniciar sesión.";

            // Redirect to the LogIn page after successful registration
            return RedirectToAction("LogIn", "Access");
        }

        [HttpGet]
        public IActionResult LogIn()
        {
            if (User.Identity!.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        // Acción POST para procesar el envío del formulario (Inicio de Sesión)
        [HttpPost]
        public async Task<IActionResult> LogIn(LoginVM model) // Asumiendo LoginVM es tu modelo de vista para Login
        {
            // Find user by email first
            User? user_Found = await _appDdContext.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user_Found == null)
            {
                ViewData["Mensaje"] = "El usuario no se encuentra registrado."; // Generic message for security
                return View(model);
            }

            // VERIFY THE HASHED PASSWORD
            // BCrypt.Net.BCrypt.Verify compares the plain-text password with the hashed password
            if (!BCrypt.Net.BCrypt.Verify(model.Password, user_Found.Password))
            {
                ViewData["Mensaje"] = "Credenciales inválidas."; // Generic message for security
                return View(model);
            }

            // --- CRITICAL PART: Ensure both NameIdentifier and Role claims are added ---
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user_Found.FullName),
                new Claim(ClaimTypes.NameIdentifier, user_Found.UserId.ToString()) // <-- THIS IS VITAL
            };

            // Add the role claim if a role exists on your User model
            if (!string.IsNullOrEmpty(user_Found.Role))
            {
                claims.Add(new Claim(ClaimTypes.Role, user_Found.Role)); // <-- THIS IS ALSO VITAL
            }
            // --- END CRITICAL PART ---

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            AuthenticationProperties properties = new AuthenticationProperties()
            {
                AllowRefresh = true,
                // You might want to set IsPersistent = model.RememberMe (if you have this checkbox)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                properties
            );

            return RedirectToAction("Index", "Home");
        }


    }
}