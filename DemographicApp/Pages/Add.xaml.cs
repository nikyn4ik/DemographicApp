using Database.Models;
using Database;
using System;

namespace DemographicApp.Pages
{
    public partial class Add : ContentPage
    {
        public event EventHandler RegionAdded;

        public Add()
        {
            InitializeComponent();
        }

        private async void OnSaveButtonClicked(object sender, EventArgs e)
        {
            try
            {
                var regionName = RegionNameEntry.Text;
                if (int.TryParse(PopulationEntry.Text, out int population) &&
                    int.TryParse(BirthRateEntry.Text, out int birthRate) &&
                    int.TryParse(DeathRateEntry.Text, out int deathRate) &&
                    int.TryParse(MalePopulationEntry.Text, out int malePopulation) &&
                    int.TryParse(FemalePopulationEntry.Text, out int femalePopulation))
                {
                    using (var context = new ApplicationContext())
                    {
                        var region = new Database.Models.Region { Name = regionName };
                        context.Regions.Add(region);

                        var demographicData = new DemographicData
                        {
                            Region = region,
                            Population = population,
                            BirthRate = birthRate,
                            DeathRate = deathRate,
                            MalePopulation = malePopulation,
                            FemalePopulation = femalePopulation
                        };
                        context.DemographicData.Add(demographicData);
                        await context.SaveChangesAsync();
                    }

                    RegionAdded?.Invoke(this, EventArgs.Empty);
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Ошибка", "Некорректные данные", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка сохранения данных: {ex.Message}", "OK");
            }
        }
    }
}
