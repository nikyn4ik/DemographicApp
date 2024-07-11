using Database;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace DemographicApp.ViewModels
{
    public class AddRegionView : INotifyPropertyChanged
    {
        private string _name;
        private double _latitude;
        private double _longitude;
        private string _errorMessage;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public double Latitude
        {
            get => _latitude;
            set
            {
                _latitude = value;
                OnPropertyChanged();
            }
        }

        public double Longitude
        {
            get => _longitude;
            set
            {
                _longitude = value;
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

        public ICommand AddRegionCommand { get; }

        public AddRegionView()
        {
            AddRegionCommand = new Command(async () => await AddRegionAsync());
        }

        private async Task AddRegionAsync()
        {
            ErrorMessage = string.Empty;

            // Validate data before saving
            if (string.IsNullOrEmpty(Name))
            {
                ErrorMessage = "Название региона не может быть пустым.";
                return;
            }

            if (Latitude < -90 || Latitude > 90)
            {
                ErrorMessage = "Широта должна быть в диапазоне от -90 до 90.";
                return;
            }

            if (Longitude < -180 || Longitude > 180)
            {
                ErrorMessage = "Долгота должна быть в диапазоне от -180 до 180.";
                return;
            }

            try
            {
                using (var context = new ApplicationContext())
                {
                    var newRegion = new Database.Models.Region
                    {
                        Name = Name,
                        Latitude = Latitude,
                        Longitude = Longitude,
                    };

                    context.Regions.Add(newRegion);
                    await context.SaveChangesAsync();

                    // Clear fields after successful save
                    Name = string.Empty;
                    Latitude = 0;
                    Longitude = 0;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка при сохранении региона: {ex.Message}";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}