// Models/ViewModels/CourseGradesViewModel.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace E_learning.Models.ViewModels
{
    public class CourseGradesViewModel
    {
        public int CourseId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
        public int NumberOfStudentsEnrolled { get; set; } // Útil para mostrar el número de estudiantes
    }
}


namespace E_learning.Models.ViewModels
{
    public class IndividualStudentGradeEntryViewModel
    {
        public int StudentId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }

        // Mensaje de error actualizado a español
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal? Lapso1 { get; set; }

        // Mensaje de error actualizado a español
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal? Lapso2 { get; set; }

        // Mensaje de error actualizado a español
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal? Lapso3 { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal TotalBalance // Propiedad calculada
        {
            get
            {
                int count = 0;
                decimal sum = 0;
                if (Lapso1.HasValue) { sum += Lapso1.Value; count++; }
                if (Lapso2.HasValue) { sum += Lapso2.Value; count++; }
                if (Lapso3.HasValue) { sum += Lapso3.Value; count++; }

                // Evitar división por cero
                return count > 0 ? sum / count : 0;
            }
        }
    }
}



namespace E_learning.Models.ViewModels
{
    public class CourseGradeEntryViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public string TeacherFullName { get; set; }
        public List<IndividualStudentGradeEntryViewModel> StudentGrades { get; set; } = new List<IndividualStudentGradeEntryViewModel>();
    }
}
