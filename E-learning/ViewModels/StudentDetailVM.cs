// Models/ViewModels/StudentDetailViewModel.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace E_learning.Models.ViewModels
{
    public class StudentDetailViewModel
    {
        public int StudentId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        [DataType(DataType.Date)]
        public DateTime EnrollmentDate { get; set; }
        // Puedes añadir más propiedades relevantes del estudiante si es necesario para la validación
    }
}
