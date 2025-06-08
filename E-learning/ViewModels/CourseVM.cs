// Models/CourseViewModel.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace E_learning.Models
{
    public class CourseViewModel
    {
        public int CourseId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public string Status { get; set; } // e.g., "Activo", "Inactivo"

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        // Additional properties for display
        public string TeacherFullName { get; set; }

        public bool IsEnrolled { get; set; }
    }
}