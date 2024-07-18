using System.Collections.ObjectModel;
using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace DemographicApp.Pages
{
    public partial class Reports : ContentPage
    {
        private readonly ApplicationContext _context;
        private readonly string _reportsFolder;

        public ObservableCollection<string> ReportsCollection { get; set; }

        public Reports()
        {
            InitializeComponent();
            _context = new ApplicationContext();
            _reportsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Documentation", "Reports");
            ReportsCollection = new ObservableCollection<string>();
            BindingContext = this;

            LoadReports();
        }

        private void LoadReports()
        {
            try
            {
                if (Directory.Exists(_reportsFolder))
                {
                    var reportFiles = Directory.GetFiles(_reportsFolder, "*.pdf");
                    ReportsCollection.Clear();
                    foreach (var file in reportFiles)
                    {
                        ReportsCollection.Add(Path.GetFileName(file));
                    }
                }
                else
                {
                    DisplayAlert("Ошибка", "Папка для отчетов не найдена", "OK");
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Ошибка", $"Ошибка загрузки отчетов: {ex.Message}", "OK");
            }
        }

        private async void OnReportSelected(object sender, SelectionChangedEventArgs e)
        {
            var selectedReport = (string)e.CurrentSelection.FirstOrDefault();
            if (selectedReport == null)
                return;

            string filePath = Path.Combine(_reportsFolder, selectedReport);

            try
            {
                if (File.Exists(filePath))
                {
                    await Launcher.OpenAsync(new OpenFileRequest
                    {
                        File = new ReadOnlyFile(filePath)
                    });
                }
                else
                {
                    await DisplayAlert("Ошибка", "Файл отчета не найден", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка при открытии файла отчета: {ex.Message}", "OK");
            }

            ((CollectionView)sender).SelectedItem = null;
        }
    }
}
