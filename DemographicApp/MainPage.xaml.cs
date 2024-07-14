using Database;
using Database.Models;
using DemographicApp.Pages;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace DemographicApp
{
    public partial class MainPage : ContentPage
    {
        private readonly ApplicationContext _context;
        public ObservableCollection<DemographicData> DemographicData { get; set; }

        public MainPage()
        {
            InitializeComponent();

            _context = new ApplicationContext();

            DemographicData = new ObservableCollection<DemographicData>();
            LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            DemographicData.Clear();
            var data = await _context.DemographicData.Include(d => d.Region).ToListAsync();
            foreach (var item in data)
            {
                DemographicData.Add(item);
            }
            regionCollectionView.ItemsSource = DemographicData;
        }

        private void AddButton(object sender, EventArgs e)
        {
            Navigation.PushAsync(new Add());
        }

        private void EditButton(object sender, EventArgs e)
        {
            Navigation.PushAsync(new Edit());
        }

        private void CompareButton(object sender, EventArgs e)
        {
            Navigation.PushAsync(new Compare());
        }

        private void ReportsButton(object sender, EventArgs e)
        {
            Navigation.PushAsync(new Reports());
        }

        private void StatisticsButton(object sender, EventArgs e)
        {
            Navigation.PushAsync(new Statistics());
        }

        private void LoginButton(object sender, EventArgs e)
        {
            Navigation.PushAsync(new Login());
        }

        private async void SearchEntry(object sender, EventArgs e)
        {
            string searchTerm = searchEntry.Text;
            DemographicData.Clear();
            var data = await _context.DemographicData
                                      .Include(d => d.Region)
                                      .Where(d => d.Region.Name.Contains(searchTerm))
                                      .ToListAsync();
            foreach (var item in data)
            {
                DemographicData.Add(item);
            }
            regionCollectionView.ItemsSource = DemographicData;
        }
    }
}