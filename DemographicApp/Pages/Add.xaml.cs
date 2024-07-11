using DemographicApp.ViewModels;

namespace DemographicApp.Pages;

public partial class Add : ContentPage
{
	public Add()
	{
		InitializeComponent();
        BindingContext = Resources["ViewModel"] as ViewModels.AddRegionView;
    }
}