using System;
using System.Linq;
using System.Collections.Generic;
using LosowanieOdpowiedz.Models;

namespace LosowanieOdpowiedz.Services
{
    public class StudentService
    {
        private List<Student> students = new List<Student>();
        private Queue<Student> recentlyDrawnStudents = new Queue<Student>();
        private Random random = new Random();
        public int HappyNumber { get; set; }

        public StudentService()
        {
            LoadDefaultStudentsList();
            HappyNumber = GenerateHappyNumber();
        }

        private int GenerateHappyNumber()
        {
            return random.Next(1, 10);
        }

        public void LoadDefaultStudentsList()
        {
            var studentsDict = FileService.LoadStudentsList();
            students = studentsDict.SelectMany(pair => pair.Value).ToList();
        }

        public void UpdateStudentPresence(int studentId, bool isPresent)
        {
            var student = students.FirstOrDefault(s => s.Id == studentId);
            if (student != null)
            {
                student.IsPresent = isPresent;
                FileService.SaveStudentsList(students); 
        }
    }

        public void AddStudent(Student student)
        {
            if (string.IsNullOrEmpty(student.Class))
            {
                throw new InvalidOperationException("Klasa studenta nie może być pusta.");
            }

            var classStudents = students.Where(s => s.Class == student.Class).ToList();
            student.Id = classStudents.Any() ? classStudents.Max(s => s.Id) + 1 : 1;
            students.Add(student);
            FileService.SaveStudentsList(students);
        }



        public bool EditStudent(string currentName, string newName, bool? isPresent = null)
        {
            var student = students.FirstOrDefault(s => s.Name == currentName);
            if (student != null)
            {
                if (!string.IsNullOrWhiteSpace(newName)) student.Name = newName;
                if (isPresent.HasValue) student.IsPresent = isPresent.Value;
                FileService.SaveStudentsList(students);
                return true;
            }
            return false;
        }


        public Student DrawStudent(string klasa)
        {
            var eligibleStudents = students.Where(s => s.IsPresent && s.Id != HappyNumber && !recentlyDrawnStudents.Contains(s) && s.Class == klasa).ToList();

            if (eligibleStudents.Count == 0) return null;

            var drawnStudent = eligibleStudents[random.Next(eligibleStudents.Count)];

            recentlyDrawnStudents.Enqueue(drawnStudent);
            if (recentlyDrawnStudents.Count > 3)
            {
                var removedStudent = recentlyDrawnStudents.Dequeue();
                removedStudent.HasBeenQueried = false;
                FileService.SaveStudentsList(students);
            }
            FileService.SaveStudentsList(students);
            return drawnStudent;
        }



        public List<Student> GetAllStudents()
        {
            return students;
        }

        public void ResetQueryStatus()
        {
            students.ForEach(s => s.HasBeenQueried = false);
            FileService.SaveStudentsList(students);
        }
        public IEnumerable<Student> LoadStudents(string filePath)
        {
            var studentsDict = FileService.LoadStudentsFromFile(filePath);
            return studentsDict.SelectMany(pair => pair.Value);
        }
        public IEnumerable<string> GetAllClasses()
        {
            return students.Select(s => s.Class).Distinct();
        }

        public IEnumerable<Student> GetStudentsByClass(string className)
        {
            return students.Where(student => student.Class == className);
        }

    }
}
