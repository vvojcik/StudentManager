using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using StudentManager.Models;
using StudentManager.Utils;

namespace StudentManager
{
    public partial class StudentManagerWindow : Window
    {
        private readonly List<Student> _students;
        private Student? _selected;

        public StudentManagerWindow(List<Student> students)
        {
            InitializeComponent();
            _students = students ?? new List<Student>();

            dpBirth.SelectedDate = DateTime.Today.AddYears(-16);
            dpGradeDate.SelectedDate = DateTime.Today;

            btnAddStudent.Click += (s, e) => AddStudent();
            btnEditStudent.Click += (s, e) => EditStudent();
            btnClearStudent.Click += (s, e) => ClearStudentForm();
            btnDeleteStudent.Click += (s, e) => DeleteStudent();

            RefreshStudentsList();
            UpdateStatus();
        }

        public StudentManagerWindow() : this(new List<Student>()) { }

        private void DpBirth_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dpBirth.SelectedDate is DateTime date)
            {
                if (date > DateTime.Today)
                {
                    MessageBox.Show("Birth date cannot be in the future.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    dpBirth.SelectedDate = DateTime.Today;
                }
                lblAge.Text = $"{(int)((DateTime.Now - date).TotalDays / 365.25)} years";
            }
        }

        private void LstStudents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selected = lstStudents.SelectedItem as Student;

            if (_selected != null)
            {
                lblGradesHeader.Text = $"Grades ({_selected.FirstName} {_selected.LastName})";
                lblReportHeader.Text = $"Report ({_selected.FirstName} {_selected.LastName})";
            }
            else
            {
                lblGradesHeader.Text = "Grades (...)";
                lblReportHeader.Text = "Report (...)";
            }

