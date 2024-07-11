using DemographicApp.ViewModels;

namespace DemographicApp.Pages;

public partial class Registration : ContentPage
{
	public Registration()
	{
        InitializeComponent();
        BindingContext = new RegistrationViewModel();
    }
}