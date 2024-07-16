using Microsoft.EntityFrameworkCore;
using Database.Models;
using Database;
using System.Collections.ObjectModel;
using System.Timers;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DemographicApp.Pages;

namespace DemographicApp
{
    public partial class MainPage : ContentPage, INotifyPropertyChanged
    {
        private readonly ApplicationContext _context;
        public ObservableCollection<DemographicData> DemographicData { get; set; }
        private User _currentUser;
        private System.Timers.Timer _searchTimer;

        private bool _isAdmin;
        public bool IsAdmin
        {
            get => _isAdmin;
            set
            {
                if (_isAdmin != value)
                {
                    _isAdmin = value;
                    OnPropertyChanged(nameof(IsAdmin));
                }
            }
        }

        public MainPage()
        {
            InitializeComponent();
            _context = new ApplicationContext();
            DemographicData = new ObservableCollection<DemographicData>();
            BindingContext = this;
            LoadDataAsync();
            UpdateLoginStatus();

            _searchTimer = new System.Timers.Timer();
            _searchTimer.Interval = 500;
            _searchTimer.Elapsed += OnSearchTimerElapsed;
            _searchTimer.AutoReset = false;
        }

        private async void LoadDataAsync()
        {
            DemographicData.Clear();
                var data = await _context.DemographicData.Include(d => d.Region).ToListAsync();
                foreach (var item in data)
                {
                    DemographicData.Add(item);
                }
                regionCollectionView.ItemsSource = DemographicData;
        }

        private async void AddButton(object sender, EventArgs e)
        {
            var addPage = new Add();
            addPage.RegionAdded += OnRegionAdded;
            await Navigation.PushAsync(addPage);
        }

        private async void OnRegionAdded(object sender, EventArgs e)
        {
            LoadDataAsync();
        }

        private async void EditButton(object sender, EventArgs e)
        {
            var button = sender as Button;
            var demographicData = button.BindingContext as DemographicData;
            if (demographicData != null)
            {
                await Navigation.PushAsync(new Edit(demographicData.Id));
            }
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

        private void LogoutButton(object sender, EventArgs e)
        {
            _currentUser = null;
            IsAdmin = false;
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

                IsAdmin = _currentUser.RoleId == 1;
            }
            else
            {
                userLabel.IsVisible = false;
                loginButton.IsVisible = true;
                logoutButton.IsVisible = false;
                IsAdmin = false;
            }
        }

        private void OnLoginSuccess(User user)
        {
            _currentUser = user;
            UpdateLoginStatus();
        }

        private void SearchEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            _searchTimer.Stop();
            _searchTimer.Start();
        }

        private async void OnSearchTimerElapsed(object sender, ElapsedEventArgs e)
        {
            await SearchAsync(searchEntry.Text);
        }

        private async Task SearchAsync(string searchTerm)
        {
            searchTerm = searchTerm.Replace(" ", "");
            try
            {
                DemographicData.Clear();
                var data = await _context.DemographicData
                                          .Include(d => d.Region)
                                          .Where(d => d.Region.Name.Replace(" ", "").Contains(searchTerm))
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}