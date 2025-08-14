using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using StudentManager.Models;
using StudentManager.Services;

namespace StudentManager
{
    public partial class MainWindow : Window
    {
        private readonly DataService _dataService = new();
        private readonly List<Student> _students = new();

        public MainWindow()
        {
            InitializeComponent();
            UpdateStats();

            btnOpenManagement.Click += BtnOpenManagement_Click;
            btnSaveToTxt.Click += (s, e) => SaveTxt();
            btnLoadFromTxt.Click += (s, e) => LoadTxt();
            btnSaveToJson.Click += (s, e) => SaveJson();
            btnLoadFromJson.Click += (s, e) => LoadJson();
        }

        private void BtnOpenManagement_Click(object sender, RoutedEventArgs e)
        {
            var win = new StudentManagerWindow(_students)
            {
                Owner = this
            };
            win.ShowDialog();
            UpdateStats();
        }

        private void SaveTxt()
        {
            try
            {
                if (_dataService.SaveToTextFile(_students))
                {
                    lblLastSave.Text = $"💾 Last save: {DateTime.Now:HH:mm}";
                    lblStatus.Text = $"Status: Saved TXT | DataService: OK | Students: {_students.Count}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                lblStatus.Text = "Status: Error saving TXT";
            }
        }

        private void LoadTxt()
        {
            try
            {
                _students.Clear();
                _students.AddRange(_dataService.LoadFromTextFile());

                lblLastLoad.Text = $"📂 Last load: {DateTime.Now:HH:mm}";
                lblStatus.Text = $"Status: Loaded TXT | DataService: OK | Students: {_students.Count}";

                ReindexIds();
                UpdateStats();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                lblStatus.Text = "Status: Error loading TXT";
            }
        }

        private void SaveJson()
        {
            try
            {
                if (_dataService.SaveToJsonFile(_students))
                {
                    lblLastSave.Text = $"💾 Last save: {DateTime.Now:HH:mm}";
                    lblStatus.Text = $"Status: Saved JSON | DataService: OK | Students: {_students.Count}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                lblStatus.Text = "Status: Error saving JSON";
            }
        }

        private void LoadJson()
        {
            try
            {
                _students.Clear();
                _students.AddRange(_dataService.LoadFromJsonFile());

                lblLastLoad.Text = $"📂 Last load: {DateTime.Now:HH:mm}";
                lblStatus.Text = $"Status: Loaded JSON | DataService: OK | Students: {_students.Count}";

                ReindexIds();
                UpdateStats();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                lblStatus.Text = "Status: Error loading JSON";
            }
        }

        private void ReindexIds()
        {
            for (int i = 0; i < _students.Count; i++)
            {
                _students[i].Id = i + 1;
            }
        }

        private void UpdateStats()
        {
            lblStudentsInMemory.Text = $"👥 Students: {_students.Count}";
            lblStudentsWithGrades.Text = $"📝 With grades: {_students.Count(s => s.Grades.Count > 0)}";

            var allGrades = _students.SelectMany(s => s.Grades).ToList();
            double avg = allGrades.Count == 0 ? 0.0 : allGrades.Average(g => g.Value);
            lblAverageGrade.Text = $"⭐ Average grade: {avg:0.00}";

            var top = _students
                .OrderByDescending(s => s.CalculateAverage())
                .FirstOrDefault(s => s.Grades.Count > 0);

            lblTopStudent.Text = top == null
                ? "🏆 Top student: none"
                : $"🏆 Top student: {top.FirstName} {top.LastName} ({top.CalculateAverage():0.00})";

            lblStatus.Text = $"Status: Ready | DataService: OK | Students: {_students.Count}";
        }
    }
}