            FillFormFromSelected();
            RefreshGradesList();
            UpdateReport();
            UpdateStatus();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e) => Close();

        private void BtnAddGrade_Click(object sender, RoutedEventArgs e) => AddGrade();

        private void BtnDeleteGrade_Click(object sender, RoutedEventArgs e) => DeleteSelectedGrade();

        private void BtnRefreshGrades_Click(object sender, RoutedEventArgs e) => RefreshGradesList();

        private void BtnNewComment_Click(object sender, RoutedEventArgs e) => UpdateReport(true);

        private void BtnSaveReport_Click(object sender, RoutedEventArgs e) => SaveReportToFile();

        private void BtnExportTxt_Click(object sender, RoutedEventArgs e) => ExportCurrentStudentTxt();

        private void FillFormFromSelected()
        {
            if (_selected == null)
            {
                ClearStudentForm();
                return;
            }

            txtFirstName.Text = _selected.FirstName;
            txtLastName.Text = _selected.LastName;
            txtClass.Text = _selected.ClassName;
            dpBirth.SelectedDate = _selected.BirthDate;
            lblAge.Text = $"{_selected.Age} years";
        }

        private void ClearStudentForm()
        {
            txtFirstName.Text = string.Empty;
            txtLastName.Text = string.Empty;
            txtClass.Text = string.Empty;
            dpBirth.SelectedDate = DateTime.Today.AddYears(-16);
            lblAge.Text = string.Empty;
            UpdateStatus();
        }

        private void AddStudent()
        {
            if (string.IsNullOrWhiteSpace(txtFirstName.Text) || string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                MessageBox.Show("Please provide first and last name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var student = new Student
            {
                Id = _students.Count == 0 ? 1 : _students.Max(x => x.Id) + 1,
                FirstName = txtFirstName.Text.Trim(),
                LastName = txtLastName.Text.Trim(),
                ClassName = txtClass.Text.Trim(),
                BirthDate = dpBirth.SelectedDate ?? DateTime.Today
            };

            _students.Add(student);
            RefreshStudentsList();
            lstStudents.SelectedItem = student;
            UpdateStatus();
        }

        private void EditStudent()
        {
            if (_selected == null) return;

            _selected.FirstName = txtFirstName.Text.Trim();
            _selected.LastName = txtLastName.Text.Trim();
            _selected.ClassName = txtClass.Text.Trim();
            _selected.BirthDate = dpBirth.SelectedDate ?? DateTime.Today;

            RefreshStudentsList();
            UpdateStatus();
        }

        private void DeleteStudent()
        {
            if (_selected == null) return;

            if (MessageBox.Show("Delete selected student?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _students.Remove(_selected);
                _selected = null;
                RefreshStudentsList();
                RefreshGradesList();
                ClearStudentForm();
                UpdateReport();
                UpdateStatus();
            }
        }

        private void RefreshStudentsList()
        {
            lstStudents.ItemsSource = null;
            lstStudents.ItemsSource = _students;
        }

        private void AddGrade()
        {
            if (_selected == null)
            {
                MessageBox.Show("Please select a student first.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!double.TryParse(
                    txtGrade.Text.Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var val))
            {
                MessageBox.Show("Please enter a valid grade (e.g., 4.5).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var grade = new Grade
            {
                Subject = txtSubject.Text.Trim(),
                Value = val,
                DateIssued = dpGradeDate.SelectedDate ?? DateTime.Today,
                Description = txtDescription.Text.Trim()
            };

            if (!grade.IsValidGrade())
            {
                MessageBox.Show("Grade must be between 1.0 and 6.0 and date cannot be in the future.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _selected.Grades.Add(grade);
            RefreshGradesList();
            UpdateReport();
            UpdateStatus();
        }

        private void DeleteSelectedGrade()
        {
            if (_selected == null) return;

            if (lstGrades.SelectedItem is Grade grade)
            {
                _selected.Grades.Remove(grade);
                RefreshGradesList();
                UpdateReport();
                UpdateStatus();
            }
        }

        private void RefreshGradesList()
        {
            lstGrades.ItemsSource = null;
            lstGrades.ItemsSource = _selected?.Grades ?? new List<Grade>();
        }

        private void UpdateReport(bool commentOnly = false)
        {
            if (_selected == null)
            {
                lblStats.Text = "Grades: 0 | Average: 0.00";
                lblComment.Text = "No comment";
                UpdateStatus();
                return;
            }

            double avg = _selected.CalculateAverage();
            lblStats.Text = $"Grades: {_selected.Grades.Count} | Average: {avg:0.00}";

            lblComment.Text = commentOnly
                ? RandomCommentGenerator.GetRandomComment()
                : RandomCommentGenerator.GetCommentForAverage(avg);

            UpdateStatus();
        }

        private void SaveReportToFile()
        {
            if (_selected == null) return;

            var sb = new StringBuilder();
            sb.AppendLine($"Report: {_selected.FirstName} {_selected.LastName}");
            sb.AppendLine($"Average: {_selected.CalculateAverage():0.00}");
            sb.AppendLine($"Grade count: {_selected.Grades.Count}");
            sb.AppendLine($"Comment: {lblComment.Text}");

            try
            {
                File.WriteAllText("report.txt", sb.ToString(), Encoding.UTF8);
                MessageBox.Show("report.txt saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving report: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            UpdateStatus();
        }

        private void ExportCurrentStudentTxt()
        {
            if (_selected == null) return;

            var sb = new StringBuilder();
            sb.AppendLine($"{_selected.FirstName} {_selected.LastName} ({_selected.ClassName})");

            foreach (var g in _selected.Grades)
            {
                sb.AppendLine($"{g.Subject}: {g.Value:0.0} ({g.DateIssued:dd.MM.yyyy})");
            }

            try
            {
                File.WriteAllText("grades_export.txt", sb.ToString(), Encoding.UTF8);
                MessageBox.Show("grades_export.txt saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting grades: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            UpdateStatus();
        }

        private void UpdateStatus()
        {
            if (_selected != null)
            {
                string comment = string.IsNullOrWhiteSpace(lblComment.Text) ? "none" : lblComment.Text;
                int gradeCount = _selected.Grades?.Count ?? 0;
                double avg = gradeCount > 0 ? _selected.CalculateAverage() : 0.0;

                lblStatus.Text =
                    $"Status: {_selected.FirstName} {_selected.LastName} selected | " +
                    $"Grades: {gradeCount} | Average: {avg:0.00} | Comment: {comment}";
            }
            else
            {
                lblStatus.Text = "Status: no student selected | Grades: 0 | Average: 0.00 | Comment: none";
            }
        }
    }
}