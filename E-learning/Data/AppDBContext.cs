// Data/AppDBContext.cs
using Microsoft.EntityFrameworkCore;
using E_learning.Models;
// using System.Security.Claims; // No es necesario aquí, se puede eliminar

namespace E_learning.Data
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<StudentCourse> StudentCourses { get; set; }
        public DbSet<Grade> Grades { get; set; }

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

                tb.Property(col => col.FullName).IsRequired().HasMaxLength(50);
                tb.Property(col => col.Email).IsRequired().HasMaxLength(50);
                tb.HasIndex(col => col.Email).IsUnique();

                // Longitud de la contraseña a 128 para hashing
                tb.Property(col => col.Password).HasMaxLength(128);

                tb.Property(col => col.Role).IsRequired().HasMaxLength(20);

                // Configuración de las nuevas propiedades de perfil para User
                tb.Property(col => col.DateOfBirth).IsRequired(false); // Es nullable en el modelo
                tb.Property(col => col.Gender).HasMaxLength(10); // 'Masculino', 'Femenino', 'Otro'
                tb.Property(col => col.Country).HasMaxLength(100);
                tb.Property(col => col.NationalIdNumber).HasMaxLength(50);
                tb.Property(col => col.ProfilePicturePath).HasMaxLength(255); // Ruta a la imagen


                tb.ToTable("User"); // Asegura que el nombre de la tabla sea "User"
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

                c.HasOne(co => co.Teacher)
                   .WithMany(u => u.TaughtCourses)
                   .HasForeignKey(co => co.TeacherId)
                   .IsRequired(false)
                   .OnDelete(DeleteBehavior.Restrict);

                c.ToTable("Course");
            });

            // Configure the StudentCourse entity (Tabla de unión para muchos a muchos)
            modelBuilder.Entity<StudentCourse>(sc =>
            {
                sc.HasKey(col => col.StudentCourseId);
                sc.Property(col => col.StudentCourseId)
                    .UseIdentityColumn()
                    .ValueGeneratedOnAdd();

                sc.Property(col => col.EnrollmentDate).IsRequired();

                // Relación con Student (User)
                sc.HasOne(studentCourse => studentCourse.Student) // 'studentCourse' es el objeto StudentCourse, 'studentCourse.Student' es la propiedad de navegación
                  .WithMany(u => u.StudentCourses)
                  .HasForeignKey(studentCourse => studentCourse.StudentId) // Clave foránea en StudentCourse que apunta a User
                  .OnDelete(DeleteBehavior.Restrict);

                // Relación con Course
                sc.HasOne(studentCourse => studentCourse.Course) // 'studentCourse' es el objeto StudentCourse, 'studentCourse.Course' es la propiedad de navegación
                  .WithMany(c => c.StudentCourses)
                  .HasForeignKey(studentCourse => studentCourse.CourseId) // Clave foránea en StudentCourse que apunta a Course
                  .OnDelete(DeleteBehavior.Restrict);

                sc.ToTable("StudentCourse");
            });

            // Configurar la entidad Grade
            modelBuilder.Entity<Grade>(g =>
            {
                g.HasKey(col => col.GradeId);
                g.Property(col => col.GradeId)
                    .UseIdentityColumn()
                    .ValueGeneratedOnAdd();

                g.Property(gr => gr.Lapso1).HasColumnType("decimal(4, 2)").IsRequired(false);
                g.Property(gr => gr.Lapso2).HasColumnType("decimal(4, 2)").IsRequired(false);
                g.Property(gr => gr.Lapso3).HasColumnType("decimal(4, 2)").IsRequired(false);

                g.Property(gr => gr.FinalGrade).HasColumnType("decimal(4, 2)").IsRequired(false);
                g.Property(gr => gr.LapsoTotal).HasColumnType("decimal(4, 2)").IsRequired(false);

                g.HasOne(gr => gr.Student)
                    .WithMany(u => u.Grades)
                    .HasForeignKey(gr => gr.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);

                g.HasOne(gr => gr.Course)
                    .WithMany(c => c.Grades)
                    .HasForeignKey(gr => gr.CourseId)
                    .OnDelete(DeleteBehavior.Restrict);

                g.ToTable("Grade");
            });
        }
    }
}
