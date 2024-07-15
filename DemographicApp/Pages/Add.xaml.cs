using Database;
using Database.Models;

namespace DemographicApp.Pages
{
    public partial class Add : ContentPage
    {
        private readonly ApplicationContext _context;
        public event EventHandler RegionAdded;

        public Add()
        {
            InitializeComponent();
            _context = new ApplicationContext();
        }

        private async void AddRegion_Clicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(regionNameEntry.Text))
            {
                await DisplayAlert("������", "������� �������� �������", "OK");
                return;
            }

            if (!double.TryParse(latitudeEntry.Text, out double latitude) || !double.TryParse(longitudeEntry.Text, out double longitude))
            {
                await DisplayAlert("������", "������������ ������ ��� ������ ��� �������", "OK");
                return;
            }

            if (!int.TryParse(populationEntry.Text, out int population))
            {
                await DisplayAlert("������", "������������ ������ ��� ����������� ���������", "OK");
                return;
            }

            if (!int.TryParse(birthRateEntry.Text, out int birthRate))
            {
                await DisplayAlert("������", "������������ ������ ��� �����������", "OK");
                return;
            }

            if (!int.TryParse(deathRateEntry.Text, out int deathRate))
            {
                await DisplayAlert("������", "������������ ������ ��� ����������", "OK");
                return;
            }

            if (!int.TryParse(malePopulationEntry.Text, out int malePopulation))
            {
                await DisplayAlert("������", "������������ ������ ��� �������� ���������", "OK");
                return;
            }

            if (!int.TryParse(femalePopulationEntry.Text, out int femalePopulation))
            {
                await DisplayAlert("������", "������������ ������ ��� �������� ���������", "OK");
                return;
            }

            var newRegion = new Database.Models.Region
            {
                Name = regionNameEntry.Text,
                Latitude = latitude,
                Longitude = longitude
            };

            var newDemographicData = new DemographicData
            {
                Region = newRegion,
                Date = DateTime.Now,
                Population = population,
                BirthRate = birthRate,
                DeathRate = deathRate,
                MalePopulation = malePopulation,
                FemalePopulation = femalePopulation
            };

            _context.DemographicData.Add(newDemographicData);
            await _context.SaveChangesAsync();

            await DisplayAlert("�����", "����� ������ � ������ ������� ���������", "OK");
            RegionAdded?.Invoke(this, EventArgs.Empty);
            await Navigation.PopAsync();
        }
    }
}
