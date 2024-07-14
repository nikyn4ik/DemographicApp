using System;
using Microsoft.Maui.Controls;
using Database;
using Database.Models;
using Region = Database.Models.Region;

namespace DemographicApp.Pages
{
    public partial class Add : ContentPage
    {
        private readonly ApplicationContext _context;

        public Add()
        {
            InitializeComponent();
            _context = new ApplicationContext();
        }

        private async void AddRegion_Clicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(regionNameEntry.Text))
            {
                await DisplayAlert("Ошибка", "Введите название региона", "OK");
                return;
            }

            if (!double.TryParse(latitudeEntry.Text, out double latitude) || !double.TryParse(longitudeEntry.Text, out double longitude))
            {
                await DisplayAlert("Ошибка", "Некорректные данные для широты или долготы", "OK");
                return;
            }

            var newRegion = new Region
            {
                Name = regionNameEntry.Text,
                Latitude = latitude,
                Longitude = longitude
            };

            _context.Regions.Add(newRegion);
            await _context.SaveChangesAsync();

            await DisplayAlert("Успех", "Новый регион успешно добавлен", "OK");

            // Переход на страницу MainPage и закрытие текущей страницы Add
            await Navigation.PushAsync(new MainPage());
            Navigation.RemovePage(this);
        }
    }
}
