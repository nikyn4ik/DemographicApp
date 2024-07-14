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
            var reports = await _context.Reports.ToListAsync();
            foreach (var report in reports)
            {
                ReportsCollection.Add(report);
            }
        }

        private async void OnReportSelected(object sender, SelectionChangedEventArgs e)
        {
            var selectedReport = (Report)e.CurrentSelection.FirstOrDefault();
            if (selectedReport == null)
                return;

            string fileName = Path.Combine(FileSystem.AppDataDirectory, $"{selectedReport.Title}.pdf");
            File.WriteAllBytes(fileName, Convert.FromBase64String(selectedReport.ReportData));

            await Launcher.OpenAsync(new OpenFileRequest
            {
                File = new ReadOnlyFile(fileName)
            });

            ((CollectionView)sender).SelectedItem = null; // Deselect the item
        }
    }
}
