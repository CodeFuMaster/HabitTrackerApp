using HabitTrackerApp.Maui.ViewModels;

namespace HabitTrackerApp.Maui.Views;

public partial class LoginPage : ContentPage
{
    private readonly LoginViewModel _viewModel;

    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // Clear any sensitive data when the page appears
        if (!_viewModel.RememberMe)
        {
            _viewModel.Password = string.Empty;
            _viewModel.ConfirmPassword = string.Empty;
        }
    }
}