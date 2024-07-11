using DemographicApp.ViewModels;

namespace DemographicApp.Pages;

public partial class CreateAdmin : ContentPage
{
    public event EventHandler AdminCreated;

    public CreateAdmin()
    {
        InitializeComponent();
        var viewModel = new AdminRegistrationViewModel();
        viewModel.PropertyChanged += ViewModel_PropertyChanged;
        BindingContext = viewModel;
    }

    private async void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AdminRegistrationViewModel.ErrorMessage))
        {
            var viewModel = (AdminRegistrationViewModel)sender;
            if (viewModel.ErrorMessage == "Администратор успешно зарегистрирован.")
            {
                AdminCreated?.Invoke(this, EventArgs.Empty);

                await Application.Current.MainPage.Navigation.PushAsync(new MainPage());
            }
        }
    }
}