using DemographicApp.Pages;
using DemographicApp.ViewModels;

namespace DemographicApp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }
        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            if (BindingContext is MainPageView viewModel)
            {
                viewModel.SearchText = e.NewTextValue;
            }
        }
        private async void OnLoginButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Login());
        }
    }
}