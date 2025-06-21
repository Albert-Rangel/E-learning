// Controllers/StudentController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using E_learning.Data;
using E_learning.Models;
using E_learning.ViewModels; // Assuming you have a UserVM or similar for editing
using BCrypt.Net;
using E_learning.Models.ViewModels; // For password hashing/verification
using System.Linq; // Necesario para .Select()
using System.Threading.Tasks; // Necesario para Task

namespace E_learning.Controllers
{
    [Authorize(Roles = "Administrador")] // Only administrators can access this module
    public class StudentController : Controller
    {
        private readonly AppDBContext _context;

        public StudentController(AppDBContext context)
        {
            _context = context;
        }

        // GET: Student/Index - List all students
        public async Task<IActionResult> Index()
        {
            // Retrieve only users with the "Estudiante" role
            // *** MODIFICACIÓN AQUÍ: Incluimos StudentCourses y Course ***
            var students = await _context.Users
                                         .Where(u => u.Role == "Estudiante")
                                         .Include(u => u.StudentCourses) // Incluye las inscripciones del estudiante
                                             .ThenInclude(sc => sc.Course) // Incluye los datos del curso para cada inscripción
                                         .ToListAsync();
            return View(students);
        }

        // GET: Student/Edit/5 - Display edit form for a student
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Users.FindAsync(id);
            if (student == null || student.Role != "Estudiante") // Ensure it's an existing student
            {
                return NotFound();
            }

            // You might want to create a specific ViewModel for editing user details
            // to avoid exposing the hashed password and handle password changes gracefully.
            // For simplicity, let's assume we pass the User model directly,
            // but the password field will be handled carefully.
            return View(student);
        }

        // POST: Student/Edit/5 - Process edited student data
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,FullName,Email,Role,Password")] User user)
        {
            if (id != user.UserId)
            {
                return NotFound();
            }

            // Ensure the role remains "Estudiante" or whatever the admin sets it to
            // For students, we'll enforce it to remain 'Estudiante' unless explicitly allowed to change.
            // If you want admins to change roles, handle it carefully.
            // Here, we'll retrieve the existing user to maintain original role if not explicitly changed.
            var existingUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == id);
            if (existingUser == null || existingUser.Role != "Estudiante")
            {
                return NotFound(); // Or return a specific error
            }

            // If the password field in the form is NOT empty, hash the new password.
            // If it's empty, keep the existing password (don't overwrite with empty hash).
            if (!string.IsNullOrEmpty(user.Password))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            }
            else
            {
                // If password field is empty, retain the existing hashed password
                user.Password = existingUser.Password;
            }

            // Ensure the role remains "Estudiante" if not explicitly changed in the form
            // Or, if you have a dropdown for role, bind it and validate.
            user.Role = existingUser.Role; // Keep the original role if not explicitly changed in form.
                                           // If you add a Role dropdown in Edit, remove this line and bind role from form.

            // Email uniqueness check (if email is being changed)
            if (await _context.Users.AnyAsync(u => u.Email == user.Email && u.UserId != user.UserId))
            {
                ModelState.AddModelError("Email", "El correo electrónico ya está en uso por otro usuario.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user); // Update the user object
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Estudiante actualizado exitosamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Users.Any(e => e.UserId == user.UserId))
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

            return View(user);
        }

        // GET: Student/Delete/5 - Display delete confirmation for a student
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Users
                                        .FirstOrDefaultAsync(m => m.UserId == id && m.Role == "Estudiante"); // Ensure it's a student
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // POST: Student/Delete/5 - Confirm deletion of a student
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Users.FindAsync(id);
            if (student != null && student.Role == "Estudiante") // Double-check role before deleting
            {
                _context.Users.Remove(student);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Estudiante eliminado exitosamente.";
            }
            else
            {
                TempData["ErrorMessage"] = "Estudiante no encontrado o no autorizado para eliminar.";
            }
            return RedirectToAction(nameof(Index));
        }


        // GET: Student/ChangePassword/5
        [HttpGet]
        public async Task<IActionResult> ChangePassword(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id && u.Role == "Estudiante");
            if (user == null)
            {
                return NotFound();
            }
            // Pass a ViewModel to the view
            var model = new ChangePasswordVM
            {
                UserId = user.UserId
                // NewPassword is not set here, it's for input
            };

            // Pass user details to ViewBag for display in the view
            ViewBag.UserId = user.UserId;
            ViewBag.UserEmail = user.Email; // For display on the form

            return View(model); // Return the empty form
        }

        // POST: Student/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Parameters match the 'name' attributes in the form
        public async Task<IActionResult> ChangePassword(ChangePasswordVM model)
        {

            // Find the user to update regardless of validation for consistent error redisplay
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == model.UserId && u.Role == "Estudiante");
            if (user == null)
            {
                return NotFound();
            }
            if (ModelState.IsValid) // ModelState validation will now check the ViewModel
            {
                // Hash the new password
                user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

                _context.Update(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Contraseña de {user.FullName} cambiada exitosamente.";
                return RedirectToAction(nameof(Index));
            }

            // If ModelState is invalid, re-populate ViewBag for display name
            ViewBag.TeacherFullName = user.FullName; // Debería ser ViewBag.StudentFullName o similar aquí
            return View(model); // Return the model with validation errors
        }
    }
}