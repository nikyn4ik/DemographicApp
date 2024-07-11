using Database;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace DemographicApp.ViewModels
{
    public class EditRegionView : INotifyPropertyChanged
    {
        private int _id;
        private string _name;
        private double _latitude;
        private double _longitude;
        private string _errorMessage;

        public int Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }

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

        public ICommand SaveCommand { get; }

        public EditRegionView()
        {
            SaveCommand = new Command(async () => await SaveRegionAsync());
        }

        public async Task LoadRegionAsync(int id)
        {
            try
            {
                using (var context = new ApplicationContext())
                {
                    var region = await context.Regions.FindAsync(id);
                    if (region != null)
                    {
                        Id = region.Id;
                        Name = region.Name;
                        Latitude = region.Latitude;
                        Longitude = region.Longitude;
                    }
                    else
                    {
                        ErrorMessage = "Регион не найден.";
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Произошла ошибка при загрузке региона: {ex.Message}";
            }
        }

        private async Task SaveRegionAsync()
        {
            ErrorMessage = string.Empty;

            // Валидация данных перед сохранением
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
                    var region = await context.Regions.FindAsync(Id);
                    if (region != null)
                    {
                        region.Name = Name;
                        region.Latitude = Latitude;
                        region.Longitude = Longitude;
                        await context.SaveChangesAsync();
                    }
                    else
                    {
                        ErrorMessage = "Регион не найден.";
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка при сохранении региона в базу данных: {ex.Message} " +
                               $"{(ex.InnerException != null ? ex.InnerException.Message : string.Empty)}";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
