using System;

namespace StudentManager.Models
{
    public class Grade
    {
        public string Subject { get; set; } = string.Empty;
        public double Value { get; set; }
        public DateTime DateIssued { get; set; } = DateTime.Today;
        public string Description { get; set; } = string.Empty;

        public bool IsValidGrade()
            => Value >= 1.0 && Value <= 6.0 && DateIssued <= DateTime.Today;

        public override string ToString()
            => $"{Subject}: {Value:0.0} ({DateIssued:dd.MM.yyyy}){(string.IsNullOrWhiteSpace(Description) ? "" : $" - {Description}")}";
    }
}