using System.Collections.ObjectModel;
using System.Windows.Input;
using ApplicationContext = Database.ApplicationContext;
using Database.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DemographicApp.ViewModels
{
    public class MainPageView : ViewModelBase
    {
        private ObservableCollection<Models.Region> _regions;
        private ObservableCollection<Models.Region> _sortedRegions;
        private string _searchText;
        private User _currentUser;

        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Models.Region> Regions
        {
            get => _regions;
            set => SetProperty(ref _regions, value);
        }

        public ObservableCollection<Models.Region> SortedRegions
        {
            get => _sortedRegions;
            set => SetProperty(ref _sortedRegions, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                ApplySearchFilter();
            }
        }

        public ICommand AddCommand { get; }
        public ICommand CompareCommand { get; }
        public ICommand ReportingCommand { get; }
        public ICommand StatisticsCommand { get; }
        public ICommand LoginCommand { get; }
        public ICommand SearchCommand { get; }

        public MainPageView()
        {
            LoadData();

            AddCommand = new Command(OnAdd, CanAddRegion);
            CompareCommand = new Command(OnCompare);
            ReportingCommand = new Command(OnReporting);
            StatisticsCommand = new Command(OnStatistics);
            LoginCommand = new Command(OnLogin);
            SearchCommand = new Command(OnSearch);

            CurrentUser = CurrentUser.Current;
        }

        private void LoadData()
        {
            using (var context = new ApplicationContext())
            {
                Regions = new ObservableCollection<Models.Region>((IEnumerable<Models.Region>)context.Regions.ToList());
                SortedRegions = new ObservableCollection<Models.Region>(Regions);
            }
        }

        private void OnAdd()
        {
            //await Application.Current.MainPage.Navigation.PushAsync(new AddRegionPage());
        }

        private void OnCompare()
        {
        }

        private void OnReporting()
        {
        }

        private void OnStatistics()
        {
        }

        private void OnLogin()
        {
        }

        private void OnSearch()
        {
            ApplySearchFilter();
        }

        private void ApplySearchFilter()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                SortedRegions = new ObservableCollection<Models.Region>(Regions);
            }
            else
            {
                SortedRegions = new ObservableCollection<Models.Region>(
                    Regions.Where(r => r.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)).ToList());
            }
        }

        private bool CanAddRegion()
        {
            var currentUser = CurrentUser.Current;
            return currentUser != null && currentUser.Role.Name == "Admin";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}