// Models/StudentCourse.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_learning.Models
{
    public class StudentCourse
    {
        [Key]
        public int StudentCourseId { get; set; }

        [Required]
        public int StudentId { get; set; } // IMPORTANT: This should be int to match User.UserId

        [Required]
        public int CourseId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EnrollmentDate { get; set; }

        // Navigation properties
        [ForeignKey("StudentId")]
        public virtual User Student { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }
    }
}