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
        public ObservableCollection<DemographicData> DemographicData { get; set; }
        private User _currentUser;
        private System.Timers.Timer _searchTimer;

        private bool _isAdmin;
        public bool IsAdmin
        {
            get { return _isAdmin; }
            set
            {
                _isAdmin = value;
                OnPropertyChanged();
            }
        }

        public MainPage()
        {
            InitializeComponent();
            DemographicData = new ObservableCollection<DemographicData>();
            BindingContext = this;
            IsAdmin = false;
            _searchTimer = new System.Timers.Timer
            {
                Interval = 500,
                AutoReset = false
            };
            _searchTimer.Elapsed += OnSearchTimerElapsed;

            LoadDataAsync();
            UpdateLoginStatus();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            try
            {
                using (var context = new ApplicationContext())
                {
                    var data = await context.DemographicData.Include(d => d.Region).ToListAsync();
                    DemographicData.Clear();
                    foreach (var item in data)
                    {
                        DemographicData.Add(item);
                    }
                    SortData();
                    regionCollectionView.ItemsSource = DemographicData;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load data: {ex.Message}", "OK");
            }
        }

        private async void AddButton(object sender, EventArgs e)
        {
            if (IsAdmin)
            {
                var addPage = new Add();
                addPage.RegionAdded += OnRegionAdded;
                await Navigation.PushAsync(addPage);
            }
            else
            {
                await DisplayAlert("Ошибка", "Доступ запрещен. Только администраторы могут добавлять данные.", "OK");
            }
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
                var editPage = new Edit(demographicData.Id);
                editPage.RegionEdited += OnRegionEdited;
                await Navigation.PushAsync(editPage);
            }
        }

        private async void OnRegionEdited(object sender, EventArgs e)
        {
            LoadDataAsync();
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
                using (var context = new ApplicationContext())
                {
                    var data = await context.DemographicData
                                            .Include(d => d.Region)
                                            .Where(d => d.Region.Name.Replace(" ", "").Contains(searchTerm))
                                            .ToListAsync();
                    DemographicData.Clear();
                    foreach (var item in data)
                    {
                        DemographicData.Add(item);
                    }
                    SortData();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Search failed: {ex.Message}", "OK");
            }
        }

        private void OnSortPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            SortData();
        }

        private void SortData()
        {
            List<DemographicData> sortedData;

            switch (sortPicker.SelectedIndex)
            {
                case 0:
                    sortedData = DemographicData.OrderBy(d => d.Region.Name).ToList();
                    break;
                case 1:
                    sortedData = DemographicData.OrderByDescending(d => d.Region.Name).ToList();
                    break;
                case 2:
                    sortedData = DemographicData.OrderBy(d => d.Population).ToList();
                    break;
                case 3:
                    sortedData = DemographicData.OrderByDescending(d => d.Population).ToList();
                    break;
                default:
                    sortedData = DemographicData.ToList();
                    break;
            }

            DemographicData.Clear();
            foreach (var item in sortedData)
            {
                DemographicData.Add(item);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
