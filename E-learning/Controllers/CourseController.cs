// Controllers/CourseController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // For SelectList
using E_learning.Data;
using E_learning.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using E_learning.Models.ViewModels; // Added for DateTime

namespace E_learning.Controllers
{
   
    public class CourseController : Controller
    {
        private readonly AppDBContext _context;


        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> AssignedCourses()
        {
            var teacherIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(teacherIdString))
            {
                TempData["ErrorMessage"] = "No se pudo identificar al profesor.";
                return RedirectToAction("Login", "Account");
            }

            if (!int.TryParse(teacherIdString, out int teacherId))
            {
                TempData["ErrorMessage"] = "Formato de ID de profesor inválido.";
                return RedirectToAction("Login", "Account");
            }

            var assignedCourses = await _context.Courses
                                                .Include(c => c.StudentCourses) // Include enrollments to count students
                                                    .ThenInclude(sc => sc.Student) // Optionally include student details
                                                .Where(c => c.TeacherId == teacherId)
                                                .ToListAsync();

            var assignedCourseViewModels = assignedCourses.Select(c => new AssignedCourseViewModel
            {
                CourseId = c.CourseId,
                Name = c.Name,
                Description = c.Description,
                Status = c.Status,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                NumberOfStudentsEnrolled = c.StudentCourses?.Count ?? 0 // Count enrolled students
            }).ToList();

