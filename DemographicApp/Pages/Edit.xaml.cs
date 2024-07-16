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

        public event EventHandler RegionEdited;

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
                else
                {
                    await DisplayAlert("Ошибка", "Демографические данные не найдены", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка при загрузке данных: {ex.Message}", "OK");
            }
        }

        private async void SaveButtonClicked(object sender, EventArgs e)
        {
            try
            {
                if (_demographicData == null)
                {
                    await DisplayAlert("Ошибка", "Демографические данные не загружены", "OK");
                    return;
                }

                if (int.TryParse(populationEntry.Text, out int population) &&
                    int.TryParse(birthRateEntry.Text, out int birthRate) &&
                    int.TryParse(deathRateEntry.Text, out int deathRate) &&
                    int.TryParse(malePopulationEntry.Text, out int malePopulation) &&
                    int.TryParse(femalePopulationEntry.Text, out int femalePopulation))
                {
                    _demographicData.Population = population;
                    _demographicData.BirthRate = birthRate;
                    _demographicData.DeathRate = deathRate;
                    _demographicData.MalePopulation = malePopulation;
                    _demographicData.FemalePopulation = femalePopulation;

                    _context.DemographicData.Update(_demographicData);
                    await _context.SaveChangesAsync();

                    RegionEdited?.Invoke(this, EventArgs.Empty); // Вызываем событие при успешном сохранении

                    await DisplayAlert("Успех", "Данные обновлены", "OK");
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Ошибка", "Некорректный ввод данных", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Неудачное сохранение: {ex.Message}", "OK");
            }
        }
    }
}