using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace DemographicApp.Pages
{
    public partial class Edit : ContentPage
    {
        private readonly ApplicationContext _context;
        private int _demographicDataId;
        private DemographicData _demographicData;

        public Edit(int demographicDataId)
        {
            InitializeComponent();
            _context = new ApplicationContext();
            _demographicDataId = demographicDataId;
            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            try
            {
                _demographicData = await _context.DemographicData
                                                 .Include(d => d.Region)
                                                 .FirstOrDefaultAsync(d => d.Id == _demographicDataId);

                if (_demographicData != null)
                {
                    regionNameEntry.Text = _demographicData.Region.Name;
                    populationEntry.Text = _demographicData.Population.ToString();
                    birthRateEntry.Text = _demographicData.BirthRate.ToString();
                    deathRateEntry.Text = _demographicData.DeathRate.ToString();
                    malePopulationEntry.Text = _demographicData.MalePopulation.ToString();
                    femalePopulationEntry.Text = _demographicData.FemalePopulation.ToString();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не обновился: {ex.Message}", "OK");
            }
        }

        private async void SaveButtonClicked(object sender, EventArgs e)
        {
            try
            {
                _demographicData.Population = int.Parse(populationEntry.Text);
                _demographicData.BirthRate = int.Parse(birthRateEntry.Text);
                _demographicData.DeathRate = int.Parse(deathRateEntry.Text);
                _demographicData.MalePopulation = int.Parse(malePopulationEntry.Text);
                _demographicData.FemalePopulation = int.Parse(femalePopulationEntry.Text);

                _context.DemographicData.Update(_demographicData);
                await _context.SaveChangesAsync();

                await DisplayAlert("Успех", "Данные обновлены", "OK");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Неудачное сохранение: {ex.Message}", "OK");
            }
        }
    }
}