using System.Collections.ObjectModel;
using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace DemographicApp.Pages
{
    public partial class Reports : ContentPage
    {
        private readonly ApplicationContext _context;

        public ObservableCollection<Report> ReportsCollection { get; set; }

        public Reports()
        {
            InitializeComponent();
            _context = new ApplicationContext();
            ReportsCollection = new ObservableCollection<Report>();
            BindingContext = this;

            LoadReports();
        }

        private async void LoadReports()
        {
            try
            {
                var reports = await _context.Reports.ToListAsync();
                ReportsCollection.Clear();
                foreach (var report in reports)
                {
                    ReportsCollection.Add(report);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка загрузки отчетов: {ex.Message}", "OK");
            }
        }

        private async void OnReportSelected(object sender, SelectionChangedEventArgs e)
        {
            var selectedReport = (Report)e.CurrentSelection.FirstOrDefault();
            if (selectedReport == null)
                return;

            string fileName = $"{selectedReport.Title}.pdf";
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Documentation", "Reports", fileName);

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
