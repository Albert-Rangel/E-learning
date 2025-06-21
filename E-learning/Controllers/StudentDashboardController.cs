// Controllers/StudentDashboardController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using E_learning.Data;
using E_learning.Models;
using E_learning.ViewModels; // Asegúrate de que este namespace sea correcto si lo usas
using E_learning.Models.ViewModels; // Asegúrate de que este namespace sea correcto si lo usas

[Authorize(Roles = "Estudiante")]
public class StudentDashboardController : Controller
{
    private readonly AppDBContext _context;

    public StudentDashboardController(AppDBContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> MyGrades()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userIdString))
        {
            return RedirectToAction("Login", "Account");
        }

        int currentUserId = int.Parse(userIdString);

        // Obtener las notas del estudiante, incluyendo la información del curso
        // Asegúrate de que solo las notas de cursos en los que el estudiante está actualmente inscrito se muestren
        var studentGrades = await _context.Grades
                                          .Where(g => g.StudentId == currentUserId)
                                          // Unir con StudentCourses para asegurar que solo se muestren las notas de cursos activos
                                          .Join(_context.StudentCourses.Where(sc => sc.StudentId == currentUserId), // Filtra StudentCourses por el estudiante actual
                                                grade => grade.CourseId, // Clave de unión: CourseId de Grade
                                                studentCourse => studentCourse.CourseId, // Clave de unión: CourseId de StudentCourse
                                                (grade, studentCourse) => grade) // Selecciona el objeto Grade después de la unión
                                          .Include(g => g.Course) // Carga los datos del curso relacionado
                                          .OrderBy(g => g.Course.Name)
                                          .ToListAsync();

        var studentUser = await _context.Users.FindAsync(currentUserId);
        if (studentUser == null)
        {
            return NotFound("Datos de estudiante no encontrados.");
        }

        ViewBag.StudentName = studentUser.FullName;

        return View(studentGrades);
    }

    public async Task<IActionResult> MyCourses()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString))
        {
            return RedirectToAction("Login", "Account");
        }
        int currentUserId = int.Parse(userIdString);

        var myCourses = await _context.StudentCourses
                                      .Where(sc => sc.StudentId == currentUserId)
                                      .Include(sc => sc.Course)
                                          .ThenInclude(c => c.Teacher)
                                      .ToListAsync();

        ViewBag.StudentName = (await _context.Users.FindAsync(currentUserId))?.FullName;
        return View(myCourses);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnenrollCourse(int id) // 'id' es el StudentCourseId
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString))
        {
            return RedirectToAction("Login", "Account");
        }
        int currentUserId = int.Parse(userIdString);

        var studentCourse = await _context.StudentCourses
                                          .Include(sc => sc.Course) // Se necesita para el mensaje de éxito
                                          .FirstOrDefaultAsync(sc => sc.StudentCourseId == id && sc.StudentId == currentUserId);

        if (studentCourse == null)
        {
            TempData["ErrorMessage"] = "No se encontró la inscripción o no tienes permiso para realizar esta acción.";
            return RedirectToAction(nameof(MyCourses));
        }

        try
        {
            // ***** INICIO DE LA LÓGICA AGREGADA PARA ELIMINAR NOTAS *****
            // 1. Encontrar las calificaciones asociadas a esta inscripción
            // Usamos StudentId y CourseId porque Grade se relaciona con ambos.
            var gradesToDelete = await _context.Grades
                                              .Where(g => g.StudentId == currentUserId && g.CourseId == studentCourse.CourseId)
                                              .ToListAsync();

            // 2. Si existen calificaciones, eliminarlas
            if (gradesToDelete.Any())
            {
                _context.Grades.RemoveRange(gradesToDelete);
                // No es necesario SaveChangesAsync aquí, se hará al final junto con la eliminación del StudentCourse.
            }
            // ***** FIN DE LA LÓGICA AGREGADA PARA ELIMINAR NOTAS *****

            // 3. Eliminar la inscripción del curso
            _context.StudentCourses.Remove(studentCourse);

            // 4. Guardar todos los cambios en la base de datos (tanto grados como inscripción)
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Te has desinscrito exitosamente del curso '{studentCourse.Course.Name}' y sus notas han sido eliminadas.";
        }
        catch (Exception ex)
        {
            // Manejar errores de base de datos
            TempData["ErrorMessage"] = $"Ocurrió un error al intentar desinscribirte del curso: {ex.Message}";
            // Opcional: loggear el 'ex' completo para depuración
        }

        return RedirectToAction(nameof(MyCourses));
    }

    public async Task<IActionResult> AvailableCourses()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString))
        {
            return RedirectToAction("Login", "Account");
        }
        int currentUserId = int.Parse(userIdString);

        var allCourses = await _context.Courses
                                       .Include(c => c.Teacher)
                                       .ToListAsync();

        var enrolledCourseIds = await _context.StudentCourses
                                              .Where(sc => sc.StudentId == currentUserId)
                                              .Select(sc => sc.CourseId)
                                              .ToListAsync();

        var availableCourses = allCourses.Where(c => !enrolledCourseIds.Contains(c.CourseId)).ToList();

        ViewBag.StudentName = (await _context.Users.FindAsync(currentUserId))?.FullName;
        return View(availableCourses);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnrollCourse(int id)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString))
        {
            return RedirectToAction("Login", "Account");
        }
        int currentUserId = int.Parse(userIdString);

        bool alreadyEnrolled = await _context.StudentCourses
                                             .AnyAsync(sc => sc.StudentId == currentUserId && sc.CourseId == id);

        if (alreadyEnrolled)
        {
            TempData["ErrorMessage"] = "Ya estás inscrito en este curso.";
            return RedirectToAction(nameof(AvailableCourses));
        }

        var courseToEnroll = await _context.Courses.FindAsync(id);
        if (courseToEnroll == null)
        {
            TempData["ErrorMessage"] = "El curso al que intentas inscribirte no existe.";
            return RedirectToAction(nameof(AvailableCourses));
        }

        var studentCourse = new StudentCourse
        {
            StudentId = currentUserId,
            CourseId = id,
            EnrollmentDate = DateTime.Now
        };

        try
        {
            _context.StudentCourses.Add(studentCourse);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Te has inscrito exitosamente en el curso '{courseToEnroll.Name}'.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Ocurrió un error al intentar inscribirte en el curso: {ex.Message}";
        }

        return RedirectToAction(nameof(AvailableCourses));
    }
}