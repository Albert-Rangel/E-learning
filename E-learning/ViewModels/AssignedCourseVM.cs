// Models/ViewModels/AssignedCourseViewModel.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace E_learning.Models.ViewModels
{
    public class AssignedCourseViewModel
    {
        public int CourseId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
        public int NumberOfStudentsEnrolled { get; set; }
    }
}
