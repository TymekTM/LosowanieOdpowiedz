using System.IO;
using System.Collections.Generic;
using LosowanieOdpowiedz.Models;
using System.Text.Json;

namespace LosowanieOdpowiedz.Services
{
    public static class FileService
    {
        private static string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "LosowanieOdpowiedz");
        private static string filePath = Path.Combine(folderPath, "students.json");

        static FileService()
        {          
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }

        public static void SaveStudentsList(IEnumerable<Student> students)
        {
            var jsonString = JsonSerializer.Serialize(students);
            File.WriteAllText(filePath, jsonString);
        }

        public static IEnumerable<Student> LoadStudentsList()
        {
            if (!File.Exists(filePath)) return new List<Student>();
            var jsonString = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<IEnumerable<Student>>(jsonString);
        }

        public static IEnumerable<Student> LoadStudentsList(string filePath)
        {
            if (!File.Exists(filePath)) return new List<Student>();
            var jsonString = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<IEnumerable<Student>>(jsonString) ?? new List<Student>();
        }

    }
}
