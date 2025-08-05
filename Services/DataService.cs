using StudentManager.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace StudentManager.Services
{
    public class DataService
    {
        private readonly string _txtFilePath = "students.txt";
        private readonly string _jsonFilePath = "students.json";

        public bool SaveToTextFile(List<Student> students)
        {
            try
            {
                using var writer = new StreamWriter(_txtFilePath, false, Encoding.UTF8);
                writer.WriteLine("ID|FirstName|LastName|BirthDate|ClassName");

                foreach (var s in students)
                {
                    writer.WriteLine($"{s.Id}|{s.FirstName}|{s.LastName}|{s.BirthDate:dd.MM.yyyy}|{s.ClassName}");
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new IOException($"Error saving to TXT: {ex.Message}", ex);
            }
        }

        public List<Student> LoadFromTextFile()
        {
            var students = new List<Student>();

            if (!File.Exists(_txtFilePath))
            {
                throw new FileNotFoundException("TXT file does not exist.");
            }

            try
            {
                using var reader = new StreamReader(_txtFilePath, Encoding.UTF8);
                string? line;
                bool skipHeader = true;

                while ((line = reader.ReadLine()) != null)
                {
                    if (skipHeader)
                    {
                        skipHeader = false;
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var parts = line.Split('|');
                    if (parts.Length >= 5)
                    {
                        try
                        {
                            students.Add(new Student
                            {
                                Id = int.Parse(parts[0]),
                                FirstName = parts[1],
                                LastName = parts[2],
                                BirthDate = DateTime.ParseExact(parts[3], "dd.MM.yyyy", null),
                                ClassName = parts[4]
                            });
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new IOException($"Error reading from TXT: {ex.Message}", ex);
            }

            return students;
        }

        public bool SaveToJsonFile(List<Student> students)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                var json = JsonSerializer.Serialize(students, options);
                File.WriteAllText(_jsonFilePath, json, Encoding.UTF8);

                return true;
            }
            catch (Exception ex)
            {
                throw new IOException($"Error saving to JSON: {ex.Message}", ex);
            }
        }

        public List<Student> LoadFromJsonFile()
        {
            if (!File.Exists(_jsonFilePath))
            {
                throw new FileNotFoundException("JSON file does not exist.");
            }

            try
            {
                var json = File.ReadAllText(_jsonFilePath, Encoding.UTF8);
                return JsonSerializer.Deserialize<List<Student>>(json) ?? new List<Student>();
            }
            catch (Exception ex)
            {
                throw new IOException($"Error reading from JSON: {ex.Message}", ex);
            }
        }
    }
}