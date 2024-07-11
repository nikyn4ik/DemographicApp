using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Input;
using Region = Database.Models.Region;

namespace DemographicApp.ViewModels
{
    public class AddViewModel : INotifyPropertyChanged
    {
        private Region _selectedRegion;
        private DemographicData _demographicData;
        private string _errorMessage;
        private static readonly HttpClient httpClient = new HttpClient();

        public ObservableCollection<Region> Regions { get; set; }
        public Region SelectedRegion
        {
            get => _selectedRegion;
            set
            {
                _selectedRegion = value;
                OnPropertyChanged();
                LoadDemographicData();
            }
        }
        public DemographicData DemographicData
        {
            get => _demographicData;
            set
            {
                _demographicData = value;
                OnPropertyChanged();
            }
        }
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }
        public ICommand SaveCommand { get; }

        public AddViewModel()
        {
            Regions = new ObservableCollection<Region>();
            DemographicData = new DemographicData { Date = DateTime.Now };
            SaveCommand = new Command(Save);

            LoadRegions();
        }

        private async void LoadRegions()
        {
            try
            {
                var regionsFromInternet = await FetchRegionsFromInternet();
                foreach (var region in regionsFromInternet)
                {
                    Regions.Add(region);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки регионов: {ex.Message}";
            }
        }

        private async void LoadDemographicData()
        {
            if (SelectedRegion != null)
            {
                using (var db = new ApplicationContext())
                {
                    var data = await db.DemographicData
                                       .FirstOrDefaultAsync(d => d.RegionId == SelectedRegion.Id);
                    if (data != null)
                    {
                        DemographicData = data;
                    }
                    else
                    {
                        DemographicData = new DemographicData
                        {
                            RegionId = SelectedRegion.Id,
                            Date = DateTime.Now
                        };
                    }
                }
            }
        }

        private async void Save()
        {
            using (var db = new ApplicationContext())
            {
                if (SelectedRegion == null)
                {
                    ErrorMessage = "Выберите регион.";
                    return;
                }

                db.Entry(DemographicData).State = DemographicData.Id == 0 ? EntityState.Added : EntityState.Modified;
                await db.SaveChangesAsync();

                ErrorMessage = "Данные успешно сохранены.";
                await Application.Current.MainPage.Navigation.PopAsync();
            }
        }

        private async Task<IEnumerable<Region>> FetchRegionsFromInternet()
        {
            var url = "https://api.example.com/regions"; // Замените на реальный URL вашего API
            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Ошибка получения регионов: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var regions = JsonSerializer.Deserialize<List<Region>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return regions;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}