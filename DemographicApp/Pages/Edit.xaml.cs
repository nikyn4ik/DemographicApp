using Database.Models;
using Database;
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
                    RegionNameEntry.Text = _demographicData.Region.Name;
                    PopulationEntry.Text = _demographicData.Population.ToString();
                    BirthRateEntry.Text = _demographicData.BirthRate.ToString();
                    DeathRateEntry.Text = _demographicData.DeathRate.ToString();
                    MalePopulationEntry.Text = _demographicData.MalePopulation.ToString();
                    FemalePopulationEntry.Text = _demographicData.FemalePopulation.ToString();
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

                _demographicData.Region.Name = RegionNameEntry.Text;
                if (int.TryParse(PopulationEntry.Text, out int population) &&
                    int.TryParse(BirthRateEntry.Text, out int birthRate) &&
                    int.TryParse(DeathRateEntry.Text, out int deathRate) &&
                    int.TryParse(MalePopulationEntry.Text, out int malePopulation) &&
                    int.TryParse(FemalePopulationEntry.Text, out int femalePopulation))
                {
                    _demographicData.Population = population;
                    _demographicData.BirthRate = birthRate;
                    _demographicData.DeathRate = deathRate;
                    _demographicData.MalePopulation = malePopulation;
                    _demographicData.FemalePopulation = femalePopulation;

                    await _context.SaveChangesAsync();

                    RegionEdited?.Invoke(this, EventArgs.Empty);
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
