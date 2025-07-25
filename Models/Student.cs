using System;
using System.Collections.Generic;
using System.Linq;

namespace StudentManager.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; } = DateTime.Today;
        public string ClassName { get; set; } = string.Empty;
        public List<Grade> Grades { get; set; } = new();

        public int Age => (int)((DateTime.Now - BirthDate).TotalDays / 365.25);
        public double CalculateAverage() => Grades.Count == 0 ? 0.0 : Grades.Average(g => g.Value);

        public override string ToString()
            => $"{FirstName} {LastName} ({Age} years) - {ClassName} - {Grades.Count} grades";
    }
}