            return View(assignedCourseViewModels);
        }

        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> ValidateStudents(int? id) // 'id' will be CourseId
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacherIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(teacherIdString))
            {
                TempData["ErrorMessage"] = "No se pudo identificar al profesor.";
                return RedirectToAction("Login", "Account");
            }

            if (!int.TryParse(teacherIdString, out int teacherId))
            {
                TempData["ErrorMessage"] = "Formato de ID de profesor inválido.";
                return RedirectToAction("Login", "Account");
            }

            var course = await _context.Courses
                                       .Include(c => c.Teacher) // Include teacher to display their name
                                       .Include(c => c.StudentCourses) // Include enrollments
                                           .ThenInclude(sc => sc.Student) // Include student details for each enrollment
                                       .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
            {
                return NotFound();
            }

            // IMPORTANT: Verify that the logged-in teacher is assigned to this course
            if (course.TeacherId != teacherId)
            {
                TempData["ErrorMessage"] = "No tienes permiso para ver los estudiantes de este curso.";
                return RedirectToAction(nameof(AssignedCourses)); // Redirect to their assigned courses
            }

            var studentDetails = course.StudentCourses
                                       .Select(sc => new StudentDetailViewModel
                                       {
                                           StudentId = sc.Student.UserId, // Assuming UserId is the PK for User
                                           FullName = sc.Student.FullName,
                                           Email = sc.Student.Email,
                                           EnrollmentDate = sc.EnrollmentDate
                                       })
                                       .ToList();

            var viewModel = new StudentValidationViewModel
            {
                CourseId = course.CourseId,
                CourseName = course.Name,
                CourseDescription = course.Description,
                TeacherFullName = course.Teacher?.FullName ?? "N/A",
                EnrolledStudents = studentDetails
            };

            return View(viewModel);
        }

        // --- NUEVAS Acciones para la Gestión de Notas del Profesor ---

        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> GradesManagement()
        {
            var teacherIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(teacherIdString))
            {
                TempData["ErrorMessage"] = "No se pudo identificar al profesor.";
                return RedirectToAction("Login", "Account");
            }

            if (!int.TryParse(teacherIdString, out int teacherId))
            {
                TempData["ErrorMessage"] = "Formato de ID de profesor inválido.";
                return RedirectToAction("Login", "Account");
            }

            // Reutilizar la lógica de obtención de cursos asignados
            var assignedCourses = await _context.Courses
                                                .Include(c => c.StudentCourses) // Incluir inscripciones para contar estudiantes
                                                .Where(c => c.TeacherId == teacherId)
                                                .ToListAsync();

            var courseGradesViewModels = assignedCourses.Select(c => new CourseGradesViewModel
            {
                CourseId = c.CourseId,
                Name = c.Name,
                Description = c.Description,
                Status = c.Status,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                NumberOfStudentsEnrolled = c.StudentCourses?.Count ?? 0
            }).ToList();

            return View(courseGradesViewModels);
        }

        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> EnterGrades(int? courseId) // 'courseId' para identificar el curso
        {
            if (courseId == null)
            {
                return NotFound();
            }

            var teacherIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(teacherIdString))
            {
                TempData["ErrorMessage"] = "No se pudo identificar al profesor.";
                return RedirectToAction("Login", "Account");
            }

            if (!int.TryParse(teacherIdString, out int teacherId))
            {
                TempData["ErrorMessage"] = "Formato de ID de profesor inválido.";
                return RedirectToAction("Login", "Account");
            }

            var course = await _context.Courses
                                       .Include(c => c.Teacher)
                                       .Include(c => c.StudentCourses)
                                           .ThenInclude(sc => sc.Student) // Incluir datos del estudiante para cada inscripción
                                       .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course == null)
            {
                return NotFound();
            }

            // Verificar que el profesor logueado esté asignado a este curso
            if (course.TeacherId != teacherId)
            {
                TempData["ErrorMessage"] = "No tienes permiso para ingresar notas a este curso.";
                return RedirectToAction(nameof(GradesManagement));
            }

            var studentGrades = new List<IndividualStudentGradeEntryViewModel>();

            foreach (var sc in course.StudentCourses)
            {
                // Buscar notas existentes para este estudiante en este curso
                var existingGrade = await _context.Grades
                                                  .FirstOrDefaultAsync(g => g.StudentId == sc.StudentId && g.CourseId == course.CourseId);

                studentGrades.Add(new IndividualStudentGradeEntryViewModel
                {
                    StudentId = sc.Student.UserId,
                    FullName = sc.Student.FullName,
                    Email = sc.Student.Email,
                    Lapso1 = existingGrade?.Lapso1,
                    Lapso2 = existingGrade?.Lapso2,
                    Lapso3 = existingGrade?.Lapso3
                });
            }

            var viewModel = new CourseGradeEntryViewModel
            {
                CourseId = course.CourseId,
                CourseName = course.Name,
                TeacherFullName = course.Teacher?.FullName ?? "N/A",
                StudentGrades = studentGrades
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Profesor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveGrades(int courseId, CourseGradeEntryViewModel model)
        {
            // Re-verificar ID del profesor y asignación del curso por seguridad
            var teacherIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(teacherIdString) || !int.TryParse(teacherIdString, out int teacherId))
            {
                TempData["ErrorMessage"] = "Error de autenticación del profesor al intentar guardar notas.";
                return RedirectToAction(nameof(GradesManagement));
            }

            var course = await _context.Courses.FindAsync(courseId);
            if (course == null || course.TeacherId != teacherId)
            {
                TempData["ErrorMessage"] = "Curso no encontrado o no asignado a este profesor. Las notas no se guardaron.";
                return RedirectToAction(nameof(GradesManagement));
            }

            //// Aquí se valida el modelo antes de guardar.
            //// Si hay errores de validación (ej. notas fuera de rango), se vuelve a la vista con los errores.
            //if (!ModelState.IsValid)
            //{
            //    // Volver a poblar los datos del curso para la vista, ya que no están en el modelo POST
            //    model.CourseName = course.Name;
            //    model.TeacherFullName = course.Teacher?.FullName ?? "N/A";
            //    TempData["ErrorMessage"] = "Por favor, corrige los errores de validación de las notas.";
            //    return View("EnterGrades", model); // Vuelve a la misma vista con los errores
            //}

            foreach (var studentGradeEntry in model.StudentGrades)
            {
                // Verificar que el estudiante esté realmente inscrito en este curso (seguridad extra)
                var isStudentEnrolled = await _context.StudentCourses
                                                      .AnyAsync(sc => sc.StudentId == studentGradeEntry.StudentId && sc.CourseId == courseId);

                if (!isStudentEnrolled)
                {
                    // Esto es una advertencia, no un error que deba detener la operación.
                    // Podrías registrarlo o informar al usuario de alguna manera si es necesario.
                    Console.WriteLine($"Advertencia: Estudiante {studentGradeEntry.FullName} (ID: {studentGradeEntry.StudentId}) no está inscrito en el curso {course.Name} (ID: {courseId}). Saltando la actualización de notas.");
                    continue; // Saltar las notas de este estudiante si no está inscrito
                }

                var grade = await _context.Grades
                                          .FirstOrDefaultAsync(g => g.StudentId == studentGradeEntry.StudentId && g.CourseId == courseId);

                if (grade == null)
                {
                    // Si no existen notas para este estudiante en este curso, crear una nueva entrada
                    grade = new Grade
                    {
                        StudentId = studentGradeEntry.StudentId,
                        CourseId = courseId
                    };
                    _context.Grades.Add(grade);
                }

                // Actualizar los valores de las notas
                grade.Lapso1 = studentGradeEntry.Lapso1;
                grade.Lapso2 = studentGradeEntry.Lapso2;
                grade.Lapso3 = studentGradeEntry.Lapso3;
                grade.LastUpdated = DateTime.Now; // Actualizar fecha de última modificación
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Notas guardadas exitosamente.";
            return RedirectToAction(nameof(GradesManagement));
        }


        public CourseController(AppDBContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Estudiante")]
        public async Task<IActionResult> AvailableCourses()
        {
            // Get the current logged-in student's ID
            var studentIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(studentIdString))
            {
                TempData["ErrorMessage"] = "No se pudo identificar al estudiante.";
                // If the student ID is not found, you might want to redirect to a login page
                // or an error page, rather than back to AvailableCourses which they can't access without an ID.
                return RedirectToAction("Login", "Account"); // Example: redirect to a Login action
            }

            // Parse studentId to int
            if (!int.TryParse(studentIdString, out int studentId))
            {
                TempData["ErrorMessage"] = "Formato de ID de estudiante inválido.";
                return RedirectToAction("Login", "Account"); // Or appropriate error page
            }

            // Fetch all active courses
            var allCourses = await _context.Courses
                                           .Include(c => c.Teacher)
                                           .Where(c => c.Status == "Activo" && c.EndDate >= DateTime.Today) // Only active and not ended courses
                                           .ToListAsync();

            // Fetch the courses the current student is already enrolled in
            var enrolledCourseIds = await _context.StudentCourses
                                                 .Where(sc => sc.StudentId == studentId)
                                                 .Select(sc => sc.CourseId)
                                                 .ToListAsync();

            // Create a list of CourseViewModel to pass to the view
            var courseViewModels = allCourses.Select(c => new CourseViewModel
            {
                CourseId = c.CourseId,
                Name = c.Name,
                Description = c.Description,
                Status = c.Status,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                TeacherFullName = c.Teacher != null ? c.Teacher.FullName : "N/A",
                IsEnrolled = enrolledCourseIds.Contains(c.CourseId)
            }).ToList();

            return View(courseViewModels);
        }

        [Authorize(Roles = "Estudiante")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnrollInCourse(int courseId)
        {
            // Get studentId as string from claims
            var studentIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(studentIdString))
            {
                TempData["ErrorMessage"] = "No se pudo identificar al estudiante.";
                return RedirectToAction(nameof(AvailableCourses));
            }

            // Attempt to parse the studentId to int
            if (!int.TryParse(studentIdString, out int studentId))
            {
                TempData["ErrorMessage"] = "Formato de ID de estudiante inválido.";
                return RedirectToAction(nameof(AvailableCourses));
            }

            // Check if the student is already enrolled
            var existingEnrollment = await _context.StudentCourses
                                                   .FirstOrDefaultAsync(sc => sc.StudentId == studentId && sc.CourseId == courseId);

            if (existingEnrollment != null)
            {
                TempData["ErrorMessage"] = "Ya estás inscrito en este curso.";
            }
            else
            {
                var course = await _context.Courses.FindAsync(courseId);
                if (course == null)
                {
                    TempData["ErrorMessage"] = "Curso no encontrado.";
                }
                else if (course.Status != "Activo" || course.EndDate < DateTime.Today)
                {
                    TempData["ErrorMessage"] = "No puedes inscribirte en este curso (no está activo o ya ha finalizado).";
                }
                else
                {
                    var studentCourse = new StudentCourse
                    {
                        StudentId = studentId, // Use the parsed int studentId here
                        CourseId = courseId,
                        EnrollmentDate = DateTime.Now
                    };
                    _context.StudentCourses.Add(studentCourse);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "¡Inscripción exitosa!";
                }
            }

            return RedirectToAction(nameof(AvailableCourses));
        }

        [Authorize(Roles = "Estudiante")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnenrollFromCourse(int courseId)
        {
            var studentIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(studentIdString))
            {
                TempData["ErrorMessage"] = "No se pudo identificar al estudiante.";
                return RedirectToAction(nameof(AvailableCourses));
            }

            if (!int.TryParse(studentIdString, out int studentId))
            {
                TempData["ErrorMessage"] = "Formato de ID de estudiante inválido.";
                return RedirectToAction(nameof(AvailableCourses));
            }

            var studentCourse = await _context.StudentCourses
                                              .FirstOrDefaultAsync(sc => sc.StudentId == studentId && sc.CourseId == courseId);

            if (studentCourse == null)
            {
                TempData["ErrorMessage"] = "No estás inscrito en este curso.";
            }
            else
            {
                _context.StudentCourses.Remove(studentCourse);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Te has desinscrito del curso exitosamente.";
            }

            return RedirectToAction(nameof(AvailableCourses));
        }


        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CourseManagement()
        {
            var courses = await _context.Courses
                                        .Include(c => c.Teacher)
                                        .ToListAsync();
            return View(courses);
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Create()
        {
            var teachers = await _context.Users
                                         .Where(u => u.Role == "Profesor")
                                         .ToListAsync();

            if (!teachers.Any())
            {
                ModelState.AddModelError("", "Actualmente no hay profesores disponibles para asignar a un curso. Por favor, añada un profesor primero."); //
                ViewBag.TeacherList = new SelectList(new List<User>(), "UserId", "FullName"); // Keep ViewBag populated
            }
            else
            {
                ViewBag.TeacherList = new SelectList(teachers, "UserId", "FullName");
            }

            var newCourse = new Course // Initialize the Course model here
            {
                StartDate = DateTime.Today, // Default StartDate to today's date
                EndDate = DateTime.Today.AddMonths(1) // Default EndDate to one month from today's date
            };

            return View(newCourse); // Pass the initialized model to the view
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Status,StartDate,EndDate,TeacherId")] Course course)
        {
            // Re-fetch teachers to populate the dropdown in case of validation errors
            var teachers = await _context.Users
                                         .Where(u => u.Role == "Profesor")
                                         .ToListAsync();

            if (!teachers.Any())
            {
                ModelState.AddModelError("", "No se puede crear el curso: Actualmente no hay profesores disponibles para asignar.");
                ViewBag.TeacherList = new SelectList(new List<User>(), "UserId", "FullName"); // Keep ViewBag populated
                return View(course); // Return view with errors
            }

            // --- Date Validations ---
            // 1. Fecha de fin cannot be before Fecha de inicio
            if (course.EndDate < course.StartDate)
            {
                ModelState.AddModelError("EndDate", "La fecha de fin no puede ser anterior a la fecha de inicio.");
            }
            // 2. Fecha de inicio cannot be in the past (only if it's a new course and not an edit)
            // If you want to allow creating courses with a past start date, remove this validation.
            // If you want to allow editing a course to have a past start date (e.g., if it already started), handle it differently.
            if (course.StartDate < DateTime.Today)
            {
                ModelState.AddModelError("StartDate", "La fecha de inicio no puede ser una fecha pasada.");
            }
            // 3. Fecha de fin cannot be in the past (and should be at least today)
            if (course.EndDate < DateTime.Today)
            {
                ModelState.AddModelError("EndDate", "La fecha de fin no puede ser una fecha pasada.");
            }
            // --- End Date Validations ---


                _context.Add(course);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Curso creado exitosamente.";
                return RedirectToAction(nameof(CourseManagement));
            

            // If model is invalid, re-populate ViewBag for dropdown before returning view
            ViewBag.TeacherList = new SelectList(teachers, "UserId", "FullName", course.TeacherId);

            return View(course);
        }

       
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                                       .Include(c => c.Teacher)
                                       .FirstOrDefaultAsync(m => m.CourseId == id);
            if (course == null)
            {
                return NotFound();
            }

            var teachers = await _context.Users
                                         .Where(u => u.Role == "Profesor")
                                         .ToListAsync();
            ViewBag.TeacherList = new SelectList(teachers, "UserId", "FullName", course.TeacherId);

            return View(course);
        }

        
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CourseId,Name,Description,Status,StartDate,EndDate,TeacherId")] Course course)
        {
            if (id != course.CourseId)
            {
                return NotFound();
            }

            // --- Date Validations for Edit ---
            // 1. Fecha de fin cannot be before Fecha de inicio
            if (course.EndDate < course.StartDate)
            {
                ModelState.AddModelError("EndDate", "La fecha de fin no puede ser anterior a la fecha de inicio.");
            }
            // You might adjust past date validation for edits.
            // For example, if a course already started, its StartDate can be in the past.
            // But if it's a future course, its StartDate shouldn't be moved to the past.
            // Or you might enforce EndDate >= StartDate (current date)
            if (course.EndDate < DateTime.Today && course.StartDate >= DateTime.Today) // If start date is future, end date can't be past
            {
                ModelState.AddModelError("EndDate", "La fecha de fin no puede ser una fecha pasada si la fecha de inicio es futura.");
            }
            else if (course.EndDate < course.StartDate) // More robust check
            {
                ModelState.AddModelError("EndDate", "La fecha de fin no puede ser anterior a la fecha de inicio.");
            }
            // If you want to ensure the EndDate is always today or in the future
            if (course.EndDate < DateTime.Today)
            {
                ModelState.AddModelError("EndDate", "La fecha de fin no puede ser una fecha pasada.");
            }
            // --- End Date Validations ---

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(course);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Curso actualizado exitosamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Courses.Any(e => e.CourseId == course.CourseId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(CourseManagement));
            }

            // If model is invalid, re-populate ViewBag for dropdown before returning view
            var teachers = await _context.Users
                                         .Where(u => u.Role == "Profesor")
                                         .ToListAsync();
            ViewBag.TeacherList = new SelectList(teachers, "UserId", "FullName", course.TeacherId);

            return View(course);
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(m => m.CourseId == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Curso eliminado exitosamente.";
            }
            else
            {
                TempData["ErrorMessage"] = "Curso no encontrado.";
            }
            return RedirectToAction(nameof(CourseManagement));
        }
    }
}