﻿using Database;
using Database.Models;
using DemographicApp.Pages;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Timers;

namespace DemographicApp
{
    public partial class MainPage : ContentPage
    {
        private readonly ApplicationContext _context;
        public ObservableCollection<DemographicData> DemographicData { get; set; }
        private User _currentUser;
        private System.Timers.Timer _searchTimer;

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
    }
}