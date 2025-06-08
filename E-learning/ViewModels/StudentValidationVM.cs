// Models/ViewModels/StudentValidationViewModel.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace E_learning.Models.ViewModels
{
    public class StudentValidationViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public string CourseDescription { get; set; }
        public string TeacherFullName { get; set; }
        public List<StudentDetailViewModel> EnrolledStudents { get; set; } = new List<StudentDetailViewModel>();
    }
}
