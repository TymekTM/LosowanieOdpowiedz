using LosowanieOdpowiedz.Services;
using LosowanieOdpowiedz.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Reflection;
using Newtonsoft.Json;

namespace LosowanieOdpowiedz
{
    public partial class MainPage : ContentPage
    {
        private StudentService studentService;
        public ObservableCollection<Student> Students { get; private set; }
        public ICommand EditStudentCommand { get; private set; }
        

        public MainPage()
        {
            InitializeComponent();
            studentService = new StudentService();
            Students = new ObservableCollection<Student>(studentService.GetAllStudents());
            StudentsCollectionView.ItemsSource = Students;
            HappyNumberLabel.Text = $"Szczęśliwy Numerek: {studentService.HappyNumber}";

            LoadClassesIntoPicker();
        }

        private void LoadClassesIntoPicker()
        {
            var classes = studentService.GetAllClasses().ToList();
            var currentClasses = ClassPicker.ItemsSource as List<string> ?? new List<string>();

            if (!classes.SequenceEqual(currentClasses))
            {
                ClassPicker.ItemsSource = classes;
            }
        }


        private void ClassPicker_Clicked(object sender, EventArgs e)
        {
            LoadClassesIntoPicker();
        }

        private void ClassPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ClassPicker.SelectedIndex != -1)
            {
                RefreshStudentsList();
            }
        }


        private async void OnLoadStudentsButtonClicked(object sender, EventArgs e)
        {
            var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.WinUI, new[] { ".txt" } },
            { DevicePlatform.Android, new[] { "text/txt" } },
            { DevicePlatform.iOS, new[] { "public.text" } }
        });

            var options = new PickOptions
            {
                PickerTitle = "Wybierz plik z listą uczniów",
                FileTypes = customFileType,
            };

            var result = await FilePicker.PickAsync(options);
            if (result != null)
            {
                studentService.ResetState();
                var loadedStudents = studentService.LoadStudents(result.FullPath);
                Students.Clear();
                foreach (var student in loadedStudents)
                {
                    Students.Add(student);
                }
                FileService.UpdateFilePath(result.FullPath);

                LoadClassesIntoPicker(); 
                RefreshStudentsList();
            }

        }
        private void CheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.BindingContext is Student student)
            {
                studentService.UpdateStudentPresence(student.Id, checkBox.IsChecked);
            }
        }

        private void OnDrawStudentButtonClicked(object sender, EventArgs e)
        {
            var drawnStudent = studentService.DrawStudent((string)ClassPicker.SelectedItem);
            if (drawnStudent != null)
            {
                DisplayAlert("Wylosowany Student", $"{drawnStudent.Name}", "OK");
                RefreshStudentsList();
            }
            else
            {
                DisplayAlert("Błąd", "Nie udało się wylosować studenta. Sprawdź kryteria losowania.", "OK");
            }
        }

        private void RefreshStudentsList()
        {
            if (ClassPicker.SelectedItem == null) return;

            var selectedClass = ClassPicker.SelectedItem.ToString();
            var filteredStudents = studentService.GetStudentsByClass(selectedClass).ToList();

            if (!filteredStudents.SequenceEqual(Students))
            {
                Students.Clear();
                foreach (var student in filteredStudents)
                {
                    Students.Add(student);
                }
            }
        }


        private void OnAddStudentButtonClicked(object sender, EventArgs e)
        {
            var studentName = StudentNameEntry.Text?.Trim();
            var studentClass = ClassEntry.Text?.Trim(); 

            if (!string.IsNullOrEmpty(studentName) && !string.IsNullOrEmpty(studentClass))
            {
                var newStudent = new Student { Name = studentName, Class = studentClass, IsPresent = true };
                studentService.AddStudent(newStudent);
                Students.Add(newStudent);
                StudentNameEntry.Text = string.Empty;
                ClassEntry.Text = string.Empty;

                LoadClassesIntoPicker();
                RefreshStudentsList();
            }
            else
            {
                DisplayAlert("Błąd", "Musisz wpisać imię, nazwisko ucznia oraz klasę.", "OK");
            }
        }

        public async Task EditStudent(Student student)
        {
            System.Diagnostics.Debug.WriteLine($"Edit command executed for student: {student.Name}");

            string newName = await DisplayPromptAsync("Edycja", "Wpisz nowe imię i nazwisko ucznia", initialValue: student.Name);
            if (!string.IsNullOrWhiteSpace(newName))
            {
                bool isEdited = studentService.EditStudent(student.Name, newName, student.IsPresent);
                if (isEdited)
                {
                    var index = Students.IndexOf(student);
                    if (index != -1)
                    {
                        Students[index] = new Student { Id = student.Id, Name = newName, IsPresent = student.IsPresent };
                    }
                }
                else
                {
                    await DisplayAlert("Błąd", "Nie znaleziono studenta do edycji.", "OK");
                }
            }
        }

        private async void OnEditButtonClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is Student student)
            {
                await EditStudent(student);
            }
        }

    }
}
