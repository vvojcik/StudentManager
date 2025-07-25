using System;

namespace StudentManager.Models
{
    public class Report
    {
        public string StudentName { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; } = DateTime.Now;
        public double AverageGrade { get; set; }
        public int GradeCount { get; set; }
        public string RandomComment { get; set; } = string.Empty;
        public string HighestGradeSubject { get; set; } = string.Empty;
    }
}