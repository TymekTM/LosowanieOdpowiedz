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
            LoadDefaultList();
            HappyNumberLabel.Text = $"Szczęśliwy Numerek: {studentService.HappyNumber}";
        }


        private async void OnLoadStudentsButtonClicked(object sender, EventArgs e)
        {
            var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.WinUI, new[] { ".json" } },
            { DevicePlatform.Android, new[] { "application/json" } },
            { DevicePlatform.iOS, new[] { "public.json" } }
        });

            var options = new PickOptions
            {
                PickerTitle = "Wybierz plik z listą uczniów",
                FileTypes = customFileType,
            };

            var result = await FilePicker.PickAsync(options);
            if (result != null)
            {
                Students.Clear();
                var loadedStudents = studentService.LoadStudents(result.FullPath);
                foreach (var student in loadedStudents)
                {
                    Students.Add(student);
                }
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
            var drawnStudent = studentService.DrawStudent();
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
            Students.Clear();
            var allStudents = studentService.GetAllStudents();
            foreach (var student in allStudents)
            {
                Students.Add(student);
            }
        }

        private void OnAddStudentButtonClicked(object sender, EventArgs e)
        {
            var studentName = StudentNameEntry.Text?.Trim();
            if (!string.IsNullOrEmpty(studentName))
            {
                var newStudent = new Student { Name = studentName, IsPresent = true };
                studentService.AddStudent(newStudent);
                Students.Add(newStudent);
                StudentNameEntry.Text = string.Empty;
            }
            else
            {
                DisplayAlert("Błąd", "Musisz wpisać imię i nazwisko ucznia.", "OK");
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
                    // If the student was successfully edited, update the UI accordingly.
                    var index = Students.IndexOf(student);
                    if (index != -1)
                    {
                        Students[index] = new Student { Id = student.Id, Name = newName, IsPresent = student.IsPresent };
                    }
                }
                else
                {
                    // Handle the error case where the student wasn't found for editing.
                    await DisplayAlert("Błąd", "Nie znaleziono studenta do edycji.", "OK");
                }
            }
        }


        private async void LoadDefaultList()
        {
            // Assuming 'default_students.json' is the name of the default file and it's an EmbeddedResource
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(MainPage)).Assembly;
            Stream stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.students.json");

            if (stream != null)
            {
                using (var reader = new StreamReader(stream))
                {
                    var fileContents = await reader.ReadToEndAsync();
                    var defaultStudents = JsonConvert.DeserializeObject<List<Student>>(fileContents);
                    foreach (var student in defaultStudents)
                    {
                        Students.Add(student);
                    }
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
