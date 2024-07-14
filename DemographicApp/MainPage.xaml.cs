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
        private User _currentUser;

        public MainPage()
        {
            InitializeComponent();
            _context = new ApplicationContext();
            DemographicData = new ObservableCollection<DemographicData>();
            BindingContext = this;
            LoadDataAsync();
            UpdateLoginStatus();
        }

        private async void LoadDataAsync()
        {
            try
            {
                DemographicData.Clear();
                var data = await _context.DemographicData.Include(d => d.Region).ToListAsync();
                foreach (var item in data)
                {
                    DemographicData.Add(item);
                }
                regionCollectionView.ItemsSource = DemographicData;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load data: {ex.Message}", "OK");
            }
        }

        private async void AddButton(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Add());
        }

        private async void EditButton(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Edit());
        }

        private async void CompareButton(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Compare());
        }

        private async void ReportsButton(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Reports());
        }

        private async void StatisticsButton(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Statistics());
        }

        private async void LoginButton(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Login(OnLoginSuccess));
        }

        private async void LogoutButton(object sender, EventArgs e)
        {
            _currentUser = null;
            UpdateLoginStatus();
        }

        private void UpdateLoginStatus()
        {
            if (_currentUser != null)
            {
                userLabel.Text = $"Добро пожаловать, {_currentUser.UserName}!";
                userLabel.IsVisible = true;
                loginButton.IsVisible = false;
                logoutButton.IsVisible = true;
            }
            else
            {
                userLabel.IsVisible = false;
                loginButton.IsVisible = true;
                logoutButton.IsVisible = false;
            }
        }

        private void OnLoginSuccess(User user)
        {
            _currentUser = user;
            UpdateLoginStatus();
        }

        private async void SearchEntry(object sender, EventArgs e)
        {
            string searchTerm = searchEntry.Text;
            try
            {
                DemographicData.Clear();
                var data = await _context.DemographicData
                                          .Include(d => d.Region)
                                          .Where(d => d.Region.Name.Contains(searchTerm))
                                          .ToListAsync();
                foreach (var item in data)
                {
                    DemographicData.Add(item);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Search failed: {ex.Message}", "OK");
            }
        }
    }
}
