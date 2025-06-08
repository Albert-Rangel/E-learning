// Controllers/TeacherController.cs (Updated)
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using E_learning.Data;
using E_learning.Models;
using Microsoft.EntityFrameworkCore;
using E_learning.Models.ViewModels; // Add this using directive
using BCrypt.Net; // For password hashing
using System.Security.Cryptography; // For Secure Random Number Generator
using System.Text; // For StringBuilder for password generation

namespace E_learning.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class TeacherController : Controller
    {
        private readonly AppDBContext _context;

        public TeacherController(AppDBContext context)
        {
            _context = context;
        }

        // GET: /Teacher/Index - List all teachers
        public async Task<IActionResult> Index()
        {
            // Retrieve generated password from TempData if redirected from Create
            ViewBag.GeneratedPassword = TempData["GeneratedPassword"] as string;

            var teachers = await _context.Users
                                         .Where(u => u.Role == "Profesor")
                                         .ToListAsync();
            return View(teachers);
        }

        // GET: /Teacher/Create - Show form to create a new teacher
        public IActionResult Create()
        {
            var newTeacher = new User { Role = "Profesor" }; // Default role
            return View(newTeacher);
        }

        // POST: /Teacher/Create - Handle creation of a new teacher
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FullName,Email")] User teacher) // Password is NOT bound, it's generated
        {
            teacher.Role = "Profesor"; // Ensure role is always "Profesor"

            // Basic validation for email uniqueness for teachers
            if (await _context.Users.AnyAsync(u => u.Email == teacher.Email))
            {
                ModelState.AddModelError("Email", "Este email ya está registrado.");
            }


            // 1. Generate a random password
            string generatedPassword = GenerateRandomPassword(8); // Generate an 8-character password

            // 2. Hash the generated password
            teacher.Password = BCrypt.Net.BCrypt.HashPassword(generatedPassword);

            _context.Add(teacher);
            await _context.SaveChangesAsync();

            // Store the plain-text password in TempData to display it on the Index page
            TempData["GeneratedPassword"] = generatedPassword;
            TempData["GeneratedTeacherEmail"] = teacher.Email; // Also send email for context

            return RedirectToAction(nameof(Index));

        }

        // GET: /Teacher/Edit/5 - Show form to edit an existing teacher
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Users
                                        .FirstOrDefaultAsync(u => u.UserId == id && u.Role == "Profesor");
            if (teacher == null)
            {
                return NotFound();
            }
            return View(teacher);
        }

        // POST: /Teacher/Edit/5 - Handle updating an existing teacher
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,FullName,Email")] User teacher) // Password is NOT bound here, for a separate change password
        {
            if (id != teacher.UserId)
            {
                return NotFound();
            }

            // Retrieve the existing teacher from the database to retain the current password and role
            var existingTeacher = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == id && u.Role == "Profesor");
            if (existingTeacher == null)
            {
                return NotFound();
            }

            // Ensure the role remains "Profesor" and preserve the existing password
            teacher.Role = "Profesor";
            teacher.Password = existingTeacher.Password; // Preserve the current hashed password

            // Basic validation for email uniqueness (excluding self)
            if (await _context.Users.AnyAsync(u => u.Email == teacher.Email && u.UserId != teacher.UserId))
            {
                ModelState.AddModelError("Email", "Este email ya está registrado por otro usuario.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(teacher);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Profesor actualizado exitosamente."; // Add success message
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Users.Any(e => e.UserId == teacher.UserId && e.Role == "Profesor"))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(teacher);
        }

        // GET: /Teacher/ChangePassword/5 - Show form to change teacher's password
        public async Task<IActionResult> ChangePassword(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id && u.Role == "Profesor");
            if (teacher == null)
            {
                return NotFound();
            }

            // Pass a ViewModel to the view
            var model = new ChangePasswordVM
            {
                UserId = teacher.UserId
                // NewPassword is not set here, it's for input
            };

            ViewBag.TeacherFullName = teacher.FullName; // Still use ViewBag for display name
            return View(model); // Pass the ViewModel
        }

        // POST: /Teacher/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM model) // Accept the ViewModel
        {
            var teacher = await _context.Users.FirstOrDefaultAsync(u => u.UserId == model.UserId && u.Role == "Profesor");
            if (teacher == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid) // ModelState validation will now check the ViewModel
            {
                // Hash the new password
                teacher.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

                _context.Update(teacher);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Contraseña de {teacher.FullName} cambiada exitosamente.";
                return RedirectToAction(nameof(Index));
            }

            // If ModelState is invalid, re-populate ViewBag for display name
            ViewBag.TeacherFullName = teacher.FullName;
            return View(model); // Return the model with validation errors
        }


        // GET: Teacher/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Fetch the user, ensuring they are a "Profesor"
            var teacher = await _context.Users
                .FirstOrDefaultAsync(m => m.UserId == id && m.Role == "Profesor");

            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher); // Pass the teacher object to the view
        }

        // POST: Teacher/Delete/5 (or Teacher/DeleteConfirmed/5)
        [HttpPost, ActionName("Delete")] // Use ActionName if your POST action is named differently
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Fetch the user, ensuring they are a "Profesor"
            var teacher = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id && u.Role == "Profesor");

            if (teacher != null)
            {
                _context.Users.Remove(teacher);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Profesor {teacher.FullName} eliminado exitosamente.";
            }
            else
            {
                TempData["ErrorMessage"] = "Profesor no encontrado o no autorizado para eliminar.";
            }

            return RedirectToAction(nameof(Index));
        }


        // Helper method to generate a random password
        private string GenerateRandomPassword(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";
            var password = new StringBuilder();
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] data = new byte[length];
                rng.GetBytes(data);
                foreach (byte b in data)
                {
                    password.Append(chars[b % chars.Length]);
                }
            }
            return password.ToString();
        }
    }
}