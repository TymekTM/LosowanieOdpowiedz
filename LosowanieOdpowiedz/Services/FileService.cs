using System.IO;
using System.Collections.Generic;
using LosowanieOdpowiedz.Models;
using System.Text.Json;

namespace LosowanieOdpowiedz.Services
{
    public static class FileService
    {
        private static string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "LosowanieOdpowiedz");
        private static string filePath = Path.Combine(folderPath, "students.txt");

        static FileService()
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }

        public static void SaveStudentsList(IEnumerable<Student> students)
        {
            var studentsByClass = new Dictionary<string, List<Student>>();
            foreach (var student in students)
            {
                if (!studentsByClass.ContainsKey(student.Class))
                {
                    studentsByClass[student.Class] = new List<Student>();
                }
                studentsByClass[student.Class].Add(student);
            }

            var jsonString = JsonSerializer.Serialize(studentsByClass, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, jsonString);
        }

        public static Dictionary<string, List<Student>> LoadStudentsList()
        {
            if (!File.Exists(filePath)) return new Dictionary<string, List<Student>>();
            var jsonString = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<Dictionary<string, List<Student>>>(jsonString) ?? new Dictionary<string, List<Student>>();
        }

        public static Dictionary<string, List<Student>> LoadStudentsFromFile(string customFilePath)
        {
            if (!File.Exists(customFilePath)) return new Dictionary<string, List<Student>>();
            var jsonString = File.ReadAllText(customFilePath);
            if (jsonString == string.Empty)
            {
                return new Dictionary<string, List<Student>>();
            }
            else
            {
                return JsonSerializer.Deserialize<Dictionary<string, List<Student>>>(jsonString) ?? new Dictionary<string, List<Student>>();
            }
        }

        public static void UpdateFilePath(string newFilePath)
        {
            filePath = newFilePath;
        }


    }
}
