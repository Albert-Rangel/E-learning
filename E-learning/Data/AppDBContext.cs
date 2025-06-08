using Microsoft.EntityFrameworkCore;
using E_learning.Models;
using System.Security.Claims; 

namespace E_learning.Data
{
    // If your User model is part of ASP.NET Core Identity, this should inherit from IdentityDbContext<User>
    // However, given your User entity configuration, it seems to be a custom User table, so DbContext is fine.
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<StudentCourse> StudentCourses { get; set; } // ADDED: DbSet for StudentCourse
        public DbSet<Grade> Grades { get; set; } // AÑADIDO: DbSet para Grade

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the User entity
            modelBuilder.Entity<User>(tb =>
            {
                tb.HasKey(col => col.UserId);
                tb.Property(col => col.UserId)
                    .UseIdentityColumn()
                    .ValueGeneratedOnAdd();

                tb.Property(col => col.FullName).HasMaxLength(50);
                tb.Property(col => col.Email).HasMaxLength(50);
                tb.Property(col => col.Password).HasMaxLength(50); // Be cautious about storing plain passwords
                tb.Property(col => col.Role).HasMaxLength(20); // Ensure Role property is configured

                // If User is a custom entity and not directly inheriting from IdentityUser,
                // and you want to manage roles, you might need to add role-related properties here.
                tb.ToTable("User"); // Ensure table name matches
            });

            // Configure the Course entity
            modelBuilder.Entity<Course>(c =>
            {
                c.HasKey(col => col.CourseId);
                c.Property(col => col.CourseId)
                    .UseIdentityColumn()
                    .ValueGeneratedOnAdd();

                c.Property(col => col.Name).IsRequired().HasMaxLength(100);
                c.Property(col => col.Description).HasMaxLength(500);
                c.Property(col => col.Status).IsRequired().HasMaxLength(50);

                c.Property(col => col.StartDate).IsRequired();
                c.Property(col => col.EndDate).IsRequired();

                c.HasOne(co => co.Teacher) // A Course has one Teacher
                 .WithMany(u => u.TaughtCourses) // Assuming User model has ICollection<Course> TaughtCourses
                 .HasForeignKey(co => co.TeacherId)
                 .IsRequired(false) // If TeacherId can be null
                 .OnDelete(DeleteBehavior.Restrict); // Prevent deleting a User if they are assigned to a course
                                                     // Use .OnDelete(DeleteBehavior.SetNull) if you want to clear TeacherId on teacher deletion

                c.ToTable("Course");
            });

            // ADDED: Configure the StudentCourse entity
            modelBuilder.Entity<StudentCourse>(sc =>
            {
                sc.HasKey(col => col.StudentCourseId);
                sc.Property(col => col.StudentCourseId)
                    .UseIdentityColumn()
                    .ValueGeneratedOnAdd();

                sc.Property(col => col.EnrollmentDate).IsRequired();

                // Relationship: StudentCourse has one Student (User)
                sc.HasOne(s => s.Student)
                  .WithMany(u => u.StudentCourses) // Assuming User model has ICollection<StudentCourse> StudentCourses
                  .HasForeignKey(s => s.StudentId)
                  .OnDelete(DeleteBehavior.Restrict); // Or .Cascade, depending on your desired behavior

                // Relationship: StudentCourse has one Course
                sc.HasOne(s => s.Course)
                  .WithMany(c => c.StudentCourses) // Assuming Course model has ICollection<StudentCourse> StudentCourses
                  .HasForeignKey(s => s.CourseId)
                  .OnDelete(DeleteBehavior.Restrict); // Or .Cascade

                sc.ToTable("StudentCourse"); // Name your join table
            });

            // AÑADIDO: Configurar la entidad Grade
            modelBuilder.Entity<Grade>(g =>
            {
                g.HasKey(col => col.GradeId);
                g.Property(col => col.GradeId)
                    .UseIdentityColumn()
                    .ValueGeneratedOnAdd();

                g.HasOne(gr => gr.Student) // Una Nota tiene un Estudiante
                    .WithMany(u => u.Grades) // Un Estudiante puede tener muchas Notas
                    .HasForeignKey(gr => gr.StudentId)
                    .OnDelete(DeleteBehavior.Restrict); // Opcional: restringir eliminación

                g.HasOne(gr => gr.Course) // Una Nota pertenece a un Curso
                    .WithMany(c => c.Grades) // Un Curso puede tener muchas Notas
                    .HasForeignKey(gr => gr.CourseId)
                    .OnDelete(DeleteBehavior.Restrict); // Opcional: restringir eliminación

                g.ToTable("Grade"); // Nombre de la tabla
            });
        }
    }
